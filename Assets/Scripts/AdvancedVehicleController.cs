using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharpConfig;
using System;
using System.IO;
using AVC;

public class AdvancedVehicleController : MonoBehaviour {
    /// <summary>
    /// 车辆配置文件
    /// </summary>
    [Serializable]
    public class Vehicle {
        public string name;
        public string description;
        
        public ControlType type=ControlType.Engine;

        public string engine;
        public string motor;
        public string transmission;

        public float mainRatio;

        public int sitCapacity;
        public int standCapacity;

        public FuelType fuelType;
        public float fuel;
        public float fuelCapacity;

        public float battery;
        public BatteryType batteryType;
        public float batteryVoltage;
        public float batteryCapacity;

        public float powerBattery;
        public BatteryType powerBatteryType;
        public float powerBatteryVoltage;
        public float powerBatteryCapacity;

        public bool airBreak;
        public float airPressure;
        public float airBreakCapacity;
        public bool retarder;

        public string wheelFront;
        public string wheelBack;
    }

    /// <summary>
    /// 发动机类
    /// </summary>
    [Serializable]
    public class Engine {
        public string name;
        public string description;

        public float torqueMax;
        public float powerMax;
        [Space(5)]
        public EngineState state = 0;  //0=熄火 1=正在启动  2=运行  -1=正在熄火  ?=损坏
        public float rpm;
        public float rpmE;
        public float rpmMax;
        public float rpmMin;
        [Space(5)]
        public float torque;
        public float power;
        public float force;
        public float ge;

        [Space(5)]
        [Range(0,1)]
        public float throttle = 0.0f;//发动机自控油门
        [Range(0, 1)]
        public float clutch = 1.0f;  //发动机出轴离合器

        [Space(5)]
        public AnimationCurve torqueCurve;
        public AnimationCurve geCurve;
        public AnimationCurve geTorqueCurve;

        //[Space(5)]
        //public EngineFan engineFan;
        
        public void Update() {

        }

        public void Load(string enginePath) {
            Configuration engineConfig = Configuration.LoadFromFile(GlobalClass.GetGamePath() + enginePath);
            name = engineConfig["Engine"]["name"].StringValue;
            description = engineConfig["Engine"]["description"].StringValue;
            
            torqueMax = engineConfig["Engine"]["torque"].FloatValue;
            powerMax = engineConfig["Engine"]["power"].FloatValue;

            rpmMax = engineConfig["Engine"]["rpmMax"].FloatValue;
            rpmMin = engineConfig["Engine"]["rpmMin"].FloatValue;
        }
    }
    
    /// <summary>
    /// 电动机配置
    /// </summary>
    [Serializable]
    public class Motor {
        public string name;
        public string description;

        public float torqueMax;
        public float powerMax;
        public float rpmMax;

        public float selfRatio;

        public AnimationCurve torqueCurve;

        public float rpm;
        public float torque;
        public float power;
        public float force;

        public void Update() {
            
        }

        public void Load(string motorPath) {
            Configuration motorConfig = Configuration.LoadFromFile(GlobalClass.GetGamePath() + motorPath);
            if (!motorConfig["Motor"]["name"].IsEmpty) name = motorConfig["Motor"]["name"].StringValue;
            if (!motorConfig["Motor"]["description"].IsEmpty) description = motorConfig["Motor"]["description"].StringValue;

            if (!motorConfig["Motor"]["torque"].IsEmpty) torqueMax = motorConfig["Motor"]["torque"].FloatValue;
            if (!motorConfig["Motor"]["power"].IsEmpty) powerMax = motorConfig["Motor"]["power"].FloatValue;
            if (!motorConfig["Motor"]["rpmMax"].IsEmpty) rpmMax = motorConfig["Motor"]["rpmMax"].FloatValue;
            if (!motorConfig["Motor"]["selfRatio"].IsEmpty) selfRatio = motorConfig["Motor"]["selfRatio"].FloatValue;

            if (motorConfig["Motor"].Contains("torqueCurve")) {
                torqueCurve = GlobalClass.LoadCurve(motorConfig, "Motor", "torqueCurve");
            }
        }
    }

    public class Battery {
        public float value;
        public BatteryType type;
        public float voltage;
        public float capacity;
    }


    /// <summary>
    /// 变速器配置文件
    /// </summary>
    [Serializable]
    public class Transmission {
        public string name;
        public string description;
        public TransType type;
        public float[] forward;
        public float reverse;

        public Gear gear = Gear.N;

        public void Load(string transmissionPath) {
            Configuration myConfig = Configuration.LoadFromFile(GlobalClass.GetGamePath() + transmissionPath);

            name = myConfig["Transmission"]["name"].StringValue;
            description = myConfig["Transmission"]["description"].StringValue;
            type = myConfig["Transmission"]["type"].GetValue<TransType>();
            forward = myConfig["Transmission"]["forward"].FloatValueArray;
            reverse = myConfig["Transmission"]["reverse"].FloatValue;
        }
    }

    GlobalClass.VehicleKeyCodes veKeyCode;

