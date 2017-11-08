using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to power up items. Use the enum to choose which type of power up this is.
/// Barrier: adds a protective bubble to the player. saves them from 1 death.
/// Charge: keeps player's powerlevel max for the duration
/// LightingBolt: increase player speed and mass for the duration. 
/// Coin Magnet: attracts nearby coins for the duration
/// </summary>

public class PowerupScript : MonoBehaviour {
	public enum PowerupType{
		none, barrier, charge, lighting, magnet
	};

	public PowerupType type;	
	private int powerupDuration; 
	public AudioClip collectionSound;

	// Use this for initialization
	void Start () {
		if (type == PowerupType.barrier){
			powerupDuration = -1;
			GetComponent<SpriteRenderer>().color = Color.blue;
		}
		else if(type == PowerupType.charge){
			powerupDuration = 15;
			GetComponent<SpriteRenderer>().color = Color.red;
		}
		else if(type == PowerupType.lighting){
			powerupDuration = 15;
			GetComponent<SpriteRenderer>().color = Color.cyan;
		}
		else if (type == PowerupType.magnet){
			powerupDuration = 15;
			GetComponent<SpriteRenderer>().color = Color.yellow;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// if a player enters the range, let the player pick this up
	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Player") {
			other.gameObject.GetComponent<PlayerScript>().AcquiredPowerup(type, powerupDuration, collectionSound);
			Destroy(this.gameObject);
		}
	}
}
