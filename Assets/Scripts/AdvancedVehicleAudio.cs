using UnityEngine;
using System;

// Per-wheel audio-related data
public class WheelAudioData {
	public float lastDownforce = 0.0f;
	public float lastWheelBumpTime = 0.0f;
}

[RequireComponent(typeof(EVP.VehicleController))]
[RequireComponent(typeof(AdvancedVehicleController))]

public class AdvancedVehicleAudio : MonoBehaviour {
	// 为了更有条理性，相关设置被安排在了各个class里。
	public AdvancedVehicleController avc;

    [Serializable]
	public class Extras {
		public AudioSource horn;
		public AudioSource blinker;
		public AudioSource reverser;
		public AudioSource airBrake;
		public AudioClip[] airBrakes;

        [Space(5)]
        public AudioClip powerOn;

        [Space(5)]
        public AudioSource compressor;
        public AudioClip compressorOn;
        public AudioClip compressorOff;

        [Space(5)]
        public AudioSource airConditioner;

        [Space(5)]
		public AudioSource handBrake;
		public AudioClip handBrakeCharge;
		public AudioClip handBrakeRelease;

        [Space(5)]
        public AudioClip[] gearChange;

        [Space(5)]
        public AudioClip[] doorOpen;
        public AudioClip[] doorClose;
    }

	[Serializable]
	public class Engine {
		public Transform audioEngine;
		public AudioClip[] engineStart;
		public AudioClip engineStartBad;
		public AudioClip engineStop;
    }

	[Serializable]
	public class Engines {
		public AudioSource audioSource;
		[Space(5)]
		public float minRpm = 600.0f;
		public float maxRpm = 6000.0f;
		[Space(5)]
		public float minVolume = 0.4f;
		public float maxVolume = 0.6f;
		[Space(5)]
		//public float pitchReference;
		public float minPitch;
		public float maxPitch;
	}

    [Serializable]
	public class EngineExtras {
		public AudioSource turboAudioSource;
		public float turboMinRpm = 3500.0f;
		public float turboMaxRpm = 5500.0f;
		[Range(0,3)]
		public float turboMinPitch = 0.8f;
		[Range(0,3)]
		public float turboMaxPitch = 1.5f;
		[Range(0,1)]
		public float turboMaxVolume = 1.0f;
		public bool turboRequiresThrottle = true;

		[Space(5)]
		public AudioClip turboDumpClip;
		public float turboDumpMinRpm = 5000.0f;
		public float turboDumpMinInterval = 2.0f;
		public float turboDumpMinThrottleTime = 0.3f;
		public float turboDumpVolume = 0.5f;

		[Space(5)]
		public AudioSource transmissionAudioSource;
		[Range(0.1f,1)]
		public float transmissionMaxRatio = 0.9f;       // 最大传输速度比 (maxRpm * gearCount) 当传输最大时
		[Range(0,3)]
		public float transmissionMinPitch = 0.2f;
		[Range(0,3)]
		public float transmissionMaxPitch = 1.1f;
		[Range(0,1)]
		public float transmissionMinVolume = 0.1f;
		[Range(0,1)]
		public float transmissionMaxVolume = 0.2f;
	}

	[Serializable]
	public class Wheels {
		public AudioSource skidAudioSource;
		public float skidMinSlip = 2.0f;
		public float skidMaxSlip = 7.0f;
		[Range(0,3)]
		public float skidMinPitch = 0.9f;
		[Range(0,3)]
		public float skidMaxPitch = 0.8f;
		[Range(0,1)]
		public float skidMaxVolume = 0.75f;

		[Space(5)]
		public AudioSource offroadAudioSource;
		public float offroadMinSpeed = 1.0f;
		public float offroadMaxSpeed = 20.0f;
		[Range(0,3)]
		public float offroadMinPitch = 0.3f;
		[Range(0,3)]
		public float offroadMaxPitch = 2.5f;
		[Range(0,1)]
		public float offroadMinVolume = 0.3f;
		[Range(0,1)]
		public float offroadMaxVolume = 0.6f;