    public enum EngineState:sbyte { OFF = 0, Starting = 1, Running = 2, Cutting = -1, Broken = -100 }; //0=熄火 1=正在启动 2=运行 -1=正在熄火 -100=损坏
    public enum VehicleState:sbyte { OFF = 0, Starting = 1, ON = 2, Broken = -100 };        //0=电源关闭 1=正在启动 2=电源开启 -100=损坏
    public enum Gear:sbyte {
        P = -128,
        R3 = -13, R2 = -12, R1 = -11,
        R = -10,
        N = 0,
        M1 = 1, M2 = 2, M3 = 3, M4 = 4, M5 = 5, M6 = 6, M7 = 7, M8 = 8, M9 = 9, M10 = 10, M11 = 11, M12 = 12,
        D = 20, S=21, L=22,
        C = 50, C1 = 51, C2 = 52, C3 = 53, C4 = 54,
        H1 = 71, H2= 72, H3 = 73, H4 = 74,
        A = 127
    };

    public enum ControlType:sbyte { Engine=0, EV=1, HEV_EATON =2, HEV_CEP=3};
    public enum TransType:sbyte { MT=0, AT=1, AMT=2, CVT=3, DCT=4, EV=5, AMT_EATON=110 };

    
    public VehicleExtraInput inputManager;
    public VehicleState vehicleState = VehicleState.OFF;
    public Gear mainGear = Gear.N;
    [Header("Vehicle")]
    public string vehicleName = "";
    public Vehicle vehicle;
    
    [Header("Engine")]
    public Engine engine;

    [Header("Motor")]
    public Motor motor;

    [Header("Transmission")]
    public Transmission trans;

    [Header("Brake")]
    public Retarder retarder;

    [Header("Vehicle Control")]
	public EVP.VehicleController target;
	public AdvancedVehicleAudio ava;
    public EnterVehicle vehEntrance;
	[Range(-1, 1)]
	public float steerInput = 0.0f;
	[Range(-1, 1)]
	public float throttleInput = 0.0f;
	[Range(0, 1)]
	public float brakeInput = 0.0f;
    //[Range(0,1)]
    //public float engineBrakeInput = 0.0f;
    [Range(0, 1)]
    public float handbrakeVehicle = 0.0f;
    [Range(0, 1)]
    public float handbrakeUser = 0.0f;
    [Range(0, 1)]
	public float clutchInput = 0.0f;   //主离合器

    
    [Header("Runtime")]
    public float radius = 0.480f;

    public float transRatio;//驱动比例
    /// <summary>
    /// 变速箱与驱动轴之间的轴转速
    /// </summary>
    public float rpmWheel;
    /// <summary>
    /// 变速箱与发动机之间的轴转速
    /// </summary>
    public float rpmAxle;
    /// <summary>
    /// 动力系统的轴转速
    /// </summary>
    public float rpmPower;
    public float rpmPowerUp;
    public float rpmPowerDown;

    public float fuelConsume;
    float fuelConsumeF;
    public float fuelConsumeAdv = 0;
    public float fuelConsumeTotal = 0;
    public float sTotal = 0;
    bool compressorStarted = false;

    [Header("HEV")]
    public bool gearLock = false;

    //public float motorI;
    //public float motorW;
    const float G = 9.794f;

    float real_Ek, real_dEk, real_dEp, real_dEk_c;
    float mass, v1 = 0, v2, h1;
    float rec_kWh;


    [ContextMenu("输出发动机扭矩关键帧信息")]
    void printTorqueCurveKeys() {
        for(byte i = 0;i < engine.torqueCurve.keys.Length;i++) {
            print("关键帧:" + i);
            print("time:" + engine.torqueCurve.keys[i].time);
            print("value:" + engine.torqueCurve.keys[i].value);
            print("inTangent:" + engine.torqueCurve.keys[i].inTangent);
            print("outTangent:" + engine.torqueCurve.keys[i].outTangent);
            print("inWeight:" + engine.torqueCurve.keys[i].inWeight);
            print("outWeight:" + engine.torqueCurve.keys[i].outWeight);
        }
    }


    void Start () {
        //防止null
		if (!target) target = GetComponent<EVP.VehicleController>();
		if (!ava) ava = GetComponent<AdvancedVehicleAudio>();
        if (!vehEntrance) vehEntrance = GetComponent<EnterVehicle>();
        if (retarder) {
            retarder.avc = this;
        }

        inputManager = GlobalClass.Instance.InputManager;
        veKeyCode = GlobalClass.Instance.vehicleKey;
        
        //临时代码
        mass = GetComponent<Rigidbody>().mass;

        switch (vehicle.type) {
            case ControlType.Engine:
                if (rpmPowerUp < 10) rpmPowerUp = engine.rpmMax * 0.85f;
                if (rpmPowerDown < 10) rpmPowerDown = engine.rpmMin * 1.3f;
                break;
            case ControlType.EV:
                rpmPowerUp = motor.rpmMax;
                rpmPowerDown = 0;
                break;
            case ControlType.HEV_EATON:
                if (rpmPowerUp < 10) rpmPowerUp = motor.rpmMax * 0.85f;
                if (rpmPowerDown < 10) rpmPowerDown = engine.rpmMin * 1.3f;
                break;
            case ControlType.HEV_CEP:
                if (rpmPowerUp < 10) rpmPowerUp = motor.rpmMax * 0.85f;
                if (rpmPowerDown < 10) rpmPowerDown = engine.rpmMin * 1.3f;
                break;
        }

    }
	
