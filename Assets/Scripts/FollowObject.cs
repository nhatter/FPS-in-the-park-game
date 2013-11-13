using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour {
	public GameObject targetObject;
	public Vector3 offset;
	
	// Update is called once per frame
	void Update () {
		if(targetObject != null) {
			this.transform.position = targetObject.transform.position + offset;
		}
	}
}
