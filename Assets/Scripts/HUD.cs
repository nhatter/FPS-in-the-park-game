using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
	Rect crosshairPos;
	public Texture2D crossHair;
	
	public static HUD use;

	// Use this for initialization
	void Start () {
		crosshairPos = new Rect(MainSystem.ScreenWidth/2-crossHair.width/2, MainSystem.ScreenHeight/2-crossHair.height/2, crossHair.width, crossHair.height);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI() {
		GUI.DrawTexture(crosshairPos, crossHair);
		if(WebcamLook.webcamError != "") {
			GUILayout.Label(WebcamLook.webcamError);
		}
	}
}
