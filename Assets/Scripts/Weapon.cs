using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public AudioClip fireSound;
	public AudioClip finishedFireSound;
	public AudioClip singleShotSound;
	ParticleEmitter[] emitters;
	
	bool hasFinishedFiring = false;
	bool hasPlayedFinishedFireSound = true;
	float loopFireTimer = 0;
	public float loopFireTime = 0.05f;
	
	bool isFireSoundDue = false;
	
	// Use this for initialization
	void Start () {
		emitters = GetComponentsInChildren<ParticleEmitter>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.touches.Length == 1 || Input.GetMouseButton(0)) {
			hasFinishedFiring = false;
			hasPlayedFinishedFireSound = false;
			
			emit();
			
			if(loopFireTimer > loopFireTime) {
				if(!audio.isPlaying && !audio.loop) {
					audio.loop = true;
					audio.clip = fireSound;
					audio.Play();
				}
				isFireSoundDue = false;
			} else {
				isFireSoundDue = true;
			}
			
			loopFireTimer += Time.deltaTime;
		} else {		
			if(!hasPlayedFinishedFireSound) {
				audio.loop = false;
				audio.PlayOneShot(finishedFireSound);
				loopFireTimer = 0;
				hasPlayedFinishedFireSound = true;
			}
			
			if(!hasFinishedFiring) {
				noEmit();
				hasFinishedFiring = true;
			}
			
			if(isFireSoundDue) {
				isFireSoundDue = false;
				audio.PlayOneShot(singleShotSound);
			}
			
			loopFireTimer = 0;
		}
		
		
	}
	
	void emit() {
		foreach(ParticleEmitter emitter in emitters) {
			emitter.emit = true;
		}
	}
	
	void noEmit() {
		foreach(ParticleEmitter emitter in emitters) {
			emitter.emit = false;
		}
	}
}