	/// <summary>
    /// 画面帧更新
    /// </summary>
	void Update () {
		switch (vehicle.type) {
			case ControlType.Engine:
                control_Engine();
				break;
			case ControlType.EV:
                control_EV();
				break;
            case ControlType.HEV_EATON:
                control_HEV(ControlType.HEV_EATON);
                break;
            case ControlType.HEV_CEP:
                control_HEV(ControlType.HEV_CEP);
                break;
        }
        engine.rpm = (float)Math.Round(engine.rpm, 2);
        motor.rpm = (float)Math.Round(motor.rpm, 2);
    }

	/// <summary>
	/// 电动车控制
	/// </summary>
	void control_EV() {
        if (vehEntrance.PlayerInVehicle) {
            if (vehicleState == VehicleState.ON) {
                doMotorUpdate(motor);
                doTransUpdate(TransType.EV);
                target.maxDriveForce = motor.force;

                //关闭电源
                if (Input.GetKeyDown(veKeyCode.insertKey)) {
                    StartCoroutine(GlobalClass.DelayToInvokeDo(() => {
                        vehicleState = VehicleState.OFF;
                        GlobalClass.Instance.uiShowInputs.powerSwitch(false);
                        motor.rpm = 0;
                        throttleInput = 0;
                        motor.force = 0;
                        target.maxDriveForce = 0;
                    }, 0.5f)); 
                }
            }

            //开启电源
            if (vehicleState == VehicleState.OFF) {
                if (Input.GetKeyDown(veKeyCode.insertKey)) {
                    vehicleState = VehicleState.Starting;
                    //ava.SendMessage("DoPowerOn");
                    ava.DoPowerOn();
                    StartCoroutine(GlobalClass.DelayToInvokeDo(() => {
                        vehicleState = VehicleState.ON;
                        GlobalClass.Instance.uiShowInputs.powerSwitch(true);
                    }, 1.0f));

                }
            }
        }
        //档位换算
        motor.rpm = Mathf.Abs(ava.m_wheelsRpm) * vehicle.mainRatio * transRatio * motor.selfRatio;
        rpmPower = motor.rpm;

        setTargetInput();
        selectBrake(true);
    }
    void doMotorUpdate(Motor m) {
        //能量的消耗和再生
        if (vehicle.powerBattery > 0) {
            vehicle.powerBattery -= m.power * Time.deltaTime / (3.6f * vehicle.powerBatteryVoltage);
            m.torque = m.torqueCurve.Evaluate(m.rpm) * throttleInput;
            if (mainGear == Gear.N) m.torque = 0;
        }
        else {
            //没电了
            vehicle.powerBattery = 0;
            motor.torque = 0;
        }

        m.power = m.torque * m.rpm / 9549.297f;
        m.force = m.torque * vehicle.mainRatio * transRatio * m.selfRatio * 0.9f / radius;

        real_dEk = 0.5f * mass * (target.speed + v1) * (target.speed - v1);
        real_dEp = mass * G * (transform.position.y - h1);
        real_dEk_c = real_dEk + real_dEp;

        //能量回收效率50%
        if (brakeInput > 0) {
            vehicle.powerBattery += Mathf.Abs(real_dEk_c / (3600 * vehicle.powerBatteryVoltage) * brakeInput * 0.5f);
        }
        v1 = target.speed;
        h1 = transform.position.y;
    }

    /// <summary>
    /// 混动车控制
    /// </summary>
    void control_HEV(ControlType type) {
        if (vehEntrance.PlayerInVehicle) {
            switch (vehicleState) {
                case VehicleState.OFF: {
                    //按`开启电源
                    if (Input.GetKeyDown(veKeyCode.insertKey)) {
                        vehicleState = VehicleState.Starting;
                        ava.DoPowerOn();
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => {
                            vehicleState = VehicleState.ON;
                            GlobalClass.Instance.uiShowInputs.powerSwitch(true);
                        }, 1.0f));
                    }
                    if (engine.state == EngineState.Running) engine.state = EngineState.Cutting;
                    break;
                }
                case VehicleState.ON: {
                    //关闭电源
                    if (Input.GetKeyDown(veKeyCode.insertKey)) {
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => {
                            vehicleState = VehicleState.OFF;
                            GlobalClass.Instance.uiShowInputs.powerSwitch(false);
                            motor.rpm = 0;
                            throttleInput = 0;
                            engine.force = 0;
                            motor.force = 0;
                            target.maxDriveForce = 0;
                        }, 0.5f));
                    }
                    doMotorUpdate(motor);

