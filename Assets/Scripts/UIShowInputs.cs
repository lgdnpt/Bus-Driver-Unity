using UnityEngine;
using UnityEngine.UI;

public class UIShowInputs : MonoBehaviour {
	public VehicleExtraInput input;
	public AdvancedVehicleController avc;
	public Slider clutch;
	public Slider brake;
	public Slider throttle;
    [Space(5)]
    public Slider rpm;
	public Text rpmText;
    Image rpmImage;
    [Space(5)]
	public Slider rpm_m;
	public Text rpm_mText;
    Image rpm_mImage;

    [Space(5)]
    public Slider fuel;
	public Text fuelText;
	public Slider mainBattery;
	public Text mainBatteryText;
	public Slider air;
	public Text airText;
    public Slider powerBattery;
    public Text powerBatteryText;

    [Space(5)]
    public Image signPower;
    public Image signHandbrake;
    public Image signEngine;
    public Image signTran;
    public Sprite[] signTrans=new Sprite[7];

    [Space(5)]
    public Image signEmpty;

    void Start () {
		if (input == null)
			input = GlobalClass.Instance.InputManager;
        rpmImage = rpm.fillRect.GetComponent<Image>();
        rpm_mImage = rpm_m.fillRect.GetComponent<Image>();
    }
	
	void Update () {
		clutch.value = input.clutchAxis;
		brake.value = input.brakeInput;
		throttle.value = Mathf.Abs(input.throttleAxis);
		if (avc != null) {
			rpm.value = avc.engine.rpm/avc.engine.rpmMax;
			rpmText.text = avc.engine.rpm.ToString();

            rpm_m.value = avc.motor.rpm/avc.motor.rpmMax;
            rpm_mText.text = avc.motor.rpm.ToString();
            
            //变色
            //rpmImage.color = Color.Lerp(Color.green, Color.yellow, rpm.value);
            //rpm_mImage.color = Color.Lerp(Color.green, Color.yellow, rpm_m.value);

            fuel.value = avc.vehicle.fuel/avc.vehicle.fuelCapacity;
            //fuelText.text = avc.vehicle.fuel.ToString();

            mainBattery.value = avc.vehicle.battery/avc.vehicle.batteryCapacity;
            //mainBatteryText.text = avc.vehicle.battery.ToString();

            air.value = avc.vehicle.airPressure / avc.vehicle.airBreakCapacity;
            //airText.text = avc.vehicle.airPressure.ToString();
            powerBattery.value = avc.vehicle.powerBattery / avc.vehicle.powerBatteryCapacity;
            //powerBatteryText.text = avc.vehicle.powerBattery.ToString();
        }
	}

    public void powerSwitch(bool powerOn) {
        if(powerOn) {
            signPower.color=Color.green;
        } else {
            signPower.color=Color.white;
        }
    }

    public void handbrakeSwitch(bool brakeOn) {
        if(brakeOn) {
            signHandbrake.color=Color.red;
        } else {
            signHandbrake.color=Color.gray;
        }
    }

    public void engineSwitch(bool engineOn) {
        if(engineOn) {
            signEngine.color=Color.green;
        } else {
            signEngine.color=Color.white;
        }
    }

    public void transSwitch(AdvancedVehicleController.TransType type) {
        switch(type) {
            case AdvancedVehicleController.TransType.AT:
                signTran.sprite=signTrans[0];
                break;
            case AdvancedVehicleController.TransType.MT:
                signTran.sprite=signTrans[1];
                break;
            case AdvancedVehicleController.TransType.AMT:
                signTran.sprite=signTrans[2];
                break;
            case AdvancedVehicleController.TransType.EV:
                signTran.sprite=signTrans[3];
                break;
            case AdvancedVehicleController.TransType.AMT_EATON:
                signTran.sprite=signTrans[4];
                break;
            case AdvancedVehicleController.TransType.CVT:
                signTran.sprite=signTrans[5];
                break;
            case AdvancedVehicleController.TransType.DCT:
                signTran.sprite=signTrans[6];
                break;
        }
    }
}