		[Space(5)]
		public AudioClip bumpAudioClip;
		public float bumpMinForceDelta = 2000.0f;		// 在固定时间内被视为一次颠簸的最小力的变化
		public float bumpMaxForceDelta = 18000.0f;		// 造成最大强度的颠簸所需的力
		[Range(0,1)]
		public float bumpMinVolume = 0.2f;				// 最小颠簸音量
		[Range(0,1)]
		public float bumpMaxVolume = 0.6f;				// 最大颠簸音量
	}

	[Serializable]
	public class Impacts {
		[Space(5)]
		public AudioClip hardImpactAudioClip;
		public AudioClip softImpactAudioClip;
		public float minSpeed = 0.1f;				// 撞击的最小接触速度
		public float maxSpeed = 10.0f;              // 撞击的最大接触速度
		[Range(0,3)]
		public float minPitch = 0.3f;				// 最小接触速度下的音高
		[Range(0,3)]
		public float maxPitch = 0.6f;               // 最大接触速度下的音高
		[Range(0,3)]
		public float randomPitch = 0.2f;			// 随机音高范围 (+- 值)
		[Range(0,1)]
		public float minVolume = 0.7f;				// 最小解除速度的音量
		[Range(0,1)]
		public float maxVolume = 1.0f;              // 最大解除速度的音量
		[Range(0,1)]
		public float randomVolume = 0.2f;			// 随机音量范围 (+- 值)
	}

	[Serializable]
	public class Drags {
		public AudioSource hardDragAudioSource;
		public AudioSource softDragAudioSource;
		public float minSpeed = 2.0f;
		public float maxSpeed = 20.0f;
		[Range(0,3)]
		public float minPitch = 0.6f;
		[Range(0,3)]
		public float maxPitch = 0.8f;
		[Range(0,1)]
		public float minVolume = 0.8f;
		[Range(0,1)]
		public float maxVolume = 1.0f;

		[Space(5)]
		public AudioClip scratchAudioClip;
		public float scratchRandomThreshold = 0.02f;	// 拖动事件造成一次拖动的百分比
		public float scratchMinSpeed = 2.0f;
		public float scratchMinInterval = 0.2f;
		[Range(0,3)]
		public float scratchMinPitch = 0.7f;
		[Range(0,3)]
		public float scratchMaxPitch = 1.1f;
		[Range(0,1)]
		public float scratchMinVolume = 0.9f;
		[Range(0,1)]
		public float scratchMaxVolume = 1.0f;
	}

	[Serializable]
	public class Wind {
		public AudioSource windAudioSource;
		public float minSpeed = 3.0f;
		public float maxSpeed = 30.0f;
		[Range(0,3)]
		public float minPitch = 0.5f;
		[Range(0,3)]
		public float maxPitch = 1.0f;
		[Range(0,1)]
		public float maxVolume = 0.5f;
	}


	// 现有的公共属性
	[Tooltip("单次发声源")]
	public AudioSource audioClipTemplate;
	[Space(2)]
	public Extras extra = new Extras();
	[Space(2)]
	public Engine engine = new Engine();
	[Space(2)]
	public Engines[] engines = new Engines[0];
    [Space(2)]
    public Engines[] exhausts = new Engines[0];
    [Space(2)]
	public Engine motor = new Engine();
	[Space(2)]
	public Engines[] motors = new Engines[0];
    [Space(2)]
	public EngineExtras engineExtras = new EngineExtras();
	[Space(2)]
	public Wheels wheels = new Wheels();
	[Space(2)]
	public Impacts impacts = new Impacts();
	[Space(2)]
	public Drags drags = new Drags();
	[Space(2)]
	public Wind wind = new Wind();

    public AnimationCurve volumeCurve;

    // 附加的不相关属性  必要时从脚本配置。
    [NonSerialized] public float skidRatioChangeRate = 40.0f;
	[NonSerialized] public float offroadSpeedChangeRate = 20.0f;
	[NonSerialized] public float offroadCutoutSpeed = 0.02f;
	[NonSerialized] public float dragCutoutSpeed = 0.01f;
	[NonSerialized] public float turboRatioChangeRate = 8.0f;
	[NonSerialized] public float wheelsRpmChangeRateLimit = 400.0f;

