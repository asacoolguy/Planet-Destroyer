using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainSpaceScript : PlayerScript {
	public float homingBoosterSpeed;

	public override void ActivatePlayerAction(){
		if (!isLanded && canAction){
			StartCoroutine(ActivateTractorBeam());

			canAction = false;
			actionTaken = true;
		}
	}

	// captainSpace's action allows player to fly back to home planet at a certain speed until it lands a planet
	private IEnumerator ActivateTractorBeam(){
		Vector2 newVelocity = transform.position.normalized * homingBoosterSpeed * -1;
		playerAudio.PlayTractorBeamSound();
		while(!isLanded){
			GetComponent<Rigidbody2D>().velocity = newVelocity;
			canJump = false;

			yield return null;

		}
		playerAudio.StopTractorBeamSound();
	}



	/*IEnumerator actionBoost(){
		while(boosting){
			GetComponent<Rigidbody2D>().velocity += new Vector2(transform.position.normalized.x * boostStrength,
			                                                    transform.position.normalized.y * boostStrength);
			//print ("boosted " + currentBoostDuration);
			currentBoostDuration -= 1;
			if (currentBoostDuration < 0){
				//print ("finished");
				boosting = false;
				currentBoostDuration = boostDuration;
			}
			yield return 0;
		}
	}*/
}
