using UnityEngine;
using System.Collections;

public class GunSway : MonoBehaviour {
	public float gunSteadySpeed = 5.0f;
	GameObject neck;
		
	void Start() {
		neck = ComponentUtils.FindTransformInChildren(gameObject, "Neck1").gameObject;
	}

	void LateUpdate () {
		transform.rotation = Quaternion.Slerp(transform.rotation, WebcamLook.gameCamera.transform.rotation, Time.deltaTime * gunSteadySpeed);
		
		// Stop model head getting in the way of the gun
		neck.transform.localScale = Vector3.zero;
	}
}