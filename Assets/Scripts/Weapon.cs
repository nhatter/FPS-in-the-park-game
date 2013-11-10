using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public int ammo = 30;
	public float fireRate = 0.1f;
	
	GameObject soldier;
	GameObject casingEject;
	float oldCasingEjectMaxEmission;
	
	public AudioClip fireSound;
	public AudioClip finishedFireSound;
	public AudioClip singleShotSound;
	ParticleEmitter[] emitters;
	
	bool hasFinishedFiring = false;
	bool hasPlayedFinishedFireSound = true;
	float loopFireTimer = 0;
	float autoFireTimer = 0;
	public float loopFireTime = 0.05f;
	
	bool isFireSoundDue = false;
	
	// Use this for initialization
	void Start () {
		soldier = GameObject.Find("Soldier");
		casingEject = GameObject.Find("CasingEject");
		oldCasingEjectMaxEmission = casingEject.particleEmitter.maxEmission;
		
		emitters = GetComponentsInChildren<ParticleEmitter>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.touches.Length == 1 || Input.GetMouseButton(0)){
			if(ammo > 0) {
				hasFinishedFiring = false;
				hasPlayedFinishedFireSound = false;
				
				emit();
				
				autoFireTimer += Time.deltaTime;
				if(autoFireTimer > fireRate) {
					ammo--;
					autoFireTimer = 0;
				}
				
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
				
				soldier.animation.Play("StandingFire");
				loopFireTimer += Time.deltaTime;
			} else {
				audio.loop = false;
				isFireSoundDue = true;
				noEmit();
				hasFinishedFiring = true;
				autoFireTimer = 0;
			}
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
				autoFireTimer = 0;
			}
			
			if(isFireSoundDue) {
				isFireSoundDue = false;
				if(ammo == 0) {
					audio.PlayOneShot(GenericSounds.use.outOfAmmo);
				} else {
					audio.PlayOneShot(singleShotSound);
					casingEject.particleEmitter.maxEmission = 1;
					casingEject.particleEmitter.Emit();
					ammo--;
				}
			}
			
			loopFireTimer = 0;
		}
		
		
	}
	
	void emit() {
		foreach(ParticleEmitter emitter in emitters) {
			emitter.emit = true;
		}
		
		casingEject.particleEmitter.maxEmission = oldCasingEjectMaxEmission;
	}
	
	void noEmit() {
		foreach(ParticleEmitter emitter in emitters) {
			emitter.emit = false;
		}
	}
}
