using UnityEngine;
using System.Collections;

namespace EVP {
    [RequireComponent(typeof(VehicleController))]
    public class VehicleVisuals : MonoBehaviour {
	    [Header("Steering wheel")]
	    public Transform steeringWheel;
	    public float degreesOfRotation = 420.0f;
        
        VehicleController m_vehicle;

	    void OnEnable() {
		    m_vehicle = GetComponent<VehicleController>();
	    }

	    void Update() {
		    if (steeringWheel != null) {
			    Vector3 angles = steeringWheel.localEulerAngles;
			    angles.z = 0.5f * degreesOfRotation * m_vehicle.steerAngle / m_vehicle.maxSteerAngle;
			    steeringWheel.localEulerAngles = angles;
		    }
	    }
    }
}