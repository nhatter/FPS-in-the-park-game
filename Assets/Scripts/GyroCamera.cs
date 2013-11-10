// PFC - prefrontal cortex
// Full Android Sensor Access for Unity3D
// Contact:
// 		contact.prefrontalcortex@gmail.com

using UnityEngine;
using System.Collections;

public class GyroCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Use mouselook if not using a mobile device
		#if UNITY_EDITOR
			this.gameObject.AddComponent<MouseLook>();
		#endif

		SensorHelper.ActivateRotation();
		
		useGUILayout = false;
	}
	
	// Update is called once per frame
	void Update () {
#if !UNITY_EDITOR
		transform.rotation = SensorHelper.rotation;
#endif
	}
}