                    //电源开启时可用功能
                    switch (type) {
                        case ControlType.HEV_EATON: {
                            rpmPower = motor.rpm;
                            doTransUpdate(TransType.AMT);

                            if (trans.gear == Gear.M1) {
                                engine.clutch = 0;
                                if (engine.state == EngineState.Running && vehicle.airPressure > 7.9f && vehicle.powerBattery > 5) engine.state = EngineState.Cutting;
                            }

                            if ((byte)trans.gear >= 2 && engine.state == EngineState.OFF) {
                                engine.state = EngineState.Starting;
                                engine.clutch = 1;
                            }

                            if (vehicle.airPressure < 4 && engine.state == EngineState.OFF) {
                                engine.state = EngineState.Starting;
                            }

                            break;
                        }
                        case ControlType.HEV_CEP: {
                            rpmPower = motor.rpm;
                            doTransUpdate(TransType.EV);

                            if (target.speed > 5f) {
                                engine.clutch = Mathf.Lerp(engine.clutch, 1, 0.1f);

                                //滑行或刹车熄火
                                if (engine.state == EngineState.Running) {
                                    if (vehicle.airPressure > 7f && vehicle.powerBattery > 4) {
                                        //发动机正在启动或正在运行-->熄火
                                        if (brakeInput > 0.6f || throttleInput < 0.001f) {
                                            engine.state = EngineState.Cutting;
                                        }
                                    }

                                }

                                //===发动机启动===
                                if ((engine.state == EngineState.Cutting || engine.state == EngineState.OFF) && throttleInput > 0.1f) {
                                    engine.state = EngineState.Starting;
                                    if (target.speed < 6f)
                                        ava.DoStart();
                                }


                            } else {
                                engine.clutch = Mathf.Lerp(engine.clutch, 0, 0.1f);
                                if (vehicle.airPressure > 7.99f && vehicle.powerBattery > 10) {
                                    if(engine.state == EngineState.Starting || engine.state == EngineState.Running) {
                                        engine.state = EngineState.Cutting;
                                    }
                                }
                            }

                            if (engine.state == EngineState.OFF && vehicle.airPressure < 5) {
                                engine.state = EngineState.Starting;
                                ava.DoStart();
                            }

                            //强制启动
                            if (engine.state == EngineState.OFF && Input.GetKeyDown(veKeyCode.startEngine)) {
                                if (engine.rpm > engine.rpmMin * 1.8f) engine.state = EngineState.Starting;
                                if (vehicle.battery > 2) {
                                    engine.state = EngineState.Starting;  //更新为 在启动 状态
                                    ava.DoStart();
                                } else {
                                    ava.DoStartBad();
                                }
                            }

                            break;
                        }
                    }

