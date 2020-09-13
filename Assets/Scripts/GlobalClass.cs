using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpConfig;

public class GlobalClass : Singleton<GlobalClass> {
    public static string gamePath { get; set; }
    [Serializable]
    public class CommonKeyCodes {
        public KeyCode forward = KeyCode.W;
        public KeyCode backward = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode getOnVehicle = KeyCode.G;

        public KeyCode showDetail = KeyCode.LeftControl;

        public KeyCode slowMode = KeyCode.T;
        public KeyCode pause = KeyCode.Escape;
    }
    [Serializable]
    public class VehicleKeyCodes {
        public KeyCode throttle = KeyCode.W;
        public KeyCode brake = KeyCode.S;
        public KeyCode clutch = KeyCode.Tab;
        public KeyCode steerLeft = KeyCode.A;
        public KeyCode steerRight = KeyCode.D;
        public KeyCode handBrake = KeyCode.Space;

        public KeyCode resetVehicle = KeyCode.Return;

        public KeyCode lightSwitch = KeyCode.L;
        public KeyCode headLightSwitch = KeyCode.Semicolon;
        public KeyCode leftLight = KeyCode.Q;
        public KeyCode rightLight = KeyCode.E;
        public KeyCode doubleLight = KeyCode.F;

        public KeyCode horn = KeyCode.H;
        public KeyCode insertKey = KeyCode.BackQuote;
        public KeyCode startEngine = KeyCode.R;

        public KeyCode gearUp = KeyCode.PageUp;
        public KeyCode gearDown = KeyCode.PageDown;

        public KeyCode doorFront = KeyCode.Z;
        public KeyCode doorBack = KeyCode.X;
        public KeyCode door3 = KeyCode.C;
    }
    [Serializable]
    public class CameraKeyCodes {
        public KeyCode lockView = KeyCode.LeftControl;
        public KeyCode cameraFirst = KeyCode.Alpha1;
        public KeyCode cameraOrbit = KeyCode.Alpha2;
        public KeyCode cameraFollow = KeyCode.Alpha3;
        public KeyCode cameraFixed = KeyCode.Alpha4;
    }


    public PlayerControl player;
    public GameObject sceneController;

    public VehicleExtraInput InputManager;
    [Header("Audio")]
    public AudioSource audioClipTemplate;
    public AnimationCurve volumeCurve;

    [Header("Key Control")]
    public CommonKeyCodes commonKey;
    public VehicleKeyCodes vehicleKey;
    public CameraKeyCodes cameraKey;

    [Header("Mirror Control")]
    public GameObject camLeft;
    public GameObject camRight;
    public GameObject[] cams;

    //[Header("Traffic System")]
    //public TS.TSTrafficSpawner trafficSpawner;

    [Header("UI")]
    public Canvas canvas;
    public UIGlobal uiGlobal;
    public FloatInfoBox floatBox;
    public UnityEngine.UI.Text gearText;
    public EVP.ShowSpeed speedText;
    public UIShowInputs uiShowInputs;
    public GameObject UILog;
    public Transform UILogPos;
    public Color infoColor;
    public Color warningColor;
    public Color errorColor;

    [Header("Resources")]
    public Dictionary<string,GameObject> objectPool = new Dictionary<string,GameObject>();
    public Dictionary<string, Material> materialPool = new Dictionary<string, Material>();

    private void Awake() {
        loadKey();
        DontDestroyOnLoad(this);
    }

