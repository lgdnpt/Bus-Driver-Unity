//------------------------------------------------------------------------------------------------
// Edy's Vehicle Physics
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Serialization;
using System;

namespace EVP {

// TO-DO: raise to Serializable and add a common property "Mode Key" for each mode
public class CameraMode {
	// Adjust the public values for the given configuration
	public virtual void SetViewConfig(VehicleViewConfig viewConfig) { }

	// Called one time when the host camera controller is enabled
	public virtual void Initialize(Transform self) { }

	// Called when the camera mode is enabled
	public virtual void OnEnable(Transform self, Transform target, Vector3 targetOffset) { }

	// Reset the values for the given target
	public virtual void Reset(Transform self, Transform target, Vector3 targetOffset) { }

	// Do the camera control stuff
	public virtual void Update(Transform self, Transform target, Vector3 targetOffset) { }

	// Called when the camera mode is disabled
	public virtual void OnDisable(Transform self, Transform target, Vector3 targetOffset) { }

	// Utility method for getting the input for a given axis
	public static float GetInputForAxis (string axisName) {
		return string.IsNullOrEmpty(axisName)? 0.0f : Input.GetAxis(axisName);
	}
}


// Fixed camera mode
//驾驶室视角
[Serializable]
public class CameraAttachTo : CameraMode {
	public Transform attachTarget;


	public override void SetViewConfig (VehicleViewConfig viewConfig) {
		attachTarget = viewConfig.driverView;
	}


	public override void Update (Transform self, Transform target, Vector3 targetOffset) {
		if (attachTarget != null) target = attachTarget;
		if (target == null) return;

		self.position = target.position;
        self.rotation = target.rotation;
	}
}

// Smooth follow camera mode
//自动跟随视角
[Serializable]
public class CameraSmoothFollow : CameraMode {
	public float distance = 10.0f;
	public float height = 5.0f;
	public float viewHeightRatio = 0.5f;      // Look above the target (height * this ratio)
	[Space(5)]
	public float heightDamping = 2.0f;
	public float rotationDamping = 3.0f;
	[Space(5)]
	public bool followVelocity = true;
	public float velocityDamping = 5.0f;


	Vector3 m_smoothLastPos = Vector3.zero;
	Vector3 m_smoothVelocity = Vector3.zero;
	float m_smoothTargetAngle = 0.0f;


	public override void SetViewConfig (VehicleViewConfig viewConfig) {
		distance = viewConfig.viewDistance;
		height = viewConfig.viewHeight;
		rotationDamping = viewConfig.viewDamping;
	}


	public override void Reset (Transform self, Transform target, Vector3 targetOffset) {
		if (target == null) return;

		m_smoothLastPos = target.position + targetOffset;
		m_smoothVelocity = target.forward * 2.0f;
		m_smoothTargetAngle = target.eulerAngles.y;
	}


	public override void Update (Transform self, Transform target, Vector3 targetOffset) {
		if (target == null) return;

		Vector3 updatedVelocity = (target.position + targetOffset - m_smoothLastPos) / Time.deltaTime;
		m_smoothLastPos = target.position + targetOffset;

		updatedVelocity.y = 0.0f;

		if (updatedVelocity.magnitude > 1.0f) {
			m_smoothVelocity = Vector3.Lerp(m_smoothVelocity, updatedVelocity, velocityDamping * Time.deltaTime);
			m_smoothTargetAngle = Mathf.Atan2(m_smoothVelocity.x, m_smoothVelocity.z) * Mathf.Rad2Deg;
		}

		if (!followVelocity)
			m_smoothTargetAngle = target.eulerAngles.y;

		float wantedHeight = target.position.y + targetOffset.y + height;
		float currentRotationAngle = self.eulerAngles.y;
		float currentHeight = self.position.y;

		currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, m_smoothTargetAngle, rotationDamping * Time.deltaTime);
		currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);

		self.position = target.position + targetOffset;
		self.position -= currentRotation * Vector3.forward * distance;

		Vector3 t = self.position;
		t.y = currentHeight;
		self.position = t;

		self.LookAt(target.position + targetOffset + Vector3.up * height * viewHeightRatio);
	}
}


// Mouse orbit camera mode
//鼠标环视视角
[Serializable]
public class CameraMouseOrbit : CameraMode {
	public float distance = 10.0f;
	[Space(5)]
	public float minVerticalAngle = -20.0f;
	public float maxVerticalAngle = 80.0f;
	public float horizontalSpeed = 5f;
	public float verticalSpeed = 2.5f;
	public float orbitDamping = 4.0f;
	[Space(5)]
	public float minDistance = 5.0f;
	public float maxDistance = 50.0f;
	public float distanceSpeed = 10.0f;
	public float distanceDamping = 4.0f;
	[Space(5)]
	public string horizontalAxis = "Mouse X";
	public string verticalAxis = "Mouse Y";
	public string distanceAxis = "Mouse ScrollWheel";


