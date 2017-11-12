using UnityEngine;
using System.Collections;

public class OrbScript : MonoBehaviour {
	public int pointValue;
	private Vector3 blastVelocity, attractionVelocity;
	private bool isCollectable, attracted;
	public float speedDecayTime;
	private float currentSpeedDecayTime;
	[Range (0f, 0.5f)][SerializeField]
	private float speedNoiseFactor;

	// Use this for initialization
	void Awake () {
		isCollectable = false;
		attracted = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (blastVelocity.magnitude > 0){
			transform.position += blastVelocity * Time.deltaTime;
		}
		if (attractionVelocity.magnitude > 0){
			transform.position += attractionVelocity * Time.deltaTime;
		}
	}


	public void SetBlastVelocity(Vector3 direction, float speed, float uncollectableTime){
		StartCoroutine(BecomeCollectable(uncollectableTime));
		StartCoroutine(DecayBlastVelocity(direction, speed));
		currentSpeedDecayTime = speedDecayTime;
	}


	public void SetAttractionVelocity(Vector3 velocity, float accelerationRatio){
		attracted = true;
		float cubedRatio = Mathf.Pow(accelerationRatio, 3f);
		Vector3 newVelocity = velocity * (1f - cubedRatio);
		if (newVelocity.magnitude > attractionVelocity.magnitude){
			attractionVelocity = newVelocity;
		}
		else{
			// if velocity is decreasing aka the player is getting farther, fade out the current velocity
			StopAttractionVelocity();
		}
	}


	private IEnumerator BecomeCollectable(float t){
		yield return new WaitForSeconds(t);
		isCollectable = true;
	}

	private IEnumerator DecayBlastVelocity(Vector3 direction, float speed){
		// add some noise to the direction
		float speedWithNoise = speed * (1f + Random.Range(-speedNoiseFactor, speedNoiseFactor));

		float counter = 0;
		float t = 0;
		while(counter <= speedDecayTime){
			blastVelocity = direction * speedWithNoise * (1f - t);
			// use an inversed cubed interpolation function to make orbs decelerate slowly
			counter += Time.deltaTime;
			t = counter/speedDecayTime;
			t = 1f - (1f - t) * (1f - t) * (1f - t);
			yield return null;
		}
		blastVelocity = Vector3.zero;
	}


	public void StopAttractionVelocity(){
		attracted = false;
		StartCoroutine(DecayAttractionVelocity(1f));
	}


	private IEnumerator DecayAttractionVelocity(float decayTime){
		float counter = 0;
		float mag = attractionVelocity.magnitude;
		while (counter <= decayTime && attracted == false){
			attractionVelocity = attractionVelocity.normalized * Mathf.SmoothStep(mag, 0, counter/decayTime);
			counter += Time.deltaTime;
			yield return null;
		}
	}


	public bool GetIsCollectable(){
		return isCollectable;
	}


	public Vector3 GetBlastVelocity(){
		return this.blastVelocity;
	}
}
