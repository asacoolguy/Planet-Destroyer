using UnityEngine;
using System.Collections;

public class OrbScript : MonoBehaviour {
	public int pointValue;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// if a player enters the range, let the player pick this up
	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Player") {
			other.gameObject.GetComponent<PlayerScript>().AcquiredOrb(pointValue, transform.position);
			Destroy(this.gameObject);
		}
	}

}
