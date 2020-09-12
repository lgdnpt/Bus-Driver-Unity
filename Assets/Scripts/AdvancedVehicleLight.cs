using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedVehicleLight : MonoBehaviour {
    /// <summary>
    /// 灯光组件类，控制灯光的mesh和light
    /// </summary>
    [System.Serializable]
    public class LightAssembly {
        public MeshRenderer[] meshRenderer;
        public Light[] light;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
        public bool nightLight = false;
        public bool smooth = false;

        public float intensity = 0;
        public float emissionIntensity = 0;
        public Color color;
        public Color emissionColor;

        
        public void turnOn() {
            for(byte i=0;i<meshRenderer.Length;i++)
                meshRenderer[i].material.SetColor("_EmissionColor", emissionColor* emissionIntensity);


            for (byte i = 0;i < light.Length;i++) {
                light[i].intensity = intensity;
                light[i].color = color;
            }
        }
        public void turnOff() {
            for (byte i = 0;i < meshRenderer.Length;i++)
                meshRenderer[i].material.SetColor("_EmissionColor", Color.black);


            for (byte i = 0;i < light.Length;i++) {
                light[i].intensity = 0;
            }
        }
    }

    GlobalClass.VehicleKeyCodes vKeyCode;

    public AdvancedVehicleController avc;
    public AdvancedVehicleAudio ava;


    public LightAssembly headLightLow;
    public LightAssembly headLightHigh;

    public LightAssembly[] dayLight;
    public LightAssembly[] nightLight;
    public LightAssembly brakeLight;
    public LightAssembly reverseLight;

    public LightAssembly LeftLight;
    public LightAssembly RightLight;
    public float blinkTime = 0.5f;

    public enum LightState:byte {Off, On, Light }
    public enum HeadlightState:byte { Close, Low, High }

    public LightState lightState=LightState.Off;
    public HeadlightState headlightState=HeadlightState.Close;
    public bool highHeadLight = false;

    public bool brakeOn = false, reverseOn = false, leftOn = false, rightOn = false;

    void Start() {
        if(avc == null)
            avc = GetComponent<AdvancedVehicleController>();
        if(ava == null)
            ava = GetComponent<AdvancedVehicleAudio>();
        vKeyCode = GlobalClass.Instance.vehicleKey;
    }


    void Update() {
        if (!GetComponent<EnterVehicle>().PlayerInVehicle) return;

        if (!brakeOn && avc.brakeInput > 0.001) {
            brakeLight.turnOn();
            brakeOn = true;
        }
        if (brakeOn && avc.brakeInput <= 0.001) {
            brakeLight.turnOff();
            brakeOn = false;
        }

        if (!reverseOn && avc.mainGear == AdvancedVehicleController.Gear.R) {
            reverseLight.turnOn();
            reverseOn = true;
        }
        if (reverseOn && avc.mainGear != AdvancedVehicleController.Gear.R) {
            reverseLight.turnOff();
            reverseOn = false;
        }

        //灯光总开关
        if (Input.GetKeyDown(vKeyCode.lightSwitch)) {
            switch (lightState) {
                case LightState.Off:
                    //开启行车灯
                    lightState = LightState.On;
                    turnOnNightLight();

                    break;
                case LightState.On:
                    //开启近光灯
                    lightState = LightState.Light;

                    if(highHeadLight) headLightHigh.turnOn();
                    headLightLow.turnOn();

                    break;
                case LightState.Light:
                    //关闭所有灯光
                    lightState = LightState.Off;

                    turnOffNightLight();
                    headLightLow.turnOff();
                    headLightHigh.turnOff();
                    break;
            }
        }

        if (Input.GetKeyDown(vKeyCode.headLightSwitch)) {
            if (lightState == LightState.Light) {
                //切换近远光灯
                if (highHeadLight) {
                    headLightHigh.turnOff();
                    highHeadLight = false;
                } else {
                    headLightHigh.turnOn();
                    highHeadLight = true;
                }
            } else {
                //闪光
                headLightHigh.turnOn();
                highHeadLight = true;
            }

        }

        //关闭闪光
        if (Input.GetKeyUp(vKeyCode.headLightSwitch) && lightState != LightState.Light) {
            headLightHigh.turnOff();
            highHeadLight = false;
        }


        //左转向灯
        if (Input.GetKeyDown(vKeyCode.leftLight)) {
            if (leftOn && !rightOn) {
                //关闭
                StopCoroutine("blinker");
                ava.BlinkStop();
                LeftLight.turnOff();
                leftOn = false;
            } else {
                StopCoroutine("blinker");
                ava.BlinkStop();
                RightLight.turnOff();
                rightOn = false;
                StartCoroutine("blinker", 'L');
                ava.BlinkStart();
                leftOn = true;
            }
        }
        //右转向灯
        if (Input.GetKeyDown(vKeyCode.rightLight)) {
            if(rightOn && !leftOn) {
                StopCoroutine("blinker");
                RightLight.turnOff();
                ava.BlinkStop();
                rightOn = false;
            } else {
                StopCoroutine("blinker");
                LeftLight.turnOff();
                ava.BlinkStop();
                leftOn = false;
                StartCoroutine("blinker", 'R');
                ava.BlinkStart();
                rightOn = true;
            }
        }
        //危险报警灯
        if (Input.GetKeyDown(vKeyCode.doubleLight)) {
            if (leftOn && rightOn) {
                StopCoroutine("blinker");
                ava.BlinkStop();
                LeftLight.turnOff();
                RightLight.turnOff();
                leftOn = false;
                rightOn = false;
            } else {
                StopCoroutine("blinker");
                ava.BlinkStop();

                StartCoroutine("blinker", 'A');
                ava.BlinkStart();
                leftOn = true;
                rightOn = true;
            }

        }

    }

    IEnumerator blinker(object value) {
        while (true) {
            switch (value) {
                case 'L':
                    LeftLight.turnOn();
                    break;
                case 'R':
                    RightLight.turnOn();
                    break;
                case 'A':
                    LeftLight.turnOn();
                    RightLight.turnOn();
                    break;
            }
            yield return new WaitForSeconds(blinkTime);
            switch (value) {
                case 'L':
                    LeftLight.turnOff();
                    break;
                case 'R':
                    RightLight.turnOff();
                    break;
                case 'A':
                    LeftLight.turnOff();
                    RightLight.turnOff();
                    break;
            }
            yield return new WaitForSeconds(blinkTime);
        }
    }

    void turnOnNightLight() {
        for (byte i = 0;i < nightLight.Length;i++) {
            nightLight[i].turnOn();
        }
    }
    void turnOffNightLight() {
        for (byte i = 0;i < nightLight.Length;i++) {
            nightLight[i].turnOff();
        }
    }

    public void turnOnDayLight() {
        for (byte i = 0;i < dayLight.Length;i++) {
            dayLight[i].turnOn();
        }
    }
    public void turnOffDayLight() {
        for (byte i = 0;i < dayLight.Length;i++) {
            dayLight[i].turnOff();
        }
    }
}
