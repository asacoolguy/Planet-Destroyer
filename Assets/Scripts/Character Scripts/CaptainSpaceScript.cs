using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainSpaceScript : PlayerScript {
	[SerializeField]private float tractorBeamSpeed;
	private LineRenderer tractorBeam;

	private void Awake(){
		base.Awake();
		tractorBeam = GetComponent<LineRenderer>();
	}

	public override void ActivatePlayerAction(){
		if (!isLanded && canAction){
			StartCoroutine(ActivateTractorBeam());

			canAction = false;
			actionTaken = true;
		}
	}

	// captainSpace's action allows player to fly back to home planet at a certain speed until it lands a planet
	private IEnumerator ActivateTractorBeam(){
		tractorBeam.enabled = true;
		StartCoroutine(EnableTail(false, 1f));

		Vector2 newVelocity = transform.position.normalized * tractorBeamSpeed * -1;
		playerAudio.PlayTractorBeamSound();
		while(!isLanded){
			GetComponent<Rigidbody2D>().velocity = newVelocity;
			tractorBeam.SetPosition(1, transform.position);
			canJump = false;
			yield return null;

		}
		tractorBeam.enabled = false;
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
