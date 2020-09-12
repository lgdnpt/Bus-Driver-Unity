using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerControl : MonoBehaviour {
    public float speed;
    [Header("Player")]
    public string playerName;
    public PersonInfo playerInfo;

    public float maxRayDistance = 5.0f;

    [Header("Renderer")]
    public SkinnedMeshRenderer[] skinnedMeshes;

    [Header("Third Person Character")]
    public bool canControl = true;
    //private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

    [SerializeField] float m_MovingTurnSpeed = 360;
    [SerializeField] float m_StationaryTurnSpeed = 180;
    [SerializeField] float m_JumpPower = 5f;
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 1f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    [SerializeField] float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_GroundCheckDistance = 0.3f;

    public Rigidbody m_Rigidbody;
    public Animator m_Animator;
    bool m_IsGrounded;
    float m_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;

    public CapsuleCollider m_Capsule;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    bool m_Crouching;


    //VehicleExtraInput vehinput;
    GameObject scenectrl;
    private byte cangeton = 0;

    void Start() {
        scenectrl = GlobalClass.Instance.sceneController;
        //vehinput = scenectrl.GetComponent<VehicleExtraInput>();

        // get the transform of the main camera
        m_Cam = Camera.main.transform;

        if(!m_Animator) m_Animator = GetComponent<Animator>();
        if (!m_Rigidbody) m_Rigidbody = GetComponent<Rigidbody>();
        if (!m_Capsule) m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
    }

    Vector3 pos;
    RaycastHit hit;
    void Update() {
        if (canControl) {
            if (Input.GetKey(KeyCode.G)) {
                pos = transform.position;
                pos.y += 1f;
                if (Physics.Raycast(pos, transform.TransformDirection(Vector3.forward), out hit, maxRayDistance)) {
                    if (cangeton == 30) {
                        if (hit.collider.CompareTag("PlayerBus")) {
                            GetOnBus();
                        }
                        cangeton = 0;
                    }
                }
                cangeton++;
            }
            if (Input.GetKeyUp(KeyCode.G))
                cangeton = 0;
        
            if (!m_Jump) {
                m_Jump = Input.GetButtonDown("Jump");
            }
        }
    }

    //bool sit = false;
    private void FixedUpdate() {
        if (canControl) {
            // read inputs
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = v * m_CamForward + h * m_Cam.right;

            //走
            if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;

            // pass all parameters to the character control script
            Move(m_Move, crouch, m_Jump);
            m_Jump = false;
            speed = m_Rigidbody.velocity.magnitude;
        }
    }

    void GetOnBus() {
        //启用车辆下车组件
        EnterVehicle entervehicle = hit.transform.gameObject.GetComponent<EnterVehicle>();
        entervehicle.enabled = true;
        entervehicle.SendMessage("GetOnBus");    //传递上车信息给车辆处理

        //人物坐下
        transform.parent = entervehicle.driverPosition;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        m_Animator.SetBool("Sit", true);
        canControl = false;
        m_Capsule.isTrigger = true;
        m_Rigidbody.isKinematic = true;
    }

    public void SetVisible(bool value) {
        for (int i = 0;i < skinnedMeshes.Length;i++) {
            skinnedMeshes[i].enabled = value;
        }
    }

    //===================================================================
    public void Move(Vector3 move, bool crouch, bool jump) {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        //将世界相对运动输入向量转换为指向所需方向所需的局部相对转动量和前进量。
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        //站在地上和坠落时的控制和速度处理不同：
        HandleMovement(m_IsGrounded, crouch, jump);

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();

        // 传递参数
        UpdateAnimator(move);
    }

    void ScaleCapsuleForCrouching(bool crouch) {
        if (m_IsGrounded && crouch) {
            if (m_Crouching) return;
            m_Capsule.height = m_Capsule.height / 2f;
            m_Capsule.center = m_Capsule.center / 2f;
            m_Crouching = true;
        } else {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
                m_Crouching = true;
                return;
            }
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
            m_Crouching = false;
        }
    }

    void PreventStandingInLowHeadroom() {
        // prevent standing up in crouch-only zones
        if (!m_Crouching) {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
                m_Crouching = true;
            }
        }
    }

    void UpdateAnimator(Vector3 move) {
        // update the animator parameters
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        m_Animator.SetBool("Crouch", m_Crouching);
        m_Animator.SetBool("OnGround", m_IsGrounded);
        if (!m_IsGrounded) {
            m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        //计算哪条腿落后，以便在跳跃动画中保持该腿落后（此代码依赖于动画中特定的运行循环偏移量，
        //并假定一条腿以0.0和0.5的标准化剪辑时间通过另一条腿）。
        float runCycle = Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
        float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
        if (m_IsGrounded) {
            m_Animator.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        //anim倍增 允许在Inspector中调整行走/跑步的整体速度，因为根运动会影响移动速度。
        if (m_IsGrounded && move.magnitude > 0) {
            m_Animator.speed = m_AnimSpeedMultiplier;
        } else {
            m_Animator.speed = 1;
        }
    }

    void HandleMovement(bool grounded, bool crouch, bool jump) {
        if (grounded) {
            // check whether conditions are right to allow a jump:
            if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")) {
                // jump!
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                
                //Vector3 force = new Vector3(m_Move.x, m_JumpPower, m_Move.z);
                //m_Rigidbody.AddForce(force);

                m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        } else {
            // apply extra gravity from multiplier:
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce+m_Move*5);

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        }
    }

    void ApplyExtraTurnRotation() {
        // help the character turn faster (this is in addition to root rotation in the animation)
        //帮助角色更快地旋转（这是动画中除根旋转之外的其他功能）
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }


    public void OnAnimatorMove() {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (m_IsGrounded && Time.deltaTime > 0) {
            Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            //保留当前速度的y分量
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }


    void CheckGroundStatus() {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance)) {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            m_Animator.applyRootMotion = true;
        } else {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;
        }
    }
}
