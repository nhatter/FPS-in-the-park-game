using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public int ammo = 30;
	public int maxAmmoInClip = 30;
	public int ammoSupply = 90;
	public float vSwipeDistanceToReload = 10.0f;
	public float fireRate = 0.1f;
	
	public AudioClip fireSound;
	public AudioClip finishedFireSound;
	public AudioClip singleShotSound;
	public AudioClip reloadSound;
	
	GameObject soldier;
	GameObject casingEject;
	float oldCasingEjectMaxEmission;
	

		
	ParticleEmitter[] emitters;
	
	bool hasFinishedFiring = true;
	bool hasPlayedFinishedFireSound = true;
	float loopFireTimer = 0;
	float autoFireTimer = 0;
	public float loopFireTime = 0.05f;
	
	bool isFireSoundDue = false;
	bool isReloading = false;
	bool isReadyToRestockAmmo = false;
	bool reloadRequested = false;
	
	
	// Use this for initialization
	void Start () {		
		soldier = GameObject.Find("Soldier");
		casingEject = GameObject.Find("CasingEject");
		oldCasingEjectMaxEmission = casingEject.particleEmitter.maxEmission;
		
		emitters = GetComponentsInChildren<ParticleEmitter>();
	}
	
	// Update is called once per frame
	Touch touch;
	void Update () {
		if(isReloading && animation.IsPlaying("StandingReloadM4")) {
			isReadyToRestockAmmo = true;
		}
		
		if(isReadyToRestockAmmo && !animation.IsPlaying("StandingReloadM4")) {
			if(ammoSupply > 0) {
				if(ammoSupply - maxAmmoInClip >= 0) {
					ammo = maxAmmoInClip;
					ammoSupply -= maxAmmoInClip;
				} else {
					ammo = ammoSupply;
					ammoSupply = 0;
				}
			}
			
			isReadyToRestockAmmo = false;
			isReloading = false;
		}
		
		#if UNITY_EDITOR
		if(Input.GetKey(KeyCode.R)) {
			reloadRequested = true;
		} else {
			reloadRequested = false;
		}
		#endif
		
		if(Input.touches.Length == 1 || Input.GetMouseButton(0)) {
			#if !UNITY_EDITOR
			touch = Input.GetTouch(0);
			if(touch.phase == TouchPhase.Moved) {
				if(touch.deltaPosition.y < -vSwipeDistanceToReload) {
					reloadRequested = true;
				}
			} else {
				reloadRequested = false;
			}
			#endif
			
			if(!isReloading) {
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
					if(!hasPlayedFinishedFireSound && !hasFinishedFiring) {
						audio.loop = false;
						audio.PlayOneShot(finishedFireSound);
						loopFireTimer = 0;
						hasPlayedFinishedFireSound = true;
					}	
						
					audio.loop = false;
					isFireSoundDue = true;
					noEmit();
					hasFinishedFiring = true;
					autoFireTimer = 0;
				}
			}
		} else {
		
			if(!hasPlayedFinishedFireSound && !hasFinishedFiring) {
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
		
		
		if(reloadRequested && hasFinishedFiring && !isReloading && ammo < maxAmmoInClip && ammoSupply > 0) {
			audio.PlayOneShot(reloadSound);
			isReloading = true;
			soldier.animation.Play("StandingReloadM4");
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