	private float m_orbitX = 0.0f;
	private float m_orbitY = 0.0f;
	private float m_orbitDistance;


	public override void SetViewConfig (VehicleViewConfig viewConfig) {
		distance = viewConfig.viewDistance;
		minDistance = viewConfig.viewMinDistance;
		minVerticalAngle = viewConfig.viewMinHeight;
	}


	public override void Initialize (Transform self) {
		m_orbitDistance = distance;

		Vector3 angles = self.eulerAngles;
		m_orbitX = angles.y;
		m_orbitY = angles.x;
	}


	public override void Update (Transform self, Transform target, Vector3 targetOffset) {
		if (target == null) return;
        if (Input.GetKey(GlobalClass.Instance.cameraKey.lockView)) return;
		m_orbitX += GetInputForAxis(horizontalAxis) * horizontalSpeed;
		m_orbitY -= GetInputForAxis(verticalAxis) * verticalSpeed;
		distance -= GetInputForAxis(distanceAxis) * distanceSpeed;

		m_orbitY = Mathf.Clamp(m_orbitY, minVerticalAngle, maxVerticalAngle);
		distance = Mathf.Clamp(distance, minDistance, maxDistance);

		m_orbitDistance = Mathf.Lerp(m_orbitDistance, distance, distanceDamping * Time.deltaTime);
		self.rotation = Quaternion.Slerp(self.rotation, Quaternion.Euler(m_orbitY, m_orbitX, 0), orbitDamping * Time.deltaTime);
		self.position = target.position + targetOffset + self.rotation * new Vector3(0.0f, 0.0f, -m_orbitDistance);
	}
}


// Look-at camera mode
//cctv视角
[Serializable]
public class CameraLookAt : CameraMode {
	public float damping = 6.0f;
	[Space(5)]
	public float minFov = 10.0f;
	public float maxFov = 60.0f;
	public float fovSpeed = 20.0f;
	public float fovDamping = 4.0f;
	public bool autoFov = false;
	public string fovAxis = "Mouse ScrollWheel";
	[Space(5)]
	public bool enableMovement = false;
	public float movementSpeed = 2.0f;
	public float movementDamping = 5.0f;
	public string forwardAxis = "";
	public string sidewaysAxis = "";
	public string verticalAxis = "";


	Camera m_camera;
	Vector3 m_position;
	float m_fov = 0.0f;
	float m_savedFov = 0.0f;


	public override void Initialize (Transform self) {
		m_camera = self.GetComponentInChildren<Camera>();
	}


	public override void OnEnable (Transform self, Transform target, Vector3 targetOffset) {
		m_position = self.position;

		if (m_camera != null) {
			m_fov = m_camera.fieldOfView;
			m_savedFov = m_camera.fieldOfView;
		}
	}


	public override void Update (Transform self, Transform target, Vector3 targetOffset) {
		// Position

		if (enableMovement) {
			float stepSize = movementSpeed * Time.deltaTime;

			m_position += GetInputForAxis(forwardAxis) * stepSize * new Vector3(self.forward.x, 0.0f, self.forward.z).normalized;
			m_position += GetInputForAxis(sidewaysAxis) * stepSize * self.right;
			m_position += GetInputForAxis(verticalAxis) * stepSize * self.up;
		}

		self.position = Vector3.Lerp(self.position, m_position, movementDamping * Time.deltaTime);

		// Rotation

		if (target != null) {
			Quaternion lookAtRotation = Quaternion.LookRotation(target.position + targetOffset - self.position);
			self.rotation = Quaternion.Slerp(self.rotation, lookAtRotation, damping * Time.deltaTime);
		}

		// Zoom

		if (m_camera != null) {
			m_fov -= GetInputForAxis(fovAxis) * fovSpeed;
			m_fov = Mathf.Clamp(m_fov, minFov, maxFov);
			m_camera.fieldOfView = Mathf.Lerp(m_camera.fieldOfView, m_fov, fovDamping * Time.deltaTime);
		}
	}


	public override void OnDisable (Transform self, Transform target, Vector3 targetOffset) {
		if (m_camera != null)
			m_camera.fieldOfView = m_savedFov;
	}
}


// Camera controller
public class VehicleCameraController : MonoBehaviour {
	public enum Mode { AttachTo, SmoothFollow, MouseOrbit, LookAt };
	public Mode mode = Mode.SmoothFollow;

	public Transform target;
	public bool followCenterOfMass = true;
	//public KeyCode changeCameraKey = KeyCode.C;

	public CameraAttachTo attachTo = new CameraAttachTo();
	[FormerlySerializedAs("smoothFollowSettings")]
	public CameraSmoothFollow smoothFollow = new CameraSmoothFollow();
	[FormerlySerializedAs("orbitSettings")]
	public CameraMouseOrbit mouseOrbit = new CameraMouseOrbit();
	public CameraLookAt lookAt = new CameraLookAt();