                    target.maxDriveForce = engine.force + motor.force;
                    break;
                }

            }
        }
        
        motor.rpm = Mathf.Lerp(motor.rpm, Mathf.Abs(rpmAxle) * motor.selfRatio, 0.1f);
        rpmPower = engine.rpm * (engine.clutch) + motor.rpm * (1-engine.clutch);

        doEngineUpdate(engine,true);

        setTargetInput();
        selectBrake();
    }


    void setTargetInput() {
        switch (trans.gear) {
            case Gear.P: target.throttleInput = 0; break;
            case Gear.R: target.throttleInput = -1; break;
            case Gear.N: target.throttleInput = 0; break;
            default: target.throttleInput = 1; break;
        }
        
        if (throttleInput < 0.01) {
            if (trans.type != TransType.AT && trans.type != TransType.MT) target.throttleInput = 0;
        }

        if(trans.type == TransType.MT && clutchInput<0.2f) target.throttleInput=clutchInput/0.2f;
    }

    float usedAir;
    bool brakeLock = false;

    /// <summary>
    /// 气刹
    /// </summary>
    /// <param name="standAlone">是否独立运行</param>
    void airBrake(bool standAlone = false) {
        if (retarder) {
            if (brakeInput >= retarder.brakeRatio) brakeLock = true;
            //正在刹车
            if (brakeLock) {
                usedAir = Mathf.MoveTowards(usedAir, UnityEngine.Random.Range(0.5f, 0.6f), 0.0012f * (brakeInput-retarder.brakeRatio));
                //释放刹车
                if (brakeInput < retarder.brakeRatio) {
                    ava.BreakAirRelease();
                    vehicle.airPressure -= usedAir;
                    brakeLock = false;
                }
                
            }
            target.brakeInput = Mathf.Max(brakeInput, (throttleInput < 0.01f) ? retarder.brakeOutput : 0);
        } else {
            if (brakeInput >= 0.2f) brakeLock = true;
            //正在刹车
            if (brakeLock) {
                usedAir = Mathf.MoveTowards(usedAir, UnityEngine.Random.Range(0.5f, 0.6f), 0.0006f * brakeInput);
                //释放刹车
                if (brakeInput < 0.2f) {
                    ava.BreakAirRelease();
                    vehicle.airPressure -= usedAir;
                    brakeLock = false;
                }
            }
            target.brakeInput = brakeInput;
        }

        //判断气刹作用条件
        if (vehicle.airPressure < 0.5) {
            brakeInput = 0;
        }


        //断气刹
        if (vehicle.airPressure < 4) handbrakeVehicle = 1;
        else handbrakeVehicle = 0;
        target.handbrakeInput = Mathf.Max(handbrakeUser, handbrakeVehicle);

        //充气机
        if (vehicleState == VehicleState.ON) {
            if (standAlone) {
                if (vehicle.airPressure < 6 && !compressorStarted) {
                    ava.DoCompressorAudioStart();
                    compressorStarted = true;
                }

                if (compressorStarted) {
                    vehicle.airPressure = Mathf.MoveTowards(vehicle.airPressure, 8, Time.deltaTime * 0.21f);
                    if (vehicle.airPressure == 8) {
                        ava.DoCompressorAudioStop();
                        compressorStarted = false;
                    }
                }
            } else {
                if (vehicle.airPressure < 7 && !compressorStarted && engine.state == EngineState.Running) {
                    ava.DoCompressorAudioStart();
                    compressorStarted = true;
                }

                if (compressorStarted) {
                    vehicle.airPressure = Mathf.MoveTowards(vehicle.airPressure, 8, engine.rpm * Time.deltaTime * 0.00021f);
                    ava.DoCompressorAudio();
                    if (vehicle.airPressure == 8) {
                        compressorStarted = false;
                        ava.extra.compressor.volume = 0;
                    }
                }
            }
        }
    }

    void selectBrake(bool standAlone = false) {
        if (vehicle.airBreak) {
            airBrake(standAlone);
        } else {
            target.brakeInput = brakeInput;// Mathf.Max(brakeInput,engineBrakeInput);
            target.handbrakeInput = handbrakeUser; //handbrakeInput;
        }
    }

    public void handbrakeOn() {
        //仅外部调用
        GlobalClass.Instance.uiShowInputs.handbrakeSwitch(true);
        handbrakeUser = 1;
        ava.HandBreakOn();
    }
    public void handbrakeOff() {
        //仅外部调用
        GlobalClass.Instance.uiShowInputs.handbrakeSwitch(false);
        handbrakeUser = 0;
        if (handbrakeVehicle < 0.5f) ava.HandBreakOff();
    }


    /// <summary>
    /// 单发动机车辆的计算
    /// </summary>
    void control_Engine() {
        if (vehEntrance.PlayerInVehicle) {
            if (vehicleState == VehicleState.OFF) {
                //按`开启电源
                if (Input.GetKeyDown(veKeyCode.insertKey)) {
                    vehicleState = VehicleState.Starting;
                    ava.DoPowerOn();
                    StartCoroutine(GlobalClass.DelayToInvokeDo(() => {
                        vehicleState = VehicleState.ON;
                        GlobalClass.Instance.uiShowInputs.powerSwitch(true);
/*                        if (engine.engineFan) {
                            engine.engineFan.avc = this;
                            if (engine.engineFan.IsInvoking()) {
                                engine.engineFan.CancelInvoke();
                            }
                            engine.engineFan.InvokeRepeating("SlowUpdate", 1, 0.2f);
                        }*/
                    }, 1.0f));
                }
            }

            if (vehicleState == VehicleState.ON) {
                //关闭电源
                if (Input.GetKeyDown(veKeyCode.insertKey)) {
                    StartCoroutine(GlobalClass.DelayToInvokeDo(() => {
                        vehicleState = VehicleState.OFF;
                        GlobalClass.Instance.uiShowInputs.powerSwitch(false);
                        motor.rpm = 0;
                        throttleInput = 0;
                        engine.force = 0;
                        target.maxDriveForce = 0;
                        CancelInvoke("SlowUpdate");
                    }, 0.5f));
                }

                //发动机启动检测
                if (engine.state==EngineState.OFF && Input.GetKeyDown(veKeyCode.startEngine)) {
                    if (engine.rpm > engine.rpmMin * 1.8f) engine.state = EngineState.Starting;
                    if (vehicle.battery > 2) {
                        engine.state = EngineState.Starting;  //更新为 在启动 状态
                        ava.DoStart();
                    } else {
                        ava.DoStartBad();
                    }
                }

                //======变速箱控制======
                doTransUpdate(trans.type);
            }
        }
        rpmPower = engine.rpm;
        doEngineUpdate(engine);
        target.maxDriveForce = engine.force * clutchInput;
        selectBrake();
        setTargetInput();
	}

    void doEngineUpdate(Engine e,bool standAlone=false) {
        rpmWheel = (float)Math.Round(ava.m_wheelsRpm * vehicle.mainRatio, 4, MidpointRounding.ToEven);
        rpmAxle = rpmWheel * transRatio;

        //===发动机运行中===
        if (e.state == EngineState.Running) {
            //关闭发动机判断
            if ((e.rpm < e.rpmMin * 0.4f) || (Input.GetKeyDown(veKeyCode.startEngine) && vehEntrance.PlayerInVehicle)) {
                ava.DoStop();
                e.state = EngineState.Cutting;
            }

            if (e.rpm < e.rpmMin) {
                e.throttle = Mathf.Lerp(e.throttle, throttleInput * 0.8f + 0.2f, 0.09f);
            } else {
                e.throttle = Mathf.Lerp(e.throttle, throttleInput, 0.09f);
            }
            
            if (trans.gear == Gear.N) {
                if (standAlone) {
                    e.rpmE = e.rpmMin;
                } else {
                    e.rpmE = e.rpmMin + e.throttle * Mathf.Clamp(e.rpmMax - e.rpmMin,100,1000);
                }
            } else {
                if (standAlone) {
                    e.rpmE = e.rpmMin * (1-e.clutch) + (motor.rpm/motor.selfRatio) * (e.clutch);
                } else {
                    e.rpmE = Mathf.Abs(rpmAxle) * clutchInput + (e.rpmMin + e.throttle*Mathf.Clamp(e.rpmMax-e.rpmMin,100,1000)) * (1-clutchInput);
                    e.rpmE *= engine.clutch;
                }
            }

            e.rpm = Mathf.Lerp(e.rpm, e.rpmE + UnityEngine.Random.Range(-50, 50), 0.02f);

            if (e.rpm < e.rpmMin)
                e.torque = e.torqueCurve.Evaluate(e.rpm) * (e.throttle * 0.9f + 0.1f);
            else
                e.torque = e.torqueCurve.Evaluate(e.rpm) * e.throttle;

            if (e.rpmE > e.rpmMax) e.torque = 0;
            e.power = e.torque * e.rpm / 9549.297f;

            //扭矩置0
            if (e.torque < 0) e.torque = 0;
            e.force = (e.torque * transRatio * vehicle.mainRatio * 0.9f) / radius;


            if(e.force < 1) {
                target.throttleInput = 0;
                //engineBrakeInput = (Mathf.Abs(e.force))/target.maxBrakeForce;
            }
            //资源的生成
            if (vehicle.battery < vehicle.batteryCapacity)
                vehicle.battery = Mathf.MoveTowards(vehicle.battery, vehicle.batteryCapacity, e.rpm * 0.0001f);
            if (vehicle.powerBattery < vehicle.powerBatteryCapacity)
                vehicle.powerBattery = Mathf.MoveTowards(vehicle.powerBattery, vehicle.powerBatteryCapacity, e.rpm * 0.00001f);

            //资源的消耗
            e.ge = e.geCurve.Evaluate(e.rpm) * e.geTorqueCurve.Evaluate(e.torque);
            fuelConsumeF = engine.ge * (e.power * Time.deltaTime / 3600) / 1000;
            vehicle.fuel -= fuelConsumeF;
            fuelConsumeTotal += fuelConsumeF;
            if (target.speed > 1) {
                fuelConsume = fuelConsumeF / (target.speed * Time.deltaTime / 1000) * 100;
            } else fuelConsume = 0.1f;
        }
        sTotal += target.speed * Time.deltaTime / 1000;
        fuelConsumeAdv = fuelConsumeTotal / sTotal * 100;

        //===发动机停机时===
        if (e.state == EngineState.OFF) {
            e.throttle = 0;
            e.rpm = Mathf.Lerp(e.rpm, rpmAxle * clutchInput * engine.clutch, 0.02f);
            e.torque = 0;
            e.power = 0;
            e.force = 0;
        }
        //===发动机发动===
        if (e.state == EngineState.Starting) {
            e.throttle = 1;
            vehicle.battery -= 5 * Time.deltaTime;
            e.rpm = Mathf.Lerp(e.rpm, e.rpmMin * 2.2f, 0.05f);  //增加转速
            if(e.rpm >= e.rpmMin * 1.8f) {
                e.state = EngineState.Running;     //更新为 运行 状态
                GlobalClass.Instance.uiShowInputs.engineSwitch(true);
            }
        }

        //===发动机熄火===
        if (e.state == EngineState.Cutting) {     //正在熄火就把发动机转速降为0
            e.throttle = 0;
            e.rpm = Mathf.Lerp(e.rpm, 0, 0.15f);
            e.torque = 0;
            e.force = (e.torque * transRatio * vehicle.mainRatio * 0.9f) / radius;
/*            if(e.force < 1) {
                engineBrakeInput = (Mathf.Abs(e.force))/target.maxBrakeForce;
            }*/
            if (e.rpm < 1) {
                e.state = EngineState.OFF;
                GlobalClass.Instance.uiShowInputs.engineSwitch(false);
                e.rpm = 0;
                e.torque = 0;// e.torqueCurve.Evaluate(0);
                e.power = 0;
                fuelConsumeF = 0;
            }
        }
    }

    /// <summary>
    /// 换挡按键检测
    /// </summary>
    /// <param name="type">变速器类型</param>
    void doTransUpdate(TransType type) {
        switch (type) {
            case TransType.MT: {
                clutchInput = inputManager.clutchAxis;
                if (clutchInput < 0.1f) {
                    if (Input.GetKeyDown(veKeyCode.gearUp)) {
                        //升档
                        if ((sbyte)trans.gear < trans.forward.Length && mainGear > Gear.N) {
                            //前进挡升档
                            setMainGear(++mainGear);
                            setTransGear(++trans.gear);
                        } else if (mainGear == Gear.N) { //空挡升前进档
                            if(target.speed>-0.5f) {
                                setGear(Gear.M1);
                            } else {
                                GlobalClass.Log("错误操作",LogType.Warning);
                            }
                        } else if (mainGear == Gear.R) { //倒挡升空挡
                            setGear(Gear.N);
                        }
                    }
                    if (Input.GetKeyDown(veKeyCode.gearDown)) {
                        //降档
                        if (mainGear > Gear.N) { //前进挡降档
                            setMainGear(--mainGear);
                            setTransGear(--trans.gear);
                        } else if (mainGear == Gear.N) { //空挡降倒档
                            if(target.speed<0.5f) { 
                                setMainGear(Gear.R);
                                setTransGear(Gear.R);
                            } else {
                                GlobalClass.Log("错误操作",LogType.Warning);
                            }
                        }
                    }
                }
                break;
            }
            case TransType.AMT: {
                //自动处理离合
                if (engine.rpm < engine.rpmMin + 300) {
                    clutchInput = Mathf.Lerp(clutchInput, Mathf.Clamp01(engine.rpm / 300 - 4f + throttleInput), 0.02f);
                } else {
                    clutchInput = Mathf.Lerp(clutchInput, 1, 0.08f);
                }

                changeGearDNR();

                if (gearLock) {
                    //throttleInput = Mathf.Lerp(throttleInput, 0, 0.1f);
                    engine.throttle = Mathf.Lerp(engine.throttle, 0, 0.1f);
                    clutchInput = Mathf.Lerp(clutchInput, 0, 0.2f);
                }else {
                    if (rpmPower > rpmPowerUp && (sbyte)trans.gear < trans.forward.Length && trans.gear > 0) {
                        gearLock = true;
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { 
                            setTransGear(++trans.gear);
                            ava.DoGearChange();
                        }, 0.5f));
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { gearLock = false; }, 1.0f));
                    }
                    if (rpmPower < rpmPowerDown && (sbyte)trans.gear > (sbyte)Gear.M1) {
                        gearLock = true;
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { 
                            setTransGear(--trans.gear);
                            ava.DoGearChange();
                        }, 0.5f));
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { gearLock = false; }, 1.0f));
                    }
                }
                break;
            }
            case TransType.CVT: {
                trans.forward[0] = (31 / (0.0071f * engine.rpm) - 1);
                transRatio = trans.forward[0];

                //自动处理离合
                if (engine.rpm < engine.rpmMin + 300) {
                    clutchInput = Mathf.Lerp(clutchInput, Mathf.Clamp01(engine.rpm / 300 - 4f + throttleInput), 0.005f);
                } else {
                    clutchInput = Mathf.Lerp(clutchInput, 1, 0.1f);
                }
                if (inputManager.brakeAxis > 0.4f && clutchInput < 0.5f) clutchInput = 0;

                changeGearDNR();
                break;
            }
            case TransType.AT: {
                //自动处理变矩器
                clutchInput = Mathf.Lerp(clutchInput, Mathf.Clamp01(engine.rpm / 500 - 1f + throttleInput), 0.01f);

                changeGearDNR();

                if (!gearLock) {
                    if (engine.rpm > rpmPowerUp && (sbyte)trans.gear < trans.forward.Length && trans.gear > 0) {
                        setTransGear(++trans.gear);
                        gearLock = true;
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { gearLock = false; }, 1.0f));
                    }
                    if (engine.rpm < rpmPowerDown && (sbyte)trans.gear > (sbyte)Gear.M1) {
                        setTransGear(--trans.gear);
                        gearLock = true;
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { gearLock = false; }, 1.0f));
                    }
                }
                break;
            }
            case TransType.DCT: {

                break;
            }
            case TransType.EV: {
                transRatio = trans.forward[0];
                changeGearDNR();
                break;
            }
            case TransType.AMT_EATON: {
                changeGearDNR();

                if (gearLock) {
                    clutchInput = Mathf.Lerp(clutchInput, 0, 0.2f);
                } else {
                    if (motor.rpm > motor.rpmMax * 0.85f && (sbyte)trans.gear < trans.forward.Length && trans.gear > 0) {
                        gearLock = true;
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { setTransGear(++trans.gear); }, 0.2f));
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { gearLock = false; }, 1.0f));
                    }
                    if (motor.rpm < engine.rpmMin * 1.3f && (sbyte)trans.gear > (sbyte)Gear.M1) {
                        gearLock = true;
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { setTransGear(--trans.gear); }, 0.2f));
                        StartCoroutine(GlobalClass.DelayToInvokeDo(() => { gearLock = false; }, 1.0f));
                    }
                }
                break;
            }
        }
    }

    void changeGearDNR() {
        if (Input.GetKeyDown(veKeyCode.gearUp)) {
            //升档
            if (trans.gear == Gear.N) {
                if(target.speed>-0.5f) {
                    //空挡升前进档
                    setMainGear(Gear.D);
                    setTransGear(Gear.M1);
                } else {
                    GlobalClass.Log("错误操作",LogType.Warning);
                }
            } else if (trans.gear == Gear.R) {
                if(target.speed<0.5f) {
                    //倒挡升空挡
                    setMainGear(Gear.N);
                    setTransGear(Gear.N);
                } else {
                    GlobalClass.Log("错误操作",LogType.Warning);
                }
            }
        }
        if (Input.GetKeyDown(veKeyCode.gearDown)) {
            //降档
            if (trans.gear > Gear.N) {
                //前进挡降档
                setMainGear(Gear.N);
                setTransGear(Gear.N);
            } else if (trans.gear == Gear.N) {
                //空挡降倒档
                setMainGear(Gear.R);
                setTransGear(Gear.R);
            }
        }
    }
	
    //============================================档位控制======================================
	/// <summary>
	/// 设置特定档位
	/// </summary>
	/// <param name="toGear"></param>
    void setGear(Gear toGear) {
        setMainGear(toGear);
        setTransGear(toGear);
    }

    void setMainGear(Gear toGear) {
        mainGear = toGear;
        Text gearText = GlobalClass.Instance.gearText;
        switch (toGear) {
            case Gear.R:
                gearText.text = "R";
                //gearText.color = new Color32(200,30,30,255);
                break;
            case Gear.N:
                gearText.text = "N";
                //gearText.color = new Color32(0,118,255,255);
                break;
            case Gear.M1: gearText.text = "1";
                break;
            case Gear.M2: gearText.text = "2";
                break;
            case Gear.M3: gearText.text = "3";
                break;
            case Gear.M4: gearText.text = "4";
                break;
            case Gear.M5: gearText.text = "5";
                break;
            case Gear.M6: gearText.text = "6";
                break;
            case Gear.M7: gearText.text = "7";
                break;
            case Gear.M8: gearText.text = "8";
                break;
            case Gear.M9: gearText.text = "9";
                break;
            case Gear.M10: gearText.text = "10";
                break;
            case Gear.M11: gearText.text = "11";
                break;
            case Gear.M12: gearText.text = "12";
                break;
            case Gear.D:
                gearText.text = "D";
                //gearText.color = new Color32(0,118,255,255);
                break;
            case Gear.P:
                gearText.text = "P";
                //gearText.color = new Color32(0, 118, 255, 255);
                break;
            default:
                gearText.text = "X";
                break;
        }
    }
    void setTransGear(Gear toGear) {
        trans.gear = toGear;

        switch (toGear) {
            case Gear.R:
                transRatio = trans.reverse;
                break;
            case Gear.N:
                transRatio = 0;
                break;
            case Gear.M1:
                transRatio = trans.forward[0];
                break;
            case Gear.M2:
                transRatio = trans.forward[1];
                break;
            case Gear.M3:
                transRatio = trans.forward[2];
                break;
            case Gear.M4:
                transRatio = trans.forward[3];
                break;
            case Gear.M5:
                transRatio = trans.forward[4];
                break;
            case Gear.M6:
                transRatio = trans.forward[5];
                break;
            case Gear.M7:
                transRatio = trans.forward[6];
                break;
            case Gear.M8:
                transRatio = trans.forward[7];
                break;
            case Gear.M9:
                transRatio = trans.forward[8];
                break;
            case Gear.M10:
                transRatio = trans.forward[9];
                break;
            case Gear.M11:
                transRatio = trans.forward[10];
                break;
            case Gear.M12:
                transRatio = trans.forward[11];
                break;
        }

    }


    //=============================================================加载============================================================

    [ContextMenu("加载车辆")]
    void LoadVehicle() {
        //仅限菜单
        string vehiclePath = "/vehicle/" + vehicleName + ".cfg";
        print("加载车辆:" + GlobalClass.GetGamePath() + vehiclePath);
        Configuration vehConfig = Configuration.LoadFromFile(GlobalClass.GetGamePath() + vehiclePath);
        loadVehiclePath(vehConfig);
    }

    public void loadVehiclePath(Configuration myConfig) {

        vehicle.name = myConfig["Vehicle"]["name"].StringValue;
        vehicle.description = myConfig["Vehicle"]["description"].StringValue;

        vehicle.type = myConfig["Vehicle"]["type"].GetValue<ControlType>();
        
        if (myConfig["Vehicle"].Contains("engine")) {
            vehicle.engine = myConfig["Vehicle"]["engine"].StringValue;
            if (!string.IsNullOrEmpty(vehicle.engine)) engine.Load(vehicle.engine);

            vehicle.fuelType = myConfig["Vehicle"]["fuelType"].GetValue<FuelType>();
            vehicle.fuelCapacity = myConfig["Vehicle"]["fuelCapacity"].FloatValue;
        }

        if (myConfig["Vehicle"].Contains("motor")) {
            vehicle.motor = myConfig["Vehicle"]["motor"].StringValue;
            if (!string.IsNullOrEmpty(vehicle.motor)) motor.Load(vehicle.motor);

            if (myConfig["Vehicle"].Contains("powerBatteryType")) {
                vehicle.powerBatteryType = myConfig["Vehicle"]["powerBatteryType"].GetValue<BatteryType>();
                vehicle.powerBatteryVoltage = myConfig["Vehicle"]["powerBatteryVoltage"].FloatValue;
                vehicle.powerBatteryCapacity = myConfig["Vehicle"]["powerBatteryCapacity"].FloatValue;
            }
        }

        if (myConfig["Vehicle"].Contains("transmission")) {
            vehicle.transmission = myConfig["Vehicle"]["transmission"].StringValue;
            if (!string.IsNullOrEmpty(vehicle.transmission)) trans.Load(vehicle.transmission);
        }


        if (myConfig["Vehicle"].Contains("mainRatio"))
            vehicle.mainRatio = myConfig["Vehicle"]["mainRatio"].FloatValue;
        else vehicle.mainRatio = 3.0f;

        if (myConfig["Vehicle"].Contains("sitCapacity"))
            vehicle.sitCapacity = myConfig["Vehicle"]["sitCapacity"].IntValue;
        else vehicle.sitCapacity = 1;

        if (myConfig["Vehicle"].Contains("standCapacity"))
            vehicle.standCapacity = myConfig["Vehicle"]["standCapacity"].IntValue;
        else vehicle.standCapacity = 0;

        if (myConfig["Vehicle"].Contains("batteryType")) {
            vehicle.batteryType = myConfig["Vehicle"]["batteryType"].GetValue<BatteryType>();
            vehicle.batteryVoltage = myConfig["Vehicle"]["batteryVoltage"].FloatValue;
            vehicle.batteryCapacity = myConfig["Vehicle"]["batteryCapacity"].FloatValue;
        }

        if(myConfig["Vehicle"].Contains("airBreak")) {
            vehicle.airBreak = myConfig["Vehicle"]["airBreak"].BoolValue;
            if (vehicle.airBreak) {
                if (myConfig["Vehicle"].Contains("airBreakCapacity")) {
                    vehicle.airBreakCapacity = myConfig["Vehicle"]["airBreakCapacity"].FloatValue;
                } else {
                    vehicle.airBreakCapacity = 8;
                }
            }
        }
        

        //=====
        vehicle.wheelFront = myConfig["Vehicle"]["wheelFront"].StringValue;
        vehicle.wheelBack = myConfig["Vehicle"]["wheelBack"].StringValue;
    }
}



//最高速度计算公式 (弃用)
//maxV = (engine.rpmMax * 6.28f * radius) / (60 * transRatio * vehicle.mainRatio);
//target.maxSpeedForward = maxV;