	// 私有声明
	EVP.VehicleController m_vehicle;
	float m_engineRpm = 0.0f;
    float m_engineRpm_m = 0.0f;
    //float m_engineThrottleRpm = 0.0f;
    //float m_engineRpmDamp;
    /// <summary>
    /// 轮胎转速
    /// </summary>
    public float m_wheelsRpm = 0.0f;
	
	float m_skidRatio = 0.0f;
	float m_offroadSpeed = 0.0f;
	float m_lastScratchTime = 0.0f;
	float m_turboRatio = 0.0f;
	float m_lastTurboDumpTime = 0.0f;
	float m_lastThrottleInput = 0.0f;
	float m_lastThrottlePressedTime = 0.0f;

	WheelAudioData[] m_audioData = new WheelAudioData[0];


	// 对私有变量的公有访问
	public float simulatedEngineRpm { get { return m_engineRpm; } }
    public float simulatedMotorEngineRpm { get { return m_engineRpm_m; } }


    void OnEnable() {
		// 配置车辆：报告碰撞 计算扩展轮胎数据（用于打滑）
		m_vehicle = GetComponent<EVP.VehicleController>();
		m_vehicle.processContacts = true;
		m_vehicle.onImpact += DoImpactAudio;
		m_vehicle.computeExtendedTireData = true;

		avc= GetComponent<AdvancedVehicleController>();

		// 验证配置参数
		//m_engineRpmDamp = engine.gearUpRpmRate;
		m_wheelsRpm = 0.0f;

		// 验证音频是否属于当前车辆（仅编辑器）
		//VerifyAudioSources();
	}


	void OnDisable (){
		//StopAudio(engine.audioSource);
	}


	void Update(){
		DoEngineAudio();
		if (avc.vehicleState==AdvancedVehicleController.VehicleState.ON) {
            switch(avc.vehicle.type) {
                case AdvancedVehicleController.ControlType.EV:
                    DoMotorsAudio();
                    break;
                case AdvancedVehicleController.ControlType.HEV_EATON:
                    DoMotorsAudio();
                    DoEnginesAudio();
                    break;
                case AdvancedVehicleController.ControlType.HEV_CEP:
                    DoMotorsAudio();
                    DoEnginesAudio();
                    break;
                default:
                    if(avc.engine.state == AdvancedVehicleController.EngineState.Running) 
                        DoEnginesAudio();
                    break;
            }
            //播放喇叭
            if(Input.GetKeyDown(KeyCode.H)) {
                extra.horn.Play();
            }
            if(Input.GetKeyUp(KeyCode.H)) {
                extra.horn.Stop();
            }
        }
        DoEngineExtraAudio();
        DoBodyDragAudio();
		DoWindAudio();
		DoTireAudio();

		//缓存上一帧的油门输入
		m_lastThrottleInput = m_vehicle.throttleInput;
	}

    /// <summary>
    /// 播放启动音效
    /// </summary>
    public void DoStart() {
		PlayOneTime(engine.engineStart[UnityEngine.Random.Range(0,engine.engineStart.Length)], engine.audioEngine.transform.position, 1);
		//StartCoroutine(Startup());
	}
	public void DoStartBad() {
		PlayOneTime(engine.engineStartBad, engine.audioEngine.transform.position, 1);
	}
    public void DoStop() {
		PlayOneTime(engine.engineStop, engine.audioEngine.transform.position, 1);
		StartCoroutine(Shutdown());
	}

    public void DoPowerOn() {
        PlayOneTime(extra.powerOn,transform.position,1);
    }

    System.Collections.IEnumerator Startup() {
		yield return new WaitForSeconds(0.8f);
		//started = true;
	}
	System.Collections.IEnumerator Shutdown() {
		yield return new WaitForSeconds(0.3f);
		for (int i = 0; i < engines.Length; i++)
			engines[i].audioSource.volume = 0;
		
		//started = false;
	}

	/// <summary>
	/// 气刹释放
	/// </summary>
	public void BreakAirRelease() {
		if (extra.airBrakes == null || extra.airBrake == null)
			return;
		if (!extra.airBrake.isPlaying) {
			extra.airBrake.clip = extra.airBrakes[UnityEngine.Random.Range(0, extra.airBrakes.Length)];
			extra.airBrake.Play();
		}
	}