	Transform m_transform;
	Mode m_prevMode;
	CameraMode[] m_cameraModes = new CameraMode[0];

	Transform m_prevTarget;
	Rigidbody m_targetRigidbody;
	Vector3 m_localTargetOffset;
	Vector3 m_targetOffset;


	void OnEnable () {
		m_transform = GetComponent<Transform>();

		// Initialize the array with the camera modes.
		// Must be the same length as the Mode enum.

		m_cameraModes = new CameraMode[] {
			attachTo, smoothFollow, mouseOrbit, lookAt
		};

		// Initialize all modes

		foreach (CameraMode cam in m_cameraModes)
			cam.Initialize(m_transform);

		// Adquire the target and its rigidbody if specified/available

		AdquireTarget();
		ComputeTargetOffset();
		m_prevTarget = target;

		// Enable current mode

		m_cameraModes[(int)mode].OnEnable(m_transform, target, m_targetOffset);
		m_cameraModes[(int)mode].Reset(m_transform, target, m_targetOffset);
		m_prevMode = mode;
	}


	void OnDisable () {
		m_cameraModes[(int)mode].OnDisable(m_transform, target, m_targetOffset);
	}


	void LateUpdate () {
		// Target changed?

		if (target != m_prevTarget) {
			AdquireTarget();
			m_prevTarget = target;
		}

		ComputeTargetOffset();

		// 1-驾驶室视角
        // 2-车外环视视角
        // 3-跟随视角
        // 4-固定视角

		//if (Input.GetKeyDown(changeCameraKey))
		//	NextCameraMode();
            
        if (Input.GetKeyDown(GlobalClass.Instance.cameraKey.cameraFirst)) {
            if (enabled) { 
                mode = Mode.AttachTo;
				//self.rotation = GlobalClass.Instance.player.transform.rotation;
				GlobalClass.Instance.player.SetVisible(false);
            }
        }
        if (Input.GetKeyDown(GlobalClass.Instance.cameraKey.cameraOrbit)) {
            if (enabled) { 
                mode = Mode.MouseOrbit;
                GlobalClass.Instance.player.SetVisible(true);
            }
        }
        if (Input.GetKeyDown(GlobalClass.Instance.cameraKey.cameraFollow)) {
            if (enabled) { 
                mode = Mode.SmoothFollow;
                GlobalClass.Instance.player.SetVisible(true);
            }
        }
        if (Input.GetKeyDown(GlobalClass.Instance.cameraKey.cameraFixed)) {
            if (enabled) {
                mode = Mode.LookAt;
                GlobalClass.Instance.player.SetVisible(true);
            }
        }

        // Camera mode changed?

        if (mode != m_prevMode) {
			// Disable previous mode, then enable new one.
			m_cameraModes[(int)m_prevMode].OnDisable(m_transform, target, m_targetOffset);
			m_cameraModes[(int)mode].OnEnable(m_transform, target, m_targetOffset);
			m_cameraModes[(int)mode].Reset(m_transform, target, m_targetOffset);
			m_prevMode = mode;
		}

		m_cameraModes[(int)mode].Update(m_transform, target, m_targetOffset);
	}
    
	//public void NextCameraMode () {
	//	if (!enabled) return;

	//	mode++;
	//	if ((int)mode >= m_cameraModes.Length)
	//		mode = (Mode)0;
	//}

	public void ResetCamera() {
		if (enabled)
			m_cameraModes[(int)mode].Reset(m_transform, target, m_targetOffset);
	}

	public void SetViewConfig(VehicleViewConfig viewConfig) {
		foreach (CameraMode cam in m_cameraModes)
			cam.SetViewConfig(viewConfig);
	}

	void AdquireTarget () {
		// Get the view configuration if exists and configure the camera modes

		if (target != null) {
			VehicleViewConfig viewConfig = target.GetComponent<VehicleViewConfig>();

			if (viewConfig != null) {
				if (viewConfig.lookAtPoint != null)
					target = viewConfig.lookAtPoint;
				SetViewConfig(viewConfig);
			}
		}

		// Find the rigidbody if exists and get the center of mass
		if (followCenterOfMass && target != null) {
			m_targetRigidbody = target.GetComponent<Rigidbody>();
			m_localTargetOffset = m_targetRigidbody.centerOfMass;
		}
		else {
			m_targetRigidbody = null;
		}

		// Everything ready. Reset the camera for the target.
		ResetCamera();
	}

	void ComputeTargetOffset() {
		if (followCenterOfMass && m_targetRigidbody != null) {
			// centerOfMass is not affected by scale. No need to use TransformVector.
			m_targetOffset = target.TransformDirection(m_localTargetOffset);
		}
		else {
			m_targetOffset = Vector3.zero;
		}
	}
}
}
