using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleExtraInput : MonoBehaviour {
    public Text testText;

    public sbyte gear;
    sbyte gear_o;

    public EVP.VehicleController target;
    public enum ControlMode { Classic, Extra };
    public ControlMode controlmode = ControlMode.Extra;

	public Text gearText;

    GlobalClass.VehicleKeyCodes vKeyCode;

	public float brakeInput = 0.0f;
    bool reverse = false;
	float forwardInput = 0.0f;
	float reverseInput = 0.0f;
	float clutchInput = 1.0f;

    public AdvancedVehicleController avc;
    //public AVC.ECU ecu;
    public AdvancedVehicleAudio ava;
    public AdvancedVehicleLight avl;

    public bool reversal = false;

    float speed;
    // Vehicle controls
    [Range(-1, 1)]
    public float steerAxis = 0.0f;
    [Range(0, 1)]
    public float throttleAxis = 0.0f;
    [Range(0, 1)]
    public float brakeAxis = 0.0f;
	[Range(0, 1)]
	public float clutchAxis = 0.0f;

    public bool usingAxis = false;

	void OnEnable() {
		if (gearText == null)
			gearText = GlobalClass.Instance.gearText;

        //是否开启踏板控制器
        if (System.IO.File.Exists(@"D:\Work\usingAxis")) usingAxis = true;
    }

    private void Start() {
        vKeyCode = GlobalClass.Instance.vehicleKey;
    }

    void Update() {
        if (target == null)
            return;
        
        //重置车辆
        if (Input.GetKeyDown(vKeyCode.resetVehicle))
            target.ResetVehicle();

		//手刹开关
        if (Input.GetKeyDown(vKeyCode.handBrake)) {
            if(avc!=null) 
			    if (avc.handbrakeUser>0.9f) {
                    avc.handbrakeOff();
			    } else {
                    avc.handbrakeOn();
			    }
/*            if(ecu!=null)
                if(ecu.handbrakeUser>0.9f) {
                    ecu.handbrakeOff();
                } else {
                    ecu.handbrakeOn();
                }*/
        }

    }

    float minT = -0.84f, minB = -0.9f, minC = -0.83f;
    float maxT = 0.8f, maxB = 0.71f, maxC = 0.8f;
    void checkAxis() {
        //校准输入轴
        if(Input.GetAxis("Throttle") < minT) minT = Input.GetAxis("Throttle");
        if(Input.GetAxis("Brake") < minB) minB = Input.GetAxis("Brake");
        if(Input.GetAxis("Clutch") < minC) minC = Input.GetAxis("Clutch");
        if(Input.GetAxis("Throttle") > maxT) maxT = Input.GetAxis("Throttle");
        if(Input.GetAxis("Brake") > maxB) maxB = Input.GetAxis("Brake");
        if(Input.GetAxis("Clutch") > maxC) maxC = Input.GetAxis("Clutch");
        /*+
        "油门最小" + minT + " 刹车最小" + minB + " 离合最小" + minC + "\n" +
        "油门最大" + maxT + " 刹车最大" + maxB + " 离合最大" + maxC;
         */
    }

    void setAxis() {
        throttleAxis = (Input.GetAxisRaw("Throttle") - minT) / (Mathf.Abs(minT)+maxT);
        brakeAxis = (Input.GetAxisRaw("Brake") - minB) / (Mathf.Abs(minB) + maxB);
        clutchAxis = (-Input.GetAxisRaw("Clutch") - minC) / (Mathf.Abs(minC) + maxC);

        if(Input.GetAxisRaw("Throttle") == 0) throttleAxis = 0;
        if(Input.GetAxisRaw("Brake") == 0) brakeAxis = 0;
        if(Input.GetAxisRaw("Clutch") == 0) clutchAxis = 0;

        if     (Input.GetAxisRaw("Gear_1") > 0.9f) gear = 1;
        else if(Input.GetAxisRaw("Gear_2") > 0.9f) gear = 2;
        else if(Input.GetAxisRaw("Gear_3") > 0.9f) gear = 3;
        else if(Input.GetAxisRaw("Gear_4") > 0.9f) gear = 4;
        else if(Input.GetAxisRaw("Gear_5") > 0.9f) gear = 5;
        else if(Input.GetAxisRaw("Gear_6") > 0.9f) gear = 6;
        else if(Input.GetAxisRaw("Gear_R") > 0.9f) gear = -1;
        else gear = 0;
        

        if(gear != gear_o) {
            if(gear == 0) {
                avc.SendMessage("NoGear");
            }
            else if(avc.clutchInput < 0.1f) avc.SendMessage("setGear",gear);
        } 
            
        gear_o = gear;
    }

    void FixedUpdate() {
        if(target == null)
            return;
        speed = target.speed;

        if(usingAxis) {
            //踏板按键检测
            setAxis();
        } else {
            //油门按键检测
            if(Input.GetKey(vKeyCode.throttle)) {
                if(throttleAxis <= 1f) {
                    if (brakeAxis > 0.1f) {
                        brakeAxis = 0.1f;
                    }
                    //急加速
                    if (Input.GetKey(KeyCode.LeftShift))
                        throttleAxis = 1f;// Mathf.Lerp(throttleAxis,1f,0.08f);
                    else
                        throttleAxis = Mathf.Lerp(throttleAxis,0.5f,0.08f);
                }
            } else {
                if(throttleAxis > 0f)
                    throttleAxis = Mathf.Lerp(throttleAxis,0f,0.5f);
            }
            //刹车按键检测
            if(Input.GetKey(vKeyCode.brake)) {
                if(brakeAxis < 1f) {
                    throttleAxis = 0f;
                    //急刹车
                    if (Input.GetKey(KeyCode.LeftShift))
                        brakeAxis = 1f;
                    else
                        brakeAxis = Mathf.Lerp(brakeAxis, 0.6f, 0.08f);
                }
            } else {
                if(brakeAxis > 0f)
                    brakeAxis = Mathf.Lerp(brakeAxis,0f,0.8f);

            }
            //离合按键检测
                if(Input.GetKey(vKeyCode.clutch)) {
                    if(clutchAxis > 0)
                        clutchAxis = Mathf.Lerp(clutchAxis,0,0.2f);
                } else {
                    if(clutchAxis < 1)
                        clutchAxis = Mathf.Lerp(clutchAxis,1,0.02f);
                }
        }
        
        //转弯按键检测
        if (Input.GetKey(vKeyCode.steerLeft)) {
            //按下按键
            if (steerAxis <= 0 && steerAxis > -1) {
                //在左边
                if (speed < 20)
                    steerAxis = Mathf.Lerp(steerAxis, -1.2f + Mathf.Abs(speed) / 20, 0.018f);
                else
                    steerAxis = Mathf.Lerp(steerAxis, -0.2f, 0.018f);
            }
            else {
                //在右边，快速向左回正
                if (steerAxis > 0)
					steerAxis = Mathf.Lerp(steerAxis, -1, 0.02f);
			}
        }
        else {
            //未按，根据速度回正
            if (steerAxis < 0) {
				steerAxis = Mathf.Lerp(steerAxis, 0.0f, Mathf.Abs(speed) / 300);
			}
        }
        //===
        if (Input.GetKey(vKeyCode.steerRight)) {
            if (steerAxis >= 0 && steerAxis < 1) {
                if(speed<20)
				    steerAxis = Mathf.Lerp(steerAxis, 1.2f - Mathf.Abs(speed) / 20, 0.018f);
                else
                    steerAxis = Mathf.Lerp(steerAxis, 0.2f, 0.018f);
            }
            else {
                if (steerAxis < 0)
					steerAxis = Mathf.Lerp(steerAxis, 1, 0.02f);
			}
        }
        else {
            if (steerAxis > 0) {
				steerAxis = Mathf.Lerp(steerAxis, -0.0f, Mathf.Abs(speed) / 300);
			}
        }
		
        // Read the user input
        float steerInput = Mathf.Clamp(steerAxis, -1.0f, 1.0f);

        forwardInput = Mathf.Clamp01(throttleAxis);
        reverseInput = Mathf.Clamp01(brakeAxis);
		clutchInput = Mathf.Clamp01(clutchAxis);


		// 计算前后输入值给车辆 Translate forward/reverse to vehicle input

		float throttleInput = 0.0f;
		
        if (!reverse) {
            throttleInput = forwardInput;
            brakeInput = reverseInput;
        }
        else {
            throttleInput = -forwardInput;
            brakeInput = reverseInput;
        }

		// 应用到车辆
		if (avc == null) {
/*            if(ecu!=null) {
                target.steerInput = steerInput;
                ecu.throttleInput = throttleInput;
                ecu.brakeInput = brakeInput;

            } else {
            }*/
            target.steerInput = steerInput;
            target.throttleInput = throttleInput;
            target.brakeInput = brakeInput;
        }
		else {
			target.steerInput = steerInput;
			avc.throttleInput = throttleInput;
			avc.brakeInput = brakeInput;
		}

    }
}