	//手刹音频 Release为拉起手刹
	public void HandBreakOn() {
		if (extra.handBrake == null)
			return;
		if (extra.handBrake.isPlaying)
			extra.handBrake.Stop();

		extra.handBrake.clip = extra.handBrakeRelease;
		extra.handBrake.Play();
		
	}
	public void HandBreakOff() {
		if (extra.handBrake == null)
			return;
		if (extra.handBrake.isPlaying)
			extra.handBrake.Stop();
		
		extra.handBrake.clip = extra.handBrakeCharge;
		extra.handBrake.Play();
	}

	public void BlinkStart() {
		if(!extra.blinker.isPlaying)
			extra.blinker.Play();
	}
    public void BlinkStop() {
		if (extra.blinker.isPlaying)
			extra.blinker.Stop();
	}
    public void ReverseStart() {
        if (!extra.reverser.isPlaying)
            extra.reverser.Play();
    }
    public void ReverseStop() {
        if (extra.reverser.isPlaying)
            extra.reverser.Stop();
    }

    //开门
    public void PlayOpenDoor(byte num) {
        if (extra.doorOpen == null || num>=extra.doorOpen.Length)
            return;
        PlayOneTime(extra.doorOpen[num], transform.position, 1.0f);
    }
    public void PlayCloseDoor(byte num) {
        if (extra.doorClose == null || num>=extra.doorClose.Length)
            return;
        PlayOneTime(extra.doorClose[num], transform.position, 1.0f);
    }


    //播放压缩机
    public void DoCompressorAudioStart() {
        PlayOneTime(extra.compressorOn,transform.position,0.3f);
        if(extra.compressor!=null)
            extra.compressor.PlayDelayed(0.3f);
    }
    public void DoCompressorAudioStop() {
        if(extra.compressor != null)
            extra.compressor.Stop();
        PlayOneTime(extra.compressorOff,transform.position,0.3f);
    }
    public void DoCompressorAudio() {
        ProcessContinuousAudio(extra.compressor, avc.engine.rpm / avc.engine.rpmMax, 0.2f,1.5f,0,0.6f);

    }

    public void DoGearChange() {
		audioClipTemplate.PlayOneShot(extra.gearChange[UnityEngine.Random.Range(0,extra.gearChange.Length)]);
		//PlayOneTime(extra.gearChange[UnityEngine.Random.Range(0,extra.gearChange.Length)],transform.position,1);
    }

    void FixedUpdate() {
		if (m_vehicle.wheelData.Length != m_audioData.Length)
			InitializeAudioData();
		// 基于运动速度而不是力。
		// 在车辆的FixedUpdate时在WheelData跟踪运动的速度 然后移动到 Update
		DoWheelBumpAudio();
	}


	void InitializeAudioData() {
		m_audioData = new WheelAudioData[m_vehicle.wheelData.Length];

		for (int i=0; i<m_audioData.Length; i++)
			m_audioData[i] = new WheelAudioData();
	}
    

    /// <summary>
    /// 发动机基本音效
    /// </summary>
	void DoEngineAudio (){
		// 获取驱动轮的平均转速
		float averageWheelRate = 0.0f;
		int driveWheels = 0;
		foreach (EVP.WheelData wd in m_vehicle.wheelData) {
			if(wd.wheel.drive)	{
				driveWheels++;
				averageWheelRate += wd.angularVelocity;
			}
		}
		if(driveWheels == 0) {
			//if (engine.audioSource != null)
			//	engine.audioSource.Stop();
			return;
		}
		averageWheelRate /= driveWheels;
		m_wheelsRpm = Mathf.MoveTowards(m_wheelsRpm, averageWheelRate * Mathf.Rad2Deg / 6.0f, wheelsRpmChangeRateLimit * Time.deltaTime);
		m_engineRpm = avc.engine.rpm;
	}

