using UnityEngine;
using System.Collections;

public class AdjustToAmbientLight : MonoBehaviour {
	public float ambientLight;
	public float lightSensorScale = 1/5000;
	public float minimumLight = -3;
	public float maximumLight = 2;
	public float adjustToLightTime = 0.25f;

	// Use this for initialization
	void Start () {
		Sensor.Activate(Sensor.Type.Light);
	}
	
	// Update is called once per frame
	void Update () {
		ambientLight = minimumLight+(Sensor.light * lightSensorScale);
		
		if(ambientLight > maximumLight) {
			ambientLight = maximumLight;
		}

		RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, new Color(ambientLight,ambientLight,ambientLight), adjustToLightTime * Time.fixedTime);
	}
}