    void loadKey() {
        //驾驶键位
        Configuration keySetting = Configuration.LoadFromFile(GetGamePath() + "/config/keys.cfg");
        vehicleKey.throttle = keySetting["Vehicle"]["throttle"].GetValue<KeyCode>();
        vehicleKey.brake = keySetting["Vehicle"]["brake"].GetValue<KeyCode>();
        vehicleKey.clutch = keySetting["Vehicle"]["clutch"].GetValue<KeyCode>();
        vehicleKey.steerLeft = keySetting["Vehicle"]["steerLeft"].GetValue<KeyCode>();
        vehicleKey.steerRight = keySetting["Vehicle"]["steerRight"].GetValue<KeyCode>();
        vehicleKey.handBrake = keySetting["Vehicle"]["handBrake"].GetValue<KeyCode>();
        vehicleKey.resetVehicle = keySetting["Vehicle"]["resetVehicle"].GetValue<KeyCode>();

        vehicleKey.lightSwitch = keySetting["Vehicle"]["lightSwitch"].GetValue<KeyCode>();
        vehicleKey.headLightSwitch = keySetting["Vehicle"]["headLightSwitch"].GetValue<KeyCode>();
        vehicleKey.leftLight = keySetting["Vehicle"]["leftLight"].GetValue<KeyCode>();
        vehicleKey.rightLight = keySetting["Vehicle"]["rightLight"].GetValue<KeyCode>();
        vehicleKey.doubleLight = keySetting["Vehicle"]["doubleLight"].GetValue<KeyCode>();
        vehicleKey.horn = keySetting["Vehicle"]["horn"].GetValue<KeyCode>();
        vehicleKey.insertKey = keySetting["Vehicle"]["insertKey"].GetValue<KeyCode>();
        vehicleKey.startEngine = keySetting["Vehicle"]["startEngine"].GetValue<KeyCode>();

        vehicleKey.gearUp = keySetting["Vehicle"]["gearUp"].GetValue<KeyCode>();
        vehicleKey.gearDown = keySetting["Vehicle"]["gearDown"].GetValue<KeyCode>();

        vehicleKey.doorFront=keySetting["Vehicle"]["doorFront"].GetValue<KeyCode>();
        vehicleKey.doorBack=keySetting["Vehicle"]["doorBack"].GetValue<KeyCode>();
        vehicleKey.door3=keySetting["Vehicle"]["door3"].GetValue<KeyCode>();


        //通用键位
        commonKey.forward= keySetting["Common"]["forward"].GetValue<KeyCode>();
        commonKey.backward= keySetting["Common"]["backward"].GetValue<KeyCode>();
        commonKey.left= keySetting["Common"]["left"].GetValue<KeyCode>();
        commonKey.right= keySetting["Common"]["right"].GetValue<KeyCode>();
        commonKey.right= keySetting["Common"]["right"].GetValue<KeyCode>();
        commonKey.getOnVehicle= keySetting["Common"]["getOnVehicle"].GetValue<KeyCode>();
        commonKey.pause= keySetting["Common"]["pause"].GetValue<KeyCode>();
        commonKey.showDetail= keySetting["Common"]["showDetail"].GetValue<KeyCode>();

        //视角键位
        cameraKey.cameraFirst= keySetting["Camera"]["cameraFirst"].GetValue<KeyCode>();
        cameraKey.cameraOrbit= keySetting["Camera"]["cameraOrbit"].GetValue<KeyCode>();
        cameraKey.cameraFollow= keySetting["Camera"]["cameraFollow"].GetValue<KeyCode>();
        cameraKey.cameraFixed= keySetting["Camera"]["cameraFixed"].GetValue<KeyCode>();
    }
    [ContextMenu("保存key")]
    void saveKey() {
        Configuration keySetting = new Configuration();
        keySetting["Vehicle"]["throttle"].SetValue(vehicleKey.throttle);
        keySetting["Vehicle"]["brake"].SetValue(vehicleKey.brake);
        keySetting["Vehicle"]["clutch"].SetValue(vehicleKey.clutch);
        keySetting["Vehicle"]["steerLeft"].SetValue(vehicleKey.steerLeft);
        keySetting["Vehicle"]["steerRight"].SetValue(vehicleKey.steerRight);
        keySetting["Vehicle"]["handBrake"].SetValue(vehicleKey.handBrake);
        keySetting["Vehicle"]["resetVehicle"].SetValue(vehicleKey.resetVehicle);

        keySetting["Vehicle"]["lightSwitch"].SetValue(vehicleKey.lightSwitch);
        keySetting["Vehicle"]["headLightSwitch"].SetValue(vehicleKey.headLightSwitch);
        keySetting["Vehicle"]["leftLight"].SetValue(vehicleKey.leftLight);
        keySetting["Vehicle"]["rightLight"].SetValue(vehicleKey.rightLight);
        keySetting["Vehicle"]["doubleLight"].SetValue(vehicleKey.doubleLight);
        keySetting["Vehicle"]["horn"].SetValue(vehicleKey.horn);
        keySetting["Vehicle"]["insertKey"].SetValue(vehicleKey.insertKey);
        keySetting["Vehicle"]["startEngine"].SetValue(vehicleKey.startEngine);

        keySetting["Vehicle"]["gearUp"].SetValue(vehicleKey.gearUp);
        keySetting["Vehicle"]["gearDown"].SetValue(vehicleKey.gearDown);

        keySetting["Vehicle"]["doorFront"].SetValue(vehicleKey.doorFront);
        keySetting["Vehicle"]["doorBack"].SetValue(vehicleKey.doorBack);
        keySetting["Vehicle"]["door3"].SetValue(vehicleKey.door3);

        keySetting["Common"]["forward"].SetValue(commonKey.forward);
        keySetting["Common"]["backward"].SetValue(commonKey.backward);
        keySetting["Common"]["left"].SetValue(commonKey.left);
        keySetting["Common"]["right"].SetValue(commonKey.right);
        keySetting["Common"]["slowMode"].SetValue(commonKey.slowMode);
        keySetting["Common"]["getOnVehicle"].SetValue(commonKey.getOnVehicle);
        keySetting["Common"]["pause"].SetValue(commonKey.pause);
        keySetting["Common"]["showDetail"].SetValue(commonKey.showDetail);

        keySetting["Camera"]["cameraFirst"].SetValue(cameraKey.cameraFirst);
        keySetting["Camera"]["cameraOrbit"].SetValue(cameraKey.cameraOrbit);
        keySetting["Camera"]["cameraFollow"].SetValue(cameraKey.cameraFollow);
        keySetting["Camera"]["cameraFixed"].SetValue(cameraKey.cameraFixed);
        keySetting["Camera"]["lockView"].SetValue(cameraKey.lockView);

        keySetting.SaveToFile(GetGamePath() + "/config/keys.cfg");
    }

