using UnityEngine;
using System.Collections;

public class WebcamLook : MonoBehaviour {
	public string[] deviceNames;
	public static string webcamError;
	WebCamTexture wct;
	public static Camera gameCamera;

	// Use this for initialization
	void Start () {
		gameCamera = GameObject.Find("GameCamera").camera;
		
		WebCamDevice[] devices = WebCamTexture.devices;
		deviceNames = new string[devices.Length];
		
		for(int i=0;i<devices.Length;i++) {
			deviceNames[i] = devices[i].name;
		}
		
		GameObject deviceCameraTexture = GameObject.Find("DeviceCameraTexture");
		wct = new WebCamTexture(devices[0].name);
		deviceCameraTexture.guiTexture.texture = wct;
		deviceCameraTexture.guiTexture.pixelInset =new Rect(-Screen.height/2,-Screen.width/2,Screen.height,Screen.width);
			
		if(wct != null) {
			wct.Play();
		} else {
			webcamError = "There was an error accessing the device's camera";
		}
		
		Screen.orientation = ScreenOrientation.LandscapeLeft;
	}
}
