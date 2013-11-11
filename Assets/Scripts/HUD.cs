using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
	Rect crosshairPos;
	TextMesh clipAmmo;
	TextMesh ammoSupply;
	
	Weapon equippedWeapon;
	public Texture2D crossHair;
	
	public static HUD use;

	// Use this for initialization
	void Start () {
		clipAmmo = GameObject.Find("ClipAmmo").GetComponent<TextMesh>();
		ammoSupply = GameObject.Find ("AmmoSupply").GetComponent<TextMesh>();
		
		//TODO: proper weapon equip system
		equippedWeapon = GameObject.Find("M4").GetComponent<Weapon>();
		
		crosshairPos = new Rect(MainSystem.ScreenWidth/2-crossHair.width/2, MainSystem.ScreenHeight/2-crossHair.height/2, crossHair.width, crossHair.height);
	}
	
	// Update is called once per frame
	void Update () {
		clipAmmo.text = ""+equippedWeapon.ammo;
		ammoSupply.text = ""+equippedWeapon.ammoSupply;
	}
	
	void OnGUI() {
		GUI.DrawTexture(crosshairPos, crossHair);
		if(WebcamLook.webcamError != "") {
			GUILayout.Label(WebcamLook.webcamError);
		}
	}
}