    public static string GetGamePath() {
        if(string.IsNullOrEmpty(gamePath)) {
            string strPath = Application.dataPath;
            return gamePath = strPath.Substring(0,strPath.LastIndexOf("/")) + "/GameData";
        } else {
            return gamePath;
        }
    }



    public static string GetBasePath() {
        return "./base";
    }

    public static IEnumerator DelayToInvokeDo(Action action,float delaySeconds) {
        yield return new WaitForSeconds(delaySeconds);
        action();
    }

    public static AnimationCurve LoadCurve(Configuration config, string configName, string curveName) {
        sbyte nodeNum = config[configName][curveName].SByteValue;

        CurveKey curveKey;
        Keyframe[] keyframe = new Keyframe[nodeNum];
        for (sbyte i = 0;i < nodeNum;i++) {
            curveKey = config[curveName + "_" + i].ToObject<CurveKey>();

            keyframe[i].time = curveKey.time;
            keyframe[i].value = curveKey.value;
            keyframe[i].inTangent = curveKey.inTangent;
            keyframe[i].outTangent = curveKey.outTangent;
            keyframe[i].inWeight = curveKey.inWeight;
            keyframe[i].outWeight = curveKey.outWeight;
        }
        return new AnimationCurve(keyframe);
    }

    public static void Log(string message, LogType type) {
        GameObject n = Instantiate(Instance.UILog,Instance.UILogPos);
        
        UnityEngine.UI.Text text = n.GetComponent<UnityEngine.UI.Text>();
        switch (type) {
            case LogType.Info:
                text.color = Instance.infoColor;
                break;
            case LogType.Warning:
                text.color = Instance.warningColor;
                break;
            case LogType.Error:
                text.color = Instance.errorColor;
                break;
        }
        text.text = message;
    }
}
public enum LogType:byte { Info = 0, Warning = 1, Error = 2 };

/// <summary>
/// 动画关键帧配置文件
/// </summary>
public class CurveKey {
    public float time { get; set; }
    public float value { get; set; }
    public float inTangent { get; set; }
    public float outTangent { get; set; }
    public float inWeight { get; set; }
    public float outWeight { get; set; }
}

public enum FuelType:byte {
    NONE = 0,
    煤 = 11, 木头 = 21,
    汽油 = 51, 柴油 = 52, 煤油 = 53,
    天然气 = 101, CNG = 102, LNG = 103, LPG = 104,
    沼气 = 110,
    H2 = 120, 甲烷 = 121, 甲醇 = 131, 乙醇 = 132
};

public enum BatteryType:byte {
    NONE = 0,
    铅酸 = 2,
    镍氢 = 3,
    锂 = 10,
    锂离子 = 11,
    磷酸铁锂 = 12,
    锂聚合物 = 13,
    钴酸锂 = 14,
    锰酸锂 = 15,
    钛酸锂 = 16,
    镍钴锰酸锂 = 17,
    镍钴铝酸锂 = 18,

    光伏 = 100,
    超级电容 = 110
};

[Serializable]
public class PersonInfo {
    public float health = 1f;
    public float energy = 1f;
    public float hunger = 1f;
    public float air = 1f;

    public float strong = 1f;

    public float special = 0f;

}