    /// <summary>
    /// 发动机分段音效
    /// </summary>
	void DoEnginesAudio() {
        m_engineRpm = avc.engine.rpm;
        for(int i = 0; i < engines.Length; i++) {
			if (engines[i].audioSource != null) {
				float engineRatio = Mathf.InverseLerp(engines[i].minRpm, engines[i].maxRpm, m_engineRpm);
				ProcessContinuousAudioPitch(engines[i].audioSource, engineRatio, engines[i].minPitch, engines[i].maxPitch);

                float engineVolume = Mathf.Lerp(engines[i].minVolume,engines[i].maxVolume,engineRatio);// + Mathf.Abs(m_vehicle.throttleInput) * 300;
				ProcessVolume(engines[i].audioSource, engineVolume * (1-avc.throttleInput) * volumeCurve.Evaluate((m_engineRpm- engines[i].minRpm)/(engines[i].maxRpm- engines[i].minRpm)), 10, 10);
				
			}
        }
        for (int i = 0;i < exhausts.Length;i++) {
            if (exhausts[i].audioSource != null) {
                float engineRatio = Mathf.InverseLerp(exhausts[i].minRpm, exhausts[i].maxRpm, m_engineRpm);
                ProcessContinuousAudioPitch(exhausts[i].audioSource, engineRatio, exhausts[i].minPitch, exhausts[i].maxPitch);

                float engineVolume = Mathf.Lerp(exhausts[i].minVolume, exhausts[i].maxVolume, engineRatio);// + Mathf.Abs(m_vehicle.throttleInput) * 300;
                ProcessVolume(exhausts[i].audioSource, engineVolume * avc.engine.throttle * volumeCurve.Evaluate((m_engineRpm - engines[i].minRpm) / (engines[i].maxRpm - engines[i].minRpm)), 10, 10);
            }
        }
    }

    /// <summary>
    /// 电动机音效
    /// </summary>
	void DoMotorsAudio() {
		m_engineRpm_m = avc.motor.rpm;
		for (int i = 0; i < motors.Length; i++) {
			if (motors[i].audioSource != null) {
				float motorRatio = Mathf.InverseLerp(motors[i].minRpm, motors[i].maxRpm, m_engineRpm_m);
				ProcessContinuousAudioPitch(motors[i].audioSource, motorRatio, motors[i].minPitch, motors[i].maxPitch);

                float motorVolume = Mathf.Lerp(motors[i].minVolume,motors[i].maxVolume,motorRatio);// + Mathf.Abs(m_vehicle.throttleInput) * 300;
				ProcessVolume(motors[i].audioSource, motorVolume * Mathf.Max(avc.throttleInput,avc.brakeInput) * volumeCurve.Evaluate((m_engineRpm_m - motors[i].minRpm) / (motors[i].maxRpm - motors[i].minRpm)), 8, 6);
			}
		}
	}


    /// <summary>
    /// 额外音效：涡轮、传动音效
    /// </summary>
	void DoEngineExtraAudio() {
		// 涡轮音效

		float updatedTurboRatio = Mathf.InverseLerp(engineExtras.turboMinRpm, engineExtras.turboMaxRpm, m_engineRpm);
		if (engineExtras.turboRequiresThrottle)
			updatedTurboRatio *= Mathf.Clamp01(m_vehicle.throttleInput);
		m_turboRatio = Mathf.Lerp(m_turboRatio, updatedTurboRatio, turboRatioChangeRate * Time.deltaTime);

		ProcessContinuousAudio(engineExtras.turboAudioSource, m_turboRatio,	engineExtras.turboMinPitch, engineExtras.turboMaxPitch, 0.0f, engineExtras.turboMaxVolume);

		// 涡轮停转音频

		if(engineExtras.turboDumpClip != null) {
			if(Time.time-m_lastTurboDumpTime > engineExtras.turboDumpMinInterval && m_engineRpm > engineExtras.turboDumpMinRpm) {
				bool throttleReleased = m_vehicle.throttleInput < 0.5f && (m_vehicle.throttleInput - m_lastThrottleInput) / Time.deltaTime < -20.0f;

				float throttlePressedTime = Time.time - m_lastThrottlePressedTime;
				if (m_vehicle.throttleInput < 0.2f) m_lastThrottlePressedTime = Time.time;

				if((throttleReleased) && throttlePressedTime > engineExtras.turboDumpMinThrottleTime) {
					Vector3 pos = engineExtras.turboAudioSource != null? engineExtras.turboAudioSource.transform.position : m_vehicle.cachedTransform.position;
					PlayOneTime(engineExtras.turboDumpClip, pos, engineExtras.turboDumpVolume);
					m_lastTurboDumpTime = Time.time;
				}
			}
		}

        // 传动音效

        //float transmissionRatio = Mathf.Abs(m_wheelsRpm * engine.wheelsToEngineRatio) / (engine.maxRpm * engine.gears * engineExtras.transmissionMaxRatio);
        float transmissionRatio = Mathf.Abs(m_wheelsRpm * avc.vehicle.mainRatio / (4000 * engineExtras.transmissionMaxRatio));/// avc.transRatio);

        ProcessContinuousAudio(engineExtras.transmissionAudioSource, transmissionRatio,
		engineExtras.transmissionMinPitch, engineExtras.transmissionMaxPitch, engineExtras.transmissionMinVolume, engineExtras.transmissionMaxVolume);
	}



    /// <summary>
    /// 轮胎音效
    /// </summary>
	void DoTireAudio() {
		float currentSkidRatio = 0.0f;
		float currentOffroadSpeed = 0.0f;
		int offroadWheels = 0;

		// 用到所有轮子的打滑总和： 单个轮子打滑最大值导致最大值
		// 越野用到越野面上所以轮子平均值
		// Skid uses the sum of all wheels: a single wheel skidding to the top causes maximum value.
		// Offroad uses the average value of all wheels over offroad surface.

		foreach(EVP.WheelData wd in m_vehicle.wheelData) {
			// 如果没找到地面材质，默认为"hard" (打滑音效)

			if(wd.groundMaterial == null || wd.groundMaterial.surfaceType == EVP.GroundMaterial.SurfaceType.Hard) {
				// 滑移值是根据实际参数计算的滑移率之和。
				currentSkidRatio += Mathf.InverseLerp(wheels.skidMinSlip, wheels.skidMaxSlip, wd.combinedTireSlip);
			}
			else {
				// 越野值是在表面上的轮胎的平均速度。Offroad value is the average velocity of the tire over the surface among all wheels.
				if(wd.grounded) {
					currentOffroadSpeed += wd.velocity.magnitude + Mathf.Abs(wd.tireSlip.y);
					offroadWheels++;
				}
			}
		}

		// 滑值接收滑移率基于 wheels.skidMinSlip 和 wheels.skidMaxSlip
		m_skidRatio = Mathf.Lerp(m_skidRatio, currentSkidRatio, skidRatioChangeRate * Time.deltaTime);
		ProcessContinuousAudio(wheels.skidAudioSource, m_skidRatio, wheels.skidMinPitch, wheels.skidMaxPitch, 0.0f, wheels.skidMaxVolume);

		// 越野值受到车轮的实际速度在表面。
		// 它分成两个直线范围：
		//   - 从断流到最小
		//	 - 从最小到最大

		if (offroadWheels > 1) currentOffroadSpeed /= offroadWheels;
		m_offroadSpeed = Mathf.Lerp(m_offroadSpeed, currentOffroadSpeed, offroadSpeedChangeRate * Time.deltaTime);

		ProcessSpeedBasedAudio(wheels.offroadAudioSource,
			m_offroadSpeed, offroadCutoutSpeed, wheels.offroadMinSpeed, wheels.offroadMaxSpeed,
			0.0f, wheels.offroadMinPitch, wheels.offroadMaxPitch,
			wheels.offroadMinVolume, wheels.offroadMaxVolume);
	}

    /// <summary>
    /// 车体碰撞
    /// </summary>
	void DoImpactAudio() {
		// 车体碰撞
		if(impacts.hardImpactAudioClip != null || impacts.softImpactAudioClip != null) {
			float impactSpeed = m_vehicle.localImpactVelocity.magnitude;

			if(impactSpeed > impacts.minSpeed){
				float impactRatio = Mathf.InverseLerp (impacts.minSpeed, impacts.maxSpeed, impactSpeed);
				AudioClip clip = null;

				if(!impacts.softImpactAudioClip) {
					clip = impacts.hardImpactAudioClip;
				}else{
					clip = m_vehicle.isHardImpact? impacts.hardImpactAudioClip : impacts.softImpactAudioClip;
				}

				if(clip) {
					PlayOneTime(clip,m_vehicle.cachedTransform.TransformPoint(m_vehicle.localImpactPosition),
						Mathf.Lerp(impacts.minVolume,impacts.maxVolume,impactRatio) + UnityEngine.Random.Range(-impacts.randomVolume,impacts.randomVolume),
						Mathf.Lerp(impacts.minPitch,impacts.maxPitch,impactRatio) + UnityEngine.Random.Range(-impacts.randomPitch,impacts.randomPitch));
					GlobalClass.Log("碰撞",LogType.Warning);
				}
			}
		}
	}

