using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterVehicle : MonoBehaviour {
    public Transform getOutPosition;
    public Transform driverPosition;

    public bool PlayerInVehicle=false;
    public DoorControl[] doorControls;

    public MonoBehaviour[] activeComponents;
    public GameObject[] activeObjects;
    	
	void Update () {
        if (PlayerInVehicle) {
            if (Input.GetKeyDown(KeyCode.G)) {
                if(Mathf.Abs(GlobalClass.Instance.InputManager.target.speed)<1)
                    GetOffBus();
                else
                    print("停稳后下车");
            }
        }
    }

    void resetUIState(AdvancedVehicleController avc) {
        //上车时检测车辆状态
        if(avc.vehicleState==AdvancedVehicleController.VehicleState.ON) {
            GlobalClass.Instance.uiShowInputs.powerSwitch(true);
        } else {
            GlobalClass.Instance.uiShowInputs.powerSwitch(false);
        }

        if(avc.handbrakeUser<0.5) {
            GlobalClass.Instance.uiShowInputs.handbrakeSwitch(false);
        } else {
            GlobalClass.Instance.uiShowInputs.handbrakeSwitch(true);
        }

        if(avc.engine.state==AdvancedVehicleController.EngineState.Running) {
            GlobalClass.Instance.uiShowInputs.engineSwitch(true);
        } else {
            GlobalClass.Instance.uiShowInputs.engineSwitch(false);
        }

        GlobalClass.Instance.uiShowInputs.transSwitch(avc.trans.type);
    }

/*    void resetUIState(AVC.ECU avc) {
        //上车时检测车辆状态
        if(avc.vehicleState==AVC.ECU.VehicleState.ON) {
            GlobalClass.Instance.uiShowInputs.powerSwitch(true);
        } else {
            GlobalClass.Instance.uiShowInputs.powerSwitch(false);
        }

        if(avc.handbrakeUser<0.5) {
            GlobalClass.Instance.uiShowInputs.handbrakeSwitch(false);
        } else {
            GlobalClass.Instance.uiShowInputs.handbrakeSwitch(true);
        }

        if(avc.engine.state==AVC.Engine.State.Running) {
            GlobalClass.Instance.uiShowInputs.engineSwitch(true);
        } else {
            GlobalClass.Instance.uiShowInputs.engineSwitch(false);
        }

        GlobalClass.Instance.uiShowInputs.transSwitch((AdvancedVehicleController.TransType)avc.trans.type);
    }*/

    void resetUIState() {
        //下车时重置为默认
        GlobalClass.Instance.uiShowInputs.signEngine.color=Color.white;
        GlobalClass.Instance.uiShowInputs.signPower.color=Color.white;
        GlobalClass.Instance.uiShowInputs.signHandbrake.color=Color.white;
    }

    /// <summary>
    /// 上车相关代码
    /// </summary>
    void GetOnBus() {
        PlayerInVehicle = true;
/*        if(GetComponent<AVC.ECU>() != null) {
            GetOnNew();
            return;
        }*/

        GameObject scenectrl = GlobalClass.Instance.sceneController;
        VehicleExtraInput vehinput = GlobalClass.Instance.InputManager;
        AdvancedVehicleController avc = GetComponent<AdvancedVehicleController>();
        EVP.VehicleController vehctrl = avc.target;

        //设定速度表目标
        GlobalClass.Instance.speedText.enabled=true;
        GlobalClass.Instance.uiShowInputs.avc = avc;

        //设定调试界面目标
        scenectrl.GetComponent<EVP.VehicleTelemetry>().target = vehctrl;
        scenectrl.GetComponent<EVP.VehicleCameraController>().target = transform;

        //设定输入组件
        vehinput.target = vehctrl;
        vehinput.avc = avc;
        vehinput.ava = avc.ava;
        vehinput.avl = GetComponent<AdvancedVehicleLight>();
        vehinput.steerAxis = vehctrl.steerInput;

        //启用各组件
        for (byte i = 0;i < activeComponents.Length;i++) {
            if(activeComponents[i] != null)
                activeComponents[i].enabled = true;
        }
        for (byte i = 0;i < activeObjects.Length;i++) {
            if (activeObjects[i] != null)
                activeObjects[i].SetActive(true);
        }

        resetUIState(avc);
    }

/*    void GetOnNew() {
        GameObject scenectrl = GlobalClass.Instance.sceneController;
        VehicleExtraInput vehinput = GlobalClass.Instance.InputManager;
        AVC.ECU ecu = GetComponent<AVC.ECU>();

        //设定速度表目标
        GlobalClass.Instance.speedText.enabled=true;

        //设定调试界面目标
        scenectrl.GetComponent<EVP.VehicleTelemetry>().target = ecu.target;
        scenectrl.GetComponent<EVP.VehicleCameraController>().target = transform;

        //设定输入组件
        vehinput.target = ecu.target;
        vehinput.ecu = ecu;
        vehinput.avl = GetComponent<AdvancedVehicleLight>();
        vehinput.steerAxis = ecu.target.steerInput;

        //启用各组件
        for(byte i = 0;i < activeComponents.Length;i++) {
            if(activeComponents[i] != null)
                activeComponents[i].enabled = true;
        }
        for(byte i = 0;i < activeObjects.Length;i++) {
            if(activeObjects[i] != null)
                activeObjects[i].SetActive(true);
        }

        resetUIState(ecu);

    }*/

    /// <summary>
    /// 下车相关代码
    /// </summary>
    void GetOffBus() {
        GameObject scenectrl = GlobalClass.Instance.sceneController;
        PlayerControl player = GlobalClass.Instance.player;
        VehicleExtraInput vehinput = GlobalClass.Instance.InputManager;

        vehinput.target = null;
        vehinput.throttleAxis = 0;
        vehinput.brakeAxis = 0;
        vehinput.clutchAxis = 0;                                   //输入组件清除

        scenectrl.GetComponent<EVP.VehicleCameraController>().target = player.transform;//摄像机焦点定位到人物
        scenectrl.GetComponent<EVP.VehicleTelemetry>().target = null;//调试目标清除
        GlobalClass.Instance.speedText.clear();

        AdvancedVehicleController avc = GetComponent<AdvancedVehicleController>();
        avc.throttleInput = 0;                                       //清零原有油门数据

        if (avc.vehicleState != AdvancedVehicleController.VehicleState.OFF) {
            if (avc.engine.state != AdvancedVehicleController.EngineState.OFF) {
                GlobalClass.Log("下车时车辆未锁定、发动机未熄火", LogType.Warning);
            } else {
                GlobalClass.Log("下车时车辆未锁定", LogType.Warning);
            }
        }

        //关闭各组件
        for (byte i = 0;i < activeComponents.Length;i++) {
            if(activeComponents[i] != null)
                activeComponents[i].enabled = false;
        }
        for (byte i = 0;i < activeObjects.Length;i++) {
            if(activeObjects[i] != null)
                activeObjects[i].SetActive(false);
        }

        resetUIState();

        player.transform.position = getOutPosition.position;         //重新定位人物到下车点
        player.transform.rotation = Quaternion.Euler(0,0,0);
        player.transform.parent = null;
        player.m_Animator.SetBool("Sit", false);
        player.canControl = true;
        player.m_Rigidbody.isKinematic = false;
        player.m_Capsule.isTrigger = false;

        PlayerInVehicle = false;
        enabled = false;
    }
}
