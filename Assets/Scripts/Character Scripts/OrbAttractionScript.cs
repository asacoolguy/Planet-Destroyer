using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script attached to Player's coin attraction field
/// </summary>
public class OrbAttractionScript : MonoBehaviour {
	private float attractionRadius, attractionSpeed;

	void Awake(){
		this.attractionRadius = transform.parent.GetComponent<PlayerScript>().coinAttractionRadius * transform.parent.transform.lossyScale.x;
		GetComponent<CircleCollider2D>().radius = attractionRadius / transform.lossyScale.x;
		this.attractionSpeed = transform.parent.GetComponent<PlayerScript>().coinAttractionSpeed;
	}
	

	void OnTriggerStay2D(Collider2D other){
		if(other.gameObject.tag == "CoinOrb" && other.gameObject.GetComponent<OrbScript>().GetIsCollectable()){
			Vector3 direction = (this.transform.position - other.transform.position).normalized;
			float orbRadius = other.gameObject.GetComponent<CircleCollider2D>().radius * other.gameObject.transform.lossyScale.x;
			float distance = Vector3.Distance(this.transform.position, other.transform.position) - orbRadius;
			// if the player is close enough, then just fly over top speed
			if (other.gameObject.name == "CoinOrb"){
				print(distance / attractionRadius);
			}

			other.gameObject.GetComponent<OrbScript>().SetAttractionVelocity(direction * attractionSpeed, distance / attractionRadius);
		}
	}

	void OnTriggerExit2D(Collider2D other){
		if (other.gameObject.tag == "CoinOrb"){
			// decay the coin's attraction velocity
			other.gameObject.GetComponent<OrbScript>().StopAttractionVelocity();
		}
	}
}