    /// <summary>
    /// 连续拖动音频
    /// </summary>
	void DoBodyDragAudio() {
		// 连续拖动音频

		float dragSpeed = m_vehicle.localDragVelocity.magnitude;
		float hardDragSpeed = m_vehicle.isHardDrag? dragSpeed : 0.0f;
		float softDragSpeed = m_vehicle.isHardDrag? 0.0f : dragSpeed;

		ProcessSpeedBasedAudio(drags.hardDragAudioSource, hardDragSpeed, dragCutoutSpeed, drags.minSpeed, drags.maxSpeed,
			0.0f, drags.minPitch, drags.maxPitch, drags.minVolume, drags.maxVolume);

		ProcessSpeedBasedAudio(drags.softDragAudioSource, softDragSpeed, dragCutoutSpeed, drags.minSpeed, drags.maxSpeed,
			0.0f, drags.minPitch, drags.maxPitch, drags.minVolume, drags.maxVolume);

		// 在坚硬的表面时随机车身刮擦声音
		if (drags.scratchAudioClip != null) {
			if (dragSpeed > drags.scratchMinSpeed && m_vehicle.isHardDrag
				&& UnityEngine.Random.value < drags.scratchRandomThreshold
				&& Time.time-m_lastScratchTime > drags.scratchMinInterval) {
				PlayOneTime(drags.scratchAudioClip, m_vehicle.cachedTransform.TransformPoint(m_vehicle.localDragPosition),
					UnityEngine.Random.Range(drags.scratchMinVolume, drags.scratchMaxVolume),
					UnityEngine.Random.Range(drags.scratchMinPitch, drags.scratchMaxPitch));
				GlobalClass.Log("碰撞",LogType.Warning);
				m_lastScratchTime = Time.time;
			}
		}
	}

	void DoWheelBumpAudio() {
		if (wheels.bumpAudioClip == null) return;

		for(int i=0, c=m_vehicle.wheelData.Length; i<c; i++) {
			EVP.WheelData wd = m_vehicle.wheelData[i];
			WheelAudioData ad = m_audioData[i];

			// 先处理车轮颠簸
			float suspensionForceDelta = wd.downforce - ad.lastDownforce;
			ad.lastDownforce = wd.downforce;

			if(suspensionForceDelta > wheels.bumpMinForceDelta && (Time.fixedTime - ad.lastWheelBumpTime) > 0.03f) {
				ProcessWheelBumpAudio(suspensionForceDelta, wd.transform.position);
				ad.lastWheelBumpTime = Time.fixedTime;
			}
		}
	}

	void DoWindAudio() {
		float windRatio = Mathf.InverseLerp(wind.minSpeed, wind.maxSpeed, m_vehicle.cachedRigidbody.velocity.magnitude);

		ProcessContinuousAudio(wind.windAudioSource, windRatio, wind.minPitch, wind.maxPitch, 0.0f, wind.maxVolume);
	}


	//----------------------------------------------------------------------------------------------
	/// <summary>
	/// 停止音频
	/// </summary>
	/// <param name="audio">音频源</param>
	void StopAudio (AudioSource audio){
		if (audio != null) audio.Stop();
	}
    /// <summary>
    /// 停止正在播放的音频
    /// </summary>
    /// <param name="audio">音频源</param>
    void StopAudio2(AudioSource audio) {
		if (audio.isPlaying)
			audio.Stop();
	}

