using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerScript : MonoBehaviour {
	// basic info and states
	protected bool isLanded;
	protected bool canJump;
	protected bool canControl;
	private Vector3 initialPosition;
	private Vector3 initialRotation;
	private PlanetScript currentPlanet, lastPlanet;
	private Vector2 cumulatedGravityPullForce;
	private float lastPlanetTransferTime = 0f;
	private List<GameObject> nearbyPlanets;

	// respawn variables
	private bool isDead;
	public float respawnTime;
	private float respawnTimeCount = 0;

	// charged jump stuff
	public float maxChargeTime;
	public float maxChargeSpeed;
	public float maxSpeed;
	protected float currentChargeTime;
	private GameObject chargeBar;

	// power up variables
	private float powerupCountdown;
	private PowerupScript.PowerupType activatedPowerup; // TODO:make this private later
	private int powerupChargeRate = 75; //TODO: make this private later
	private float powerupMass = 5; //TODO: make this private later
	private float powerupSpeed = 3; //TODO: make this private later
	private PowerupEffectScript powerupEffect;

	// for adding juice to the game
	private CameraShakeScript cameraShaker;
	public float slowMotionKillSpeed = 0.03f; 
	public float slowMotionKillDuration = 0.01f;

	// tracking components
	protected Rigidbody2D myRigidBody;
	protected PlayerAudioScript playerAudio;
	protected GameManager gameManager;
	protected TrailRenderer tail;

	// specialAction variables
	protected bool canAction;
	protected bool takingAction;
	public float actionChargeTime;
	protected float currentActionChargeTime;
	public int actionChargesMax;
	protected int actionChargesUsable;
	protected JetpackBar jetpackBar;
	[SerializeField]private float tractorBeamSpeed;
	[SerializeField]private float jetpackSpeed = 10f, jetpackDuration = 0.3f;
	protected float currentJetpackDuration = 0f;
	protected float leavingSpeed = 0f;

	private GameObject jetpackFlame;
	private LineRenderer tractorBeam;

	// planet pushing
	public float planetPushPower;

	// changing planet speed
	public float homePlanetRotationSpeed;

	// coin attraction 
	public float coinAttractionRadius;
	public float coinAttractionSpeed;

	// homePlanet attract force to prevent being stuck in orbit
	private float antiStuckForceTimer = 6f;
	private float currentAntiStuckForceTime;

	protected void Awake(){
		playerAudio = GetComponent<PlayerAudioScript>();
		initialPosition = transform.position;
		initialRotation = transform.rotation.eulerAngles;
		myRigidBody = GetComponent<Rigidbody2D>();
		powerupEffect = transform.GetChild(0).GetComponent<PowerupEffectScript>();
		tail = GetComponent<TrailRenderer>();
		transform.Find("Orb Attractor").GetComponent<CircleCollider2D>().radius = coinAttractionRadius / transform.lossyScale.x;
		nearbyPlanets = new List<GameObject>();
		chargeBar = transform.Find("Charge Bar").gameObject;
		jetpackBar = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<JetpackBar>();
		tractorBeam = GetComponent<LineRenderer>();
		jetpackFlame = transform.Find("Jetpack Flame").gameObject;
	}


	protected void Start () {
		cameraShaker = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShakeScript>();
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

		// puts player in its initial state if it has no velocity aka it just respawned
		if (myRigidBody.velocity == Vector2.zero){
			ResetPlayerStates();
		}
	}
	
	// Update is called once per frame
	protected void Update () {
		if (!isDead){ 
			if (tag == "Player"){
				if (!isLanded){ 
					// if player is drifting, it should move based on gravity from nearby planets and rotate to face forwards
					Vector2 gravityPullForce = CalculateGravityPull();
					if (gravityPullForce.magnitude > 0){
						cumulatedGravityPullForce += gravityPullForce;
					}
					else{
						cumulatedGravityPullForce = Vector2.zero;
					}
					myRigidBody.velocity += gravityPullForce;

					// limit player speed
					if (myRigidBody.velocity.magnitude > maxSpeed){
						myRigidBody.velocity = myRigidBody.velocity.normalized * maxSpeed;
						//Debug.Log("Playerspeed limited");
					}


					// increment antiStuckForce timer while drifting. if timer exceeds certain amount, apply force towards home
					currentAntiStuckForceTime += Time.deltaTime;
					if (currentAntiStuckForceTime > antiStuckForceTimer){
						myRigidBody.velocity += (Vector2)transform.position.normalized * -3f * Time.deltaTime;
						//Debug.Log("applying anti stuck force");
					}

					// check if player can do its special action
					if (!takingAction && actionChargesUsable > 0){
						canAction = true;
					}

					DetectActionButtonPress();
				}
				else{ 
					// if player has landed, then it has no velocity and it should rotate to stand on the planet
					myRigidBody.velocity = Vector2.zero;
					StandOnCurrentPlanet();
					RotateToCurrentPlanet();

					DetectJumpButtonPress();
				}

				// if we're not at max charge for actions, charge the action bar
				if (actionChargesUsable < actionChargesMax){
					currentActionChargeTime += Time.deltaTime;
					jetpackBar.UpdateChargeBar(currentActionChargeTime);
					if (currentActionChargeTime > actionChargeTime){
						//jetpackBar.PlayChargedAnimation(actionChargesUsable);
						playerAudio.PlayChargedSound();
						actionChargesUsable += 1;
						currentActionChargeTime = 0f;
					}
				}
			}
		}
		else{ 
			// if player is dead, decrease the countdown timer
			respawnTimeCount -= Time.deltaTime;
			myRigidBody.velocity = Vector2.zero;
			transform.position = initialPosition;

			if (respawnTimeCount < 0){
				Respawn();
			}
		}

		// remove null objects from nearbyPlanets
		for(int i = nearbyPlanets.Count - 1; i >= 0; i --){
			if (nearbyPlanets[i] == null){
				nearbyPlanets.RemoveAt(i);
			}
		}

	}

	// when colliding with a planet, land on it if drifting, or switch planets if already on a planet
	void OnCollisionEnter2D(Collision2D other) {
		if ((other.gameObject.tag == "Planet" || other.gameObject.tag == "HomePlanet") && isDead == false) {
			if (currentPlanet == null && isLanded == false){
				LandOnPlanet(other.gameObject);
			}
			else{
				// make sure that enough time has passed since the last planet transfer or that this is a new planet
				if (Time.time > lastPlanetTransferTime + 1f || lastPlanet != other.gameObject){ 
					lastPlanet = currentPlanet;
					currentPlanet = other.gameObject.GetComponent<PlanetScript>();
					nearbyPlanets.Remove (currentPlanet.gameObject);
					// set the player position and rotation accordingly
					StandOnCurrentPlanet();
					RotateToCurrentPlanet();
					myRigidBody.freezeRotation = true;
					transform.parent = currentPlanet.transform;

					lastPlanetTransferTime = Time.time;
				}
			}
		}
	}


	// when colliding with a coin, collect it and destroy it
	void OnTriggerEnter2D(Collider2D other){
		if(other.gameObject.tag == "CoinOrb" && other.gameObject.GetComponent<OrbScript>().GetIsCollectable()){
			gameManager.CollectCoin(other.gameObject.GetComponent<OrbScript>().pointValue);
			playerAudio.PlayOrbSound(0);
			Destroy(other.gameObject);
		}
	}


	// helper function that resets the player to initial drifting state
	private void ResetPlayerStates(){
		isDead = false;
		isLanded = false;
		canJump = false;
		currentChargeTime = 0f;	
		canAction = false;
		takingAction = false;
		currentActionChargeTime = 0f;
		actionChargesUsable = 0;
		transform.parent = null;
		currentPlanet = null;
		lastPlanet = null;
		currentAntiStuckForceTime = 0f;
		leavingSpeed = 0f;

		chargeBar.SetActive(false);
		jetpackFlame.SetActive(false);
		activatedPowerup = PowerupScript.PowerupType.none;
		powerupCountdown = 0;
		powerupEffect.DeactivateAllEffects();
		transform.position = initialPosition; // TODO: need better method for respawn
		transform.eulerAngles = initialRotation;
		nearbyPlanets.Clear();
	}


	// disable the player's components and starts the respawn timer
	public void Suicide(){
		isDead = true;
		respawnTimeCount = respawnTime;
		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<BoxCollider2D>().enabled = false;

		StartCoroutine(ShowDeathEffect());
	}


	// Coroutine that does the effects for when a player dies
	IEnumerator ShowDeathEffect(){
		Time.timeScale = slowMotionKillSpeed;
		cameraShaker.ShakeCamera(2f, 0.5f);
		Handheld.Vibrate();
		yield return new WaitForSeconds(slowMotionKillDuration);
		Time.timeScale = 1f;
		playerAudio.PlayDeathSound();
	}


	// respawn the player by reactivating its components
	public void Respawn(){
		ResetPlayerStates();
		GetComponent<SpriteRenderer>().enabled = true;
		GetComponent<BoxCollider2D>().enabled = true;

		playerAudio.PlayRespawnSound();
	}


	// helper function that lands the player onto a given planet
	public void LandOnPlanet(GameObject planet){
		// make sure that the player hasn't already landed
		if (isLanded == false && currentPlanet == null){
			isLanded = true;
			canJump = true; // TODO: implement a brief delay when you land where you can't jump
			canAction = false;
			takingAction = false;
			currentAntiStuckForceTime = 0f;
			cumulatedGravityPullForce = Vector2.zero;
			StartCoroutine(EnableTail(false, 1f));

			// clear coin combo
			currentPlanet = planet.GetComponent<PlanetScript>();
			playerAudio.PlayLandingSound();

			// rotates and repositions the player accordingly
			StandOnCurrentPlanet();
			RotateToCurrentPlanet();

			myRigidBody.freezeRotation = true;
			transform.parent = planet.transform;
			nearbyPlanets.Remove (currentPlanet.gameObject);
			// TODO: clean this up
			if (currentPlanet.gameObject.tag == "HomePlanet" && currentPlanet.rotationSpeed != homePlanetRotationSpeed){
				currentPlanet.rotationSpeed = homePlanetRotationSpeed;
			}
		}
		else{
			Debug.Log("Error: cannot land on " + planet.name);
		}
	}

	// sends player off a planet and blows it up
	public void LeavePlanet(float chargeTime){
		// make sure there is a currentplanet and the player can jump off it
		if (isLanded && currentPlanet != null && canJump){
			isLanded = false;
			transform.parent = null;
			canJump = false;
			StartCoroutine(EnableTail(true, 0f));
			playerAudio.PlayLeavingSound();

			float chargePercentage = Mathf.Clamp(chargeTime / maxChargeTime, 0f, 1f);
			leavingSpeed = maxChargeSpeed * (2f + chargePercentage) / 3f;
			Vector2 leavingDirection = (Vector2)(transform.position - currentPlanet.transform.position).normalized;

            //if (activatedPowerup == PowerupScript.PowerupType.lighting){
            //	leavingSpeed *= powerupSpeed;
            //}
			myRigidBody.velocity = leavingDirection * leavingSpeed;
			myRigidBody.freezeRotation = false;

			lastPlanet = currentPlanet;

			// crack/destroy the planet if it's not the home planet
			if (currentPlanet.tag != "HomePlanet"){
				// if the planet will not explode, push it 
				if (!currentPlanet.WillExplodeNext()){
					float pushPower = planetPushPower * (1f + chargePercentage * 1f);
					currentPlanet.PushPlanet(-leavingDirection, pushPower);
				}
				currentPlanet.TakeDamage(leavingDirection, true);
			}
			currentPlanet = null;
		}
	}


	// helper function that rotates the player to "stand on" the current planet
	private void RotateToCurrentPlanet(){
		Vector3 difference = currentPlanet.transform.position - transform.position;
		transform.eulerAngles = new Vector3 (0, 0, GameManager.GetAngleFromVector(difference) + 90);
	}


	// helper function that repositions the player to "stand on" the current planet
	private void StandOnCurrentPlanet(){
		Vector3 difference = transform.position - currentPlanet.transform.position;
		float radius = currentPlanet.GetComponent<CircleCollider2D>().radius * currentPlanet.transform.lossyScale.x + 
						this.GetComponent<BoxCollider2D>().size.y * this.transform.lossyScale.x / 2;
		Vector3 newDifference = difference.normalized * radius;
		transform.position = transform.position - difference + newDifference;
	}


	private Vector2 CalculateGravityPull(){
		Vector2 final = Vector2.zero;
		float G = 6.67300f * 1f;  // should be 10 to the -11th power, but we're keeping planet mass low to compensate
		float m = 1; // player mass
		if (activatedPowerup == PowerupScript.PowerupType.lighting){
			m = powerupMass;
		}
		foreach(GameObject planetObj in nearbyPlanets) {
			if (planetObj != null && (lastPlanet == null || planetObj != lastPlanet.gameObject)){
				PlanetScript planet = planetObj.GetComponent<PlanetScript>();
				if (planet && planet.GetIsDestroyed() == false){
					Vector2 r = new Vector2(planet.transform.position.x - transform.position.x,
											planet.transform.position.y - transform.position.y);
					Vector2 direction = r.normalized;
					// raised to power of 2.5 instead of 2 to make gravitation attraction slightly weaker than normal
					float r_squared = Mathf.Pow(r.magnitude, 2.5f); 
					// F = G*M*m/r_squared
					float F = G * planet.mass * m / r_squared;
					final+= F * direction;
				}
			}
		}
		
		return final;
	}


	// changes the length of the trail 
	public IEnumerator EnableTail(bool b, float t){
		if (b){
			tail.time = 1f;
		}
		else{
			float counter = 0f;
			while (counter < t){
				tail.time = Mathf.MoveTowards(tail.time, 0f, t * Time.deltaTime);
				counter += Time.deltaTime;
				yield return null;
			}
		}
	}


	private void DetectJumpButtonPress(){
		// detect jump button press for spaceKey
		if (canControl && canJump){
			if (Input.GetKeyDown(KeyCode.Space) ||
				(Input.GetKey(KeyCode.Space) && currentChargeTime > 0f)){
				currentChargeTime += Time.deltaTime;
				if (currentChargeTime > 0.15f && currentChargeTime < maxChargeTime){
					playerAudio.PlayChargingSound();
					chargeBar.SetActive(true);
					chargeBar.transform.localScale = new Vector3(chargeBar.transform.localScale.x, 
															Mathf.Lerp(3f, 15f, currentChargeTime/maxChargeTime),
															chargeBar.transform.lossyScale.x);
				}
				else if(currentChargeTime >= maxChargeTime){
					playerAudio.PlayMaxedChargingSound();
				}
			}
			else if (Input.GetKeyUp(KeyCode.Space) && currentChargeTime > 0f){
				LeavePlanet(currentChargeTime);
				currentChargeTime = 0;
				chargeBar.SetActive(false);
				playerAudio.StopChargingSound();
			}
		}

		// detect jumpButton press with touch
		if (canControl && canJump && Input.touchCount > 0 && 
			GameManager.instance.IsTouchPositionValid(Input.touches[0].position))
		{
			Touch touch = Input.touches[0];

			if (touch.phase == TouchPhase.Began || 
				(touch.phase == TouchPhase.Stationary && currentChargeTime > 0f) ||
				(touch.phase == TouchPhase.Moved && currentChargeTime > 0f)){
				currentChargeTime += Time.deltaTime;
				if (currentChargeTime > 0.15f && currentChargeTime < maxChargeTime){
					playerAudio.PlayChargingSound();
					chargeBar.SetActive(true);
					chargeBar.transform.localScale = new Vector3(chargeBar.transform.localScale.x, 
															Mathf.Lerp(3f, 15f, currentChargeTime/maxChargeTime),
															chargeBar.transform.lossyScale.x);
				}
				else if(currentChargeTime >= maxChargeTime){
					playerAudio.PlayMaxedChargingSound();
				}
			}
			else if(Input.touches[0].phase == TouchPhase.Ended && currentChargeTime > 0f){
				LeavePlanet(currentChargeTime);
				currentChargeTime = 0;
				chargeBar.SetActive(false);
				playerAudio.StopChargingSound();
			}
		}
	}


	private void DetectActionButtonPress(){
		// detect action button press for spaceKey
		if (canControl && canAction && Input.GetKeyDown(KeyCode.Space)){
			StartCoroutine(ActivateJetpack());
		}

		// detect actionbutton press for touch
		if (canControl && canAction && Input.touchCount > 0 && 
			GameManager.instance.IsTouchPositionValid(Input.touches[0].position) &&
			Input.touches[0].phase == TouchPhase.Began)
		{
			StartCoroutine(ActivateJetpack());
		}
	}


	// function that the powerup object calls when it's been picked up by the player
	// TODO: how to get enum from another file?
	// TODO: investigate pssing anonymous functions
	public void AcquiredPowerup(PowerupScript.PowerupType type, int duration, AudioClip sound){
		powerupCountdown = duration;
		activatedPowerup = type;
		//TODOGetComponent<AudioSource>().PlayOneShot(sound);
		powerupEffect.DeactivateAllEffects();
		powerupEffect.ActivateEffect(type);
		if (type == PowerupScript.PowerupType.barrier){
			// barrier: extra life
		}
		else if (type == PowerupScript.PowerupType.charge){
			// charge: max powerlevel
		}
		else if (type == PowerupScript.PowerupType.lighting){
			// lightingbolt: max speed and mass
		}
		else if (type == PowerupScript.PowerupType.magnet){
			// coin magnet
		}
	}


	// allows player to fly back to home planet at a set speed until it lands a planet
	// TODO: what to do with this?
	private IEnumerator ActivateTractorBeam(){
		tractorBeam.enabled = true;
		StartCoroutine(EnableTail(false, 1f));

		Vector2 newVelocity = transform.position.normalized * tractorBeamSpeed * -1;
		playerAudio.PlayTractorBeamSound();

		while(!isLanded){
			myRigidBody.velocity = newVelocity;
			tractorBeam.SetPosition(1, transform.position);
			canJump = false;
			yield return null;

		}
		tractorBeam.enabled = false;
		playerAudio.StopTractorBeamSound();
	}


	// gives player a boost of speed in its current facing direction for a given duration without the effects of gravity
	private IEnumerator ActivateJetpack(){
		if (!isLanded && canAction && !takingAction){
			actionChargesUsable -= 1;
			jetpackBar.DepleteCurrentChargeBar();
			canAction = false;
			takingAction = true;

			// calculate the direction that the player is facing and the speed in that direction in the current velocity
			Vector2 facingDirection = GameManager.GetVectorFromAngle(transform.eulerAngles.z);
			float facingDirectionSpeed = GameManager.GetVectorMagnitudeFromAnotherVector(myRigidBody.velocity, facingDirection);

			float newSpeed = jetpackSpeed;
			// if facing direction speed is larger than or equal to launch speed, then we're just boosting for speed.
			// apply boost based on launch speed
			if (facingDirectionSpeed >= leavingSpeed){
				newSpeed = leavingSpeed + jetpackSpeed * 2f / 3f;
				print("---boosting on top of launch speed.");
			}
			// if facing direction speed is larger than 0 but under the leaving speed, then we're being pulled somewhere but not too much yet.
			// boost based on current speed
			else if(facingDirectionSpeed > 0f && facingDirectionSpeed < leavingSpeed){
				newSpeed = Mathf.Max(facingDirectionSpeed + jetpackSpeed * 2f / 3f, jetpackSpeed);
				print("---boosting on top of current speed");
			}
			else{
				print("---boosting on jetpack speed");
			}
			print("facing direction speed is " + facingDirectionSpeed + ", leaving speed is " + leavingSpeed + ", final speed is " + newSpeed);

			currentJetpackDuration = 0f;
			jetpackFlame.SetActive(true);
			playerAudio.PlayJetpackSound();

			// tiny delay before speedboost
			float t = 0f;
			while(t < 0.05f){
				t += Time.deltaTime;
				myRigidBody.velocity = Vector2.zero;
				yield return null;
			}

			cameraShaker.ShakeCamera(2f, 0.15f);
			while(currentJetpackDuration < jetpackDuration && !isLanded){
				myRigidBody.velocity = facingDirection * newSpeed;
				currentJetpackDuration += Time.deltaTime;
				//float speedRatio = Mathf.Pow(currentJetpackDuration / jetpackDuration, 3f);

				yield return null;
			}

			playerAudio.StopJetpackSound();
			jetpackFlame.SetActive(false);
			takingAction = false;
			currentJetpackDuration = 0f;
			leavingSpeed = newSpeed;
		}
	}


	// if the player warped during a jetpack session, resume its visual effects but not its effects on velocity
	// the sound isn't duplicated though because we're assuming the old player will finish playing it
	private IEnumerator ResumeJetpack(){
		// calculate jetpack direction based on where player is facing
		//float angle = transform.eulerAngles.z - 90f;
		//Vector2 direction = new Vector2(-Mathf.Cos(angle * Mathf.Deg2Rad), -Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

		Vector2 currentJetpackSpeed = myRigidBody.velocity;
		jetpackFlame.SetActive(true);

		while(currentJetpackDuration < jetpackDuration && !isLanded){
			// calculate and apply speed based on time
			myRigidBody.velocity = currentJetpackSpeed;
			currentJetpackDuration += Time.deltaTime;
			yield return null;
		}

		jetpackFlame.SetActive(false);
		takingAction = false;
		currentJetpackDuration = 0f;
		leavingSpeed = currentJetpackSpeed.magnitude;
	}

	// all the getters and setters
	public bool GetIsLanded(){
		return isLanded;
	}

	public bool GetIsDead(){
		return isDead;
	}

	public bool GetCanJump(){
		return canJump;
	}

	public bool GetCanAction(){
		return canAction;
	}

	public bool GetTakingAction(){
		return takingAction;
	}

	public float GetcurrentActionChargeTime(){
		return currentActionChargeTime;
	}

	public GameObject GetLastPlanet(){
		if (lastPlanet == null){
			return null;
		}
		else{
			return lastPlanet.gameObject;
		}
	}

	public PlayerAudioScript GetPlayerAudio(){
		return playerAudio;
	}

	public void AddNearByPlanet(GameObject planet){
		if ((lastPlanet == null || planet != lastPlanet.gameObject) &&
			(currentPlanet == null || planet != currentPlanet.gameObject)){
			nearbyPlanets.Add(planet);
			// reset antistuck force timer
			currentAntiStuckForceTime = 0f;
		}
	}

	public void RemoveNearByPlanet(GameObject planet){
		nearbyPlanets.Remove(planet);
	}

	// returns true if the planet object can be added to the nearbyPlanet list
	public bool HasNearByPlanet(GameObject planet){
		return nearbyPlanets.Contains(planet);
	}

	public void ClearLastPlanet(){
		lastPlanet = null;
	}


	public void GetDataFromOldPlayer(PlayerScript oldPlayer){
		isLanded = oldPlayer.isLanded;
		canJump = oldPlayer.canJump;
		canControl = oldPlayer.canControl;
		canAction = oldPlayer.canAction;
		currentActionChargeTime = oldPlayer.currentActionChargeTime;
		actionChargesUsable = oldPlayer.actionChargesUsable;
		myRigidBody.velocity = oldPlayer.GetComponent<Rigidbody2D>().velocity;

		// if the old player was using the jetpack when it warped, the new player should continue warping
		takingAction = oldPlayer.takingAction;
		currentJetpackDuration = oldPlayer.currentJetpackDuration;
		leavingSpeed = oldPlayer.leavingSpeed;
		if (takingAction && currentJetpackDuration > 0f){
			StartCoroutine(ResumeJetpack());
		}
	}


	public void SetCanControl(bool b){
		canControl = b;
	}

}
