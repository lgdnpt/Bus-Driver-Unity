//------------------------------------------------------------------------------------------------
// Edy's Vehicle Physics
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//------------------------------------------------------------------------------------------------

using UnityEngine;

namespace EVP
{

public class VehicleStandardInput : MonoBehaviour
	{
	    public VehicleController target;
        public enum ControlMode { Classic, Extra };
        public ControlMode controlmode = ControlMode.Extra;

        public bool continuousForwardAndReverse = true;

        public enum ThrottleAndBrakeInput { SingleAxis, SeparateAxes };
	    public ThrottleAndBrakeInput throttleAndBrakeInput = ThrottleAndBrakeInput.SingleAxis;

	    public string steerAxis = "Horizontal";
	    public string throttleAndBrakeAxis = "Vertical";
	    public string throttleAxis = "Fire2";
	    public string brakeAxis = "Fire3";
	    public string handbrakeAxis = "Jump";
	    public KeyCode resetVehicleKey = KeyCode.Return;

        public KeyCode ThrottleKey = KeyCode.W;
        public KeyCode BreakKey = KeyCode.S;
        public KeyCode LeftKey = KeyCode.A;
        public KeyCode RightKey = KeyCode.D;
        public float brakeInput = 0.0f;

	    bool m_doReset = false;
        public bool reverse = false;
        bool isButtonDown = false;


        void OnEnable ()
		{
		// Cache vehicle

		if (target == null)
			target = GetComponent<VehicleController>();
		}


	void Update() {
		if (target == null) return;

		if (Input.GetKeyDown(resetVehicleKey)) m_doReset = true;


        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isButtonDown) {
            isButtonDown = true;
            reverse = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftAlt) && isButtonDown) {
            isButtonDown = false;
            reverse = false;
        }
    }

    void OnGUI() {
            //if (reverse)
            //    GUI.Label(new Rect(Screen.width - (Screen.width / 1.7f), Screen.height - (Screen.height / 1.2f), 800, 100), "倒车");
            if (!reverse) {
                GUI.Label(new Rect(Screen.width - (Screen.width / 1.7f), Screen.height - (Screen.height / 1.2f), 800, 100), "前进");
            }
            else {
                GUI.Label(new Rect(Screen.width - (Screen.width / 1.7f), Screen.height - (Screen.height / 1.2f), 800, 100), "倒车");
            }
        }
	void FixedUpdate ()
		{
		if (target == null) return;

		// Read the user input

		float steerInput = Mathf.Clamp(Input.GetAxis(steerAxis), -1.0f, 1.0f);
		float handbrakeInput = Mathf.Clamp01(Input.GetAxis(handbrakeAxis));

		float forwardInput = 0.0f;
		float reverseInput = 0.0f;

		if (throttleAndBrakeInput == ThrottleAndBrakeInput.SeparateAxes)
			{
			forwardInput = Mathf.Clamp01(Input.GetAxis(throttleAxis));
			reverseInput = Mathf.Clamp01(Input.GetAxis(brakeAxis));
			}
		else
			{
			forwardInput = Mathf.Clamp01(Input.GetAxis(throttleAndBrakeAxis));
			reverseInput = Mathf.Clamp01(-Input.GetAxis(throttleAndBrakeAxis));
			}

		// Translate forward/reverse to vehicle input

		float throttleInput = 0.0f;
		brakeInput = 0.0f;

		if (continuousForwardAndReverse)
			{
			float minSpeed = 0.1f;
			float minInput = 0.1f;

			if (target.speed > minSpeed)
				{
				throttleInput = forwardInput;
				brakeInput = reverseInput;
				}
			else
				{
				if (reverseInput > minInput)
					{
					throttleInput = -reverseInput;
					brakeInput = 0.0f;
					}
				else
				if (forwardInput > minInput)
					{
					if (target.speed < -minSpeed)
						{
						throttleInput = 0.0f;
						brakeInput = forwardInput;
						}
					else
						{
						throttleInput = forwardInput;
						brakeInput = 0;
						}
					}
				}
			}
		else
			{
                //bool reverse = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                //bool reverse = false;
                


			    if (!reverse){
				    throttleInput = forwardInput;
				    brakeInput = reverseInput;
				}
			    else{
                    //throttleInput = -reverseInput;
                    //brakeInput = 0;
                    throttleInput = -forwardInput;
                    brakeInput = reverseInput;
                }
			}

		// Apply input to vehicle

		//if (target.speed > 0.1f)
			//target.steerInput = Mathf.Clamp(target.speedAngle / target.maxSteerAngle * 0.5f + steerInput, -1.0f, +1.0f);
		//else
			target.steerInput = steerInput;

		target.throttleInput = throttleInput;
		target.brakeInput = brakeInput;
		target.handbrakeInput = handbrakeInput;

		// Do a vehicle reset

		if (m_doReset)
			{
			target.ResetVehicle();
			m_doReset = false;
			}
		}
	}
}