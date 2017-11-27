using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dedicated to playing various sound effects for the player object
/// </summary>
public class PlayerAudioScript : MonoBehaviour {
	public AudioClip landingSound, leavingSound, deathSound, jetpackSound, chargedSound, 
		respawnSound, orbSound, chargingSound, maxedChargingSound, collisionSound, tractorBeamSound;
	private AudioSource audioSource, orbAudioSource, loopAudioSource;

	private bool playingChargeSound, playingTractorBeamSound, playingJetpackSound;

	// Use this for initialization
	void Awake () {
		audioSource = GetComponent<AudioSource>();
		orbAudioSource = transform.Find("Orb Audio").GetComponent<AudioSource>();
		loopAudioSource = transform.Find("Loop Audio").GetComponent<AudioSource>();
		loopAudioSource.loop = true;
		playingChargeSound = false;
		playingTractorBeamSound = false;
		playingJetpackSound = false;
	}

	void Update(){
		if ((playingChargeSound || playingTractorBeamSound || playingJetpackSound) && loopAudioSource.isPlaying == false){
			loopAudioSource.Play();
		}
		else if(!playingChargeSound && !playingTractorBeamSound && !playingJetpackSound){
			loopAudioSource.Stop();
		}
	}

	public void PlayLandingSound(){
		audioSource.PlayOneShot(landingSound);
	}

	public void PlayLeavingSound(){
		audioSource.PlayOneShot(leavingSound);
	}

	public void PlayCollisionSound(){
		audioSource.PlayOneShot(collisionSound);
	}

	public void PlayDeathSound(){
		audioSource.PlayOneShot(deathSound);
	}

	public void PlayRespawnSound(){
		audioSource.PlayOneShot(respawnSound);
	}

	public void PlayChargedSound(){
		audioSource.PlayOneShot(chargedSound);
	}

	public void PlayChargingSound(){
		StopTractorBeamSound();
		loopAudioSource.pitch = 1f;
		loopAudioSource.clip = chargingSound;
		playingChargeSound = true;
	}

	public void PlayMaxedChargingSound(){
		loopAudioSource.clip = maxedChargingSound;
		loopAudioSource.pitch = 1.2f;
	}

	public void StopChargingSound(){
		loopAudioSource.pitch = 1f;
		playingChargeSound = false;
	}

	public void PlayTractorBeamSound(){
		StopChargingSound();
		loopAudioSource.pitch = 1.2f;
		loopAudioSource.clip = tractorBeamSound;
		playingTractorBeamSound = true;
	}

	public void StopTractorBeamSound(){
		loopAudioSource.pitch = 1f;
		playingTractorBeamSound = false;
	}

	public void PlayJetpackSound(){
		StopChargingSound();
		loopAudioSource.pitch = 1.6f;
		loopAudioSource.clip = jetpackSound;
		playingJetpackSound = true;
	}

	public void StopJetpackSound(){
		loopAudioSource.pitch = 1f;
		playingJetpackSound = false;
	}

	public void PlayOrbSound(int combo){
		orbAudioSource.pitch = 1f + (combo - 1f) * 0.1f;
		orbAudioSource.PlayOneShot(orbSound);
	}
}