	/// <summary>
	/// 计算连续的音频音调和音量
	/// </summary>
	/// <param name="audio"></param>
	/// <param name="ratio">比率</param>
	/// <param name="minPitch">最低音调</param>
	/// <param name="maxPitch">最高音调</param>
	/// <param name="minVolume">最低音量</param>
	/// <param name="maxVolume">最高音量</param>
	public void ProcessContinuousAudio (AudioSource audio, float ratio, float minPitch, float maxPitch, float minVolume, float maxVolume){
		if (audio == null) return;

		audio.pitch = Mathf.Lerp(minPitch, maxPitch, ratio);
		audio.volume = Mathf.Lerp(minVolume, maxVolume, ratio);

		if (!audio.isPlaying) audio.Play();
		audio.loop = true;
	}

	/// <summary>
	/// 计算连续的音频音调
	/// </summary>
	/// <param name="audio"></param>
	/// <param name="ratio">比率</param>
	/// <param name="minPitch">最低音调</param>
	/// <param name="maxPitch">最高音调</param>
	void ProcessContinuousAudioPitch (AudioSource audio, float ratio, float minPitch, float maxPitch){
		if (audio == null) return;

		audio.pitch = Mathf.Lerp(minPitch, maxPitch, ratio);

		if (!audio.isPlaying) audio.Play();
		audio.loop = true;
	}


	void ProcessVolume(AudioSource audio, float volume, float changeRateUp, float changeRateDown) {
		float changeRate = volume > audio.volume? changeRateUp : changeRateDown;
		audio.volume = Mathf.Lerp(audio.volume, volume, Time.deltaTime * changeRate);
	}


	void ProcessSpeedBasedAudio(AudioSource audio, float speed, float cutoutSpeed, float minSpeed, float maxSpeed, float cutoutPitch, float minPitch, float maxPitch, float minVolume, float maxVolume) {
		if(audio == null) return;

		if(speed < cutoutSpeed) {
			if (audio.isPlaying) audio.Stop();
		}
		else {
			if(speed < minSpeed) {
				float ratio = Mathf.InverseLerp(cutoutSpeed, minSpeed, speed);
				audio.pitch = Mathf.Lerp(cutoutPitch, minPitch, ratio);
				audio.volume = Mathf.Lerp(0.0f, minVolume, ratio);
			}
			else {
				float ratio = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
				audio.pitch = Mathf.Lerp(minPitch, maxPitch, ratio);
				audio.volume = Mathf.Lerp(minVolume, maxVolume, ratio);
			}

			if (!audio.isPlaying) audio.Play();
			audio.loop = true;
		}
	}

	// 处理车轮碰撞音频
	void ProcessWheelBumpAudio(float suspensionForceDelta, Vector3 position) {
		float bumpRatio = Mathf.InverseLerp(wheels.bumpMinForceDelta, wheels.bumpMaxForceDelta, suspensionForceDelta);
		PlayOneTime(wheels.bumpAudioClip, position, Mathf.Lerp(wheels.bumpMinVolume, wheels.bumpMaxVolume, bumpRatio));
	}


	// 播放单次声音特效
	// Playing audio effects.
	// 不用 AudioSource.PlayClipAtPoint 因为我们要让声音作为车辆的子物体。

	void PlayOneTime (AudioClip clip, Vector3 position, float volume)	{
		PlayOneTime(clip, position, volume, 1.0f);
	}

	void PlayOneTime(AudioClip clip, Vector3 position, float volume, float pitch) {
		if (clip == null || pitch < 0.01f || volume < 0.01f) return;

		GameObject go;
		AudioSource source;

		if (audioClipTemplate != null){
			go = Instantiate(audioClipTemplate.gameObject,position,Quaternion.identity);
			source = go.GetComponent<AudioSource>();
			go.transform.parent = audioClipTemplate.transform.parent;
		}
		else{
			go = new GameObject("One shot audio");
			go.transform.parent = m_vehicle.cachedTransform;
			go.transform.position = position;
			source = null;
		}

		if (source == null){
			source = go.AddComponent<AudioSource>() as AudioSource;
			source.spatialBlend = 1.0f;
		}

		source.clip = clip;
		source.volume = volume;
		source.pitch = pitch;
		source.dopplerLevel = 0.0f;		// Doppler causes artifacts as for positioning the audio source
		source.Play();
		Destroy(go, clip.length / pitch);
	}

}

