using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanetScript : MonoBehaviour { 
	[SerializeField]private int pointValue;
	public float mass; // mass of the planet, affects its gravitation pull
	public float weight; // weight of the planet, affects how far it will get pushed
	public float rotationSpeed; // number of degrees the planet rotates every second
	public float moveSpeed; // speed at which the planet moves towards the middle
	private Vector2 moveDirection;
	private bool isDestroyed;

	// planet cracking variables
	public int maxCracks; // 2 cracks for big planets, 0 cracks for asteroids
	private int crackedState; // 0, 1 or 2 cracks. 3 means destroyed.

	// planet pushing variables
	private Vector2 pushForce;
	public float planetPushDecayTime;
	private bool collidedAfterPush;
	private GameObject lastInteractedObj; // used to make sure collision detection doesn't happen twice

	// coin emitting variables
	public int coinsToRelease;
	[Range (0f, 1f)]
	public float bigOrbReleaseChance;
	public GameObject coinOrb, coinOrbBig;

	// keep track of other game objects
	private PlanetGravityScript gravityField;
	public Sprite regular, cracked1, cracked2, explosion;
	public AudioClip explodeSound, crackSound1, crackSound2;
	private CameraShakeScript cameraShaker;

	void Start () {
		isDestroyed = false;
		crackedState = 0;
		gravityField = transform.GetChild(0).GetComponent<PlanetGravityScript>();
		cameraShaker = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShakeScript>();

		pushForce = Vector2.zero;
		collidedAfterPush = false;
	}

	void Update () {
		// rotate the planet every frame
		transform.Rotate (0, 0, Time.deltaTime * rotationSpeed);

		// move the planet towards the middle
		if (tag == "Planet"){
			if(isDestroyed == false){
				moveDirection = (Vector2) transform.position.normalized * -1f;
				GetComponent<Rigidbody2D>().velocity = moveDirection * moveSpeed + pushForce;
			}
			else{
				GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			}
		}
		else if(tag == "HomePlanet"){
			transform.position = Vector3.zero;
		}
	}


	void OnCollisionEnter2D(Collision2D other) {
		// if this is homeplanet and it collided with a normal planet, game over
		if (this.tag == "HomePlanet" && other.gameObject.tag == "Planet") {
			GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().StopGameRunning();
		}
		// if this is a planet-planet collision, the current planet is being moved by pushForce and the two planet havent just interacted with each other
		else if(this.tag == "Planet" && other.gameObject.tag == "Planet" && pushForce != Vector2.zero && other.gameObject != lastInteractedObj){
			PlanetScript otherPlanet = other.gameObject.GetComponent<PlanetScript>();
			SetLastInteractedObj(otherPlanet.gameObject);
			otherPlanet.SetLastInteractedObj(this.gameObject);

			Vector3 damageDirection = (other.transform.position - this.transform.position).normalized;
			this.TakeDamage(damageDirection, false);
			collidedAfterPush = true;
			otherPlanet.TakeDamage(-damageDirection, false);
		}
	}

	// checks all players to see if they overlap with this planet
	public bool CanSpawn(){
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject obj in players) {
			if (obj.GetComponent<PlayerScript>().GetIsDead() == false){
				float xDist = obj.transform.position.x - this.transform.position.x;
				float yDist = obj.transform.position.y - this.transform.position.y;
				float safeDistance = this.gameObject.GetComponent<CircleCollider2D>().radius + obj.GetComponent<BoxCollider2D>().size.y;
				if (Mathf.Sqrt(xDist * xDist + yDist * yDist) < safeDistance){
					return false;
				}
			}
		}

		return true;
	}

	private IEnumerator ChangeSprite(){
		switch(crackedState){
			case 0:
				GetComponent<SpriteRenderer>().sprite = regular;
				break;
			case 1: 
				GetComponent<SpriteRenderer>().sprite = cracked1;
				break;
			case 2: 
				GetComponent<SpriteRenderer>().sprite = cracked2;
				break;
			case -1:
				GetComponent<SpriteRenderer>().sprite = explosion;
				yield return new WaitForSeconds(0.4f);
				GetComponent<SpriteRenderer>().sprite = null;
				break;
		}
	}

	// called when another player blows this planet up. damageFromTakeOff is true when the damage comes from player taking off
	public void TakeDamage(Vector3 damageDirection, bool damageFromTakeOff){
		// increments the cracked state and blow it up if it exceeds the max number of allowed cracks. 
		// also do camera shake here? 
		crackedState += 1;
		cameraShaker.ShakeCamera(0.75f * crackedState, 0.2f * crackedState);
		if (crackedState > maxCracks){
			crackedState = -1;
			StartCoroutine(SelfDestructHelper());
			GameManager.instance.IncrementScore(pointValue, this.transform.position);

			if (damageFromTakeOff){
				ReleaseCoinsToFollowPlayer(damageDirection);
			}
			else{
				ReleaseCoins(-damageDirection);
			}
		}
		else{
			StartCoroutine(ChangeSprite());
			if(crackedState == 1){
				GetComponent<AudioSource>().PlayOneShot(crackSound1);
			}
			else{
				GetComponent<AudioSource>().PlayOneShot(crackSound2);
			}

		}

	}


	IEnumerator SelfDestructHelper(){
		// TODO: change this to a better animation method
		isDestroyed = true;
		gravityField.enabled = false;
		GetComponent<CircleCollider2D>().enabled = false;
		StartCoroutine(ChangeSprite());
		gameObject.GetComponent<AudioSource> ().PlayOneShot (explodeSound);

		yield return new WaitForSeconds(0.2f);

		// moves player out if this planet is a parent of the player
		for(int i = 0; i < transform.childCount; i++){
			if (transform.GetChild(i).tag == "Player"){
				transform.GetChild(i).parent = null;
			}
		}

		while (gameObject.GetComponent<AudioSource> ().isPlaying){
			yield return null;
		}

		GameManager.instance.ChangePlanetsOnScreenCount(-1);
		Destroy(this.gameObject);
	}


	// pushes the planet in the given direction and speed 
	public void PushPlanet(Vector2 direction, float speed){
		StartCoroutine(PushPlanetHelper(direction, speed));
	}

	private IEnumerator PushPlanetHelper(Vector2 direction, float speed){
		float counter = 0;
		float t = 0;
		collidedAfterPush = false;
		while(!isDestroyed && !collidedAfterPush && counter <= planetPushDecayTime){
			pushForce = direction * Mathf.SmoothStep(speed, 0, t) / weight;
			counter += Time.deltaTime;
			t = counter/planetPushDecayTime;
			yield return null;
		}
		pushForce = Vector2.zero;
	}


	// helper function called on destruction, sends out a set number of coins around a certain direction
	private void ReleaseCoins(Vector3 releaseDirection){
		if (coinsToRelease == 0){
			return;
		}

		// randomize the release angle for each coin and instantiate them with the right velocity
		float startingAngle = GameManager.GetAngleFromVector(releaseDirection) - 90f;
		float anglePerSection = 180f / coinsToRelease;
		for (int i = 0; i < coinsToRelease; i++){
			float releaseAngle = startingAngle + anglePerSection * i + Random.Range(-anglePerSection / 3f, anglePerSection / 3f);

			Vector2 adjustedReleaseDirection = new Vector2(Mathf.Cos(releaseAngle * Mathf.Deg2Rad), Mathf.Sin(releaseAngle * Mathf.Deg2Rad));
			float releaseDistance = GetComponent<CircleCollider2D>().radius * transform.lossyScale.x + 
									coinOrb.GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
			Vector3 releasePosition = this.transform.position + (Vector3) adjustedReleaseDirection * releaseDistance;

			GameObject orbObj;
			if (Random.Range(0f, 1f) < bigOrbReleaseChance * 2f){
				orbObj = Instantiate(coinOrbBig, releasePosition, Quaternion.identity);
			}
			else{
				orbObj = Instantiate(coinOrb, releasePosition, Quaternion.identity);
			}
			 
			orbObj.GetComponent<OrbScript>().SetBlastVelocity(adjustedReleaseDirection, 20f, 0.5f);
		}
	}


	// helper function called on destruction, sends out a set number of coins around a certain direction
	private void ReleaseCoinsToFollowPlayer(Vector3 playerDirection){
		if (coinsToRelease == 0){
			return;
		}

		// randomize the release angle for each coin and instantiate them with the right velocity
		float startingAngle = GameManager.GetAngleFromVector(playerDirection);
		float anglePerSection = 30f;
		int bigOrbsAllowed = coinsToRelease / 2;
		for (int i = 0; i < coinsToRelease; i++){
			float releaseAngle = startingAngle + Mathf.Pow(-1, i) * (30 + Random.Range(0f, anglePerSection));

			Vector2 adjustedReleaseDirection = new Vector2(Mathf.Cos(releaseAngle * Mathf.Deg2Rad), Mathf.Sin(releaseAngle * Mathf.Deg2Rad));
			float releaseDistance = GetComponent<CircleCollider2D>().radius * transform.lossyScale.x + 
									coinOrb.GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
			Vector3 releasePosition = this.transform.position + (Vector3) adjustedReleaseDirection * releaseDistance;

			GameObject orbObj;
			if (bigOrbsAllowed > 0 && Random.Range(0f, 1f) < bigOrbReleaseChance){
				orbObj = Instantiate(coinOrbBig, releasePosition, Quaternion.identity);
				bigOrbsAllowed -= 1;
			}
			else{
				orbObj = Instantiate(coinOrb, releasePosition, Quaternion.identity);
			}
			orbObj.GetComponent<OrbScript>().SetBlastVelocity(adjustedReleaseDirection, 25f, 0.2f);
		}
	}


	public bool WillExplodeNext(){
		return crackedState == maxCracks;
	}

	public bool GetIsDestroyed(){
		return isDestroyed;
	}

	public int GetPointValue(){
		return pointValue;
	}

	public Vector2 GetPushForce(){
		return pushForce;
	}

	public void SetLastInteractedObj(GameObject obj){
		lastInteractedObj = obj;
		StartCoroutine(ResetLastInteractedObj());
	}

	private IEnumerator ResetLastInteractedObj(){
		yield return new WaitForSeconds(0.5f);
		lastInteractedObj = null;
	}
}
