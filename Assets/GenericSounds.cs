using UnityEngine;
using System.Collections;

public class GenericSounds : MonoBehaviour {
	public static GenericSounds use;
	public AudioClip outOfAmmo;
	
	void Start() {
		use = this;
	}

}
