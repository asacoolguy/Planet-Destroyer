using UnityEngine;
using System.Collections;

/// <summary>
/// Attached to planets' "gravitation influence sphere" to determine when a planet's gravity should start infuencing the player
/// Upon detecting that a player had entered the range, adds thi planet to the player's list of nearByPlanets
/// </summary>
public class PlanetGravityScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.gameObject.tag == "Player" && !other.GetComponent<PlayerScript>().HasNearByPlanet(this.transform.parent.gameObject)) {
			other.gameObject.GetComponent<PlayerScript>().AddNearByPlanet(this.transform.parent.gameObject);
		}
	}
	
	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.tag == "Player" && other.GetComponent<PlayerScript>().HasNearByPlanet(this.transform.parent.gameObject)) {
			other.gameObject.GetComponent<PlayerScript>().RemoveNearByPlanet(this.transform.parent.gameObject);
		}
	}

}
