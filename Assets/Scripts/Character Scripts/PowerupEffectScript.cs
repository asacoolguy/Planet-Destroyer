using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupEffectScript : MonoBehaviour {
	private GameObject barrier, charge, lighting, magnet;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// called by parent player to show the animation for a certain power up
	public void ActivateEffect(PowerupScript.PowerupType type){

	}

	// call by parent player to show the flashing animation for a waning power up
	public void ActivateEffectFlash(PowerupScript.PowerupType type){

	}

	// called by parent player to deactivate all animations when power up is gone
	public void DeactivateAllEffects(){
		
	}

}
