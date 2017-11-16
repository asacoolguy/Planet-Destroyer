using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// PlayerScript is the base class for players. It defines basic properties of players such as jumping and landing.
/// different charaters should extend this class to implement their unique abilities and properties. 
/// </summary>

public abstract class PlayerScript : MonoBehaviour {
	// basic info and states
	protected bool isLanded;
	protected bool canJump;
	private Vector3 initialPosition;
	private Vector3 initialRotation;
	private PlanetScript currentPlanet, lastPlanet;
	private float lastPlanetTransferTime = 0f;
	private ArrayList nearbyPlanets = new ArrayList();
	// respawn variables
	private bool isDead;
	public float respawnTime;
	private float respawnTimeCount = 0;
	// scores and bars
	public float powerLevelInitial;
	private float powerLevel;
	public float powerLevelDecayPerSecond;
	public float powerLevelUsedOnJump;
	// new powerlevel stuff
	public float maxChargeTime;
	public float maxChargeSpeed;
	public float maxSpeed;

	// power up variables
	private float powerupCountdown;
	public PowerupScript.PowerupType activatedPowerup; // TODO:make this private later
	public int powerupChargeRate = 75; //TODO: make this private later
	public float powerupMass = 5; //TODO: make this private later
	public float powerupSpeed = 3; //TODO: make this private later
	private PowerupEffectScript powerupEffect;

	// for adding juice to the game
	private CameraShakeScript cameraShaker;
	public float slowMotionKillSpeed = 0.03f; 
	public float slowMotionKillDuration = 0.01f;

	// tracking components
	private Rigidbody2D myRigidBody;
	protected PlayerAudioScript playerAudio;
	protected GameManager gameManager;
	protected TrailRenderer tail;

	// score tracking variables
	public bool canGetPoints;

	// specialAction timers
	protected bool canAction;
	protected bool actionTaken;
	public float actionTimeLimit;
	private float currentActionTime;

	// planet pushing
	public float planetPushSpeed;

	public float homePlanetRotationSpeed;

	// coin attraction 
	public float coinAttractionRadius;
	public float coinAttractionSpeed;


	protected void Awake(){
		playerAudio = GetComponent<PlayerAudioScript>();
		initialPosition = transform.position;
		initialRotation = transform.rotation.eulerAngles;
		myRigidBody = GetComponent<Rigidbody2D>();
		powerupEffect = transform.GetChild(0).GetComponent<PowerupEffectScript>();
		tail = GetComponent<TrailRenderer>();
		transform.Find("Orb Attractor").GetComponent<CircleCollider2D>().radius = coinAttractionRadius / transform.lossyScale.x;
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
					myRigidBody.velocity += CalculateGravityPull();
					// limit player speed
					if (myRigidBody.velocity.magnitude > maxSpeed){
						myRigidBody.velocity = myRigidBody.velocity.normalized * maxSpeed;
					}

					// increment actionTimer and make it available if we pass the limit
					if (!canAction && !actionTaken){
						currentActionTime += Time.deltaTime;
						if (currentActionTime > actionTimeLimit){
							canAction = true;
							playerAudio.PlayChargedSound();
						}
					}
				}
				else{ 
					// if player has landed, then it has no velocity and it should rotate to stand on the planet
					myRigidBody.velocity = Vector2.zero;
					StandOnCurrentPlanet();
					RotateToCurrentPlanet();
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
		canAction = false;
		actionTaken = false;
		currentActionTime = 0f;
		transform.parent = null;
		currentPlanet = null;
		lastPlanet = null;
		activatedPowerup = PowerupScript.PowerupType.none;
		powerupCountdown = 0;
		powerupEffect.DeactivateAllEffects();
		transform.position = initialPosition; // TODO: need better method for respawn
		transform.eulerAngles = initialRotation;
		//powerLevel = powerLevelInitial;
		nearbyPlanets.Clear();
	}

	// helper function that lands the player onto a given planet
	public void LandOnPlanet(GameObject planet){
		// make sure that the player hasn't already landed
		if (isLanded == false && currentPlanet == null){
			isLanded = true;
			canJump = true; // TODO: implement a brief delay when you land where you can't jump
			currentActionTime = 0f;
			canAction = false;
			actionTaken = false;
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
			print("Error: cannot land on " + planet.name);
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

			Vector2 leavingDirection = (Vector2)(transform.position - currentPlanet.transform.position).normalized;
            // TODO: check if applyForce is better
            float leavingSpeed = maxChargeSpeed * 2f / 3f;
            if (chargeTime >= maxChargeTime){
            	leavingSpeed = maxChargeSpeed;
            }
            if (activatedPowerup == PowerupScript.PowerupType.lighting){
            	leavingSpeed *= powerupSpeed;
            }
			myRigidBody.velocity = leavingDirection * leavingSpeed;
			myRigidBody.freezeRotation = false;

			lastPlanet = currentPlanet;

			// crack/destroy the planet if it's not the home planet
			if (currentPlanet.tag != "HomePlanet"){
				// if the planet will not explode, push it 
				if (!currentPlanet.WillExplodeNext()){
					float pushSpeed = planetPushSpeed;
					if (chargeTime >= maxChargeTime){
						pushSpeed = planetPushSpeed * 3;
					}
					currentPlanet.PushPlanet(-leavingDirection, pushSpeed);
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


	// disable the player's components and starts the respawn timer
	public void Suicide(){
		isDead = true;
		powerLevel = 0f;
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

	// called by other players instead of suicide when this player 
	public void BarrierPop(Vector2 velocity){

	}

	// respawn the player by reactivating its components
	public void Respawn(){
		ResetPlayerStates();
		GetComponent<SpriteRenderer>().enabled = true;
		GetComponent<BoxCollider2D>().enabled = true;

		playerAudio.PlayRespawnSound();
	}

	private Vector2 CalculateGravityPull(){
		Vector2 final = Vector2.zero;
		float G = 6.67300f * 1f;  // should be 10 to the -11th power, but we're keeping planet mass low to compensate
		float m = 1; // player mass
		if (activatedPowerup == PowerupScript.PowerupType.lighting){
			m = powerupMass;
		}
		foreach(GameObject p in nearbyPlanets) {
			if (p != null && (lastPlanet == null || p != lastPlanet.gameObject)){
				PlanetScript planet = p.GetComponent<PlanetScript>();
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

	// does the player's action. to be implement by each different player class
	public abstract void ActivatePlayerAction();


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

	public bool GetActionTaken(){
		return actionTaken;
	}

	public float GetCurrentActionTime(){
		return currentActionTime;
	}

	public float GetPowerLevel(){
		return powerLevel;
	}

	public GameObject GetLastPlanet(){
		if (lastPlanet == null){
			return null;
		}
		else{
			return lastPlanet.gameObject;
		}
	}

	public Vector3 GetInitialPosition(){
		return initialPosition;
	}

	public Vector3 GetInitialRotation(){
		return initialRotation;
	}

	public void SetCanGetPoints(bool b){
		this.canGetPoints = b;
	}

	public PlayerAudioScript GetPlayerAudio(){
		return playerAudio;
	}

	public void AddNearByPlanet(GameObject planet){
		if (lastPlanet == null || planet != lastPlanet.gameObject){
			nearbyPlanets.Add(planet);
		}
	}

	public void RemoveNearByPlanet(GameObject planet){
		nearbyPlanets.Remove(planet);
	}

	public bool HasNearByPlanet(GameObject planet){
		return nearbyPlanets.Contains(planet);
	}

	public void ClearLastPlanet(){
		lastPlanet = null;
	}


	public void GetDataFromOldPlayer(PlayerScript oldPlayer){
		isLanded = oldPlayer.GetIsLanded();
		canJump = oldPlayer.GetCanJump();
		canAction = oldPlayer.GetCanAction();
		actionTaken = oldPlayer.GetActionTaken();
		currentActionTime = oldPlayer.GetCurrentActionTime();

		myRigidBody.velocity = oldPlayer.GetComponent<Rigidbody2D>().velocity;
		// powerup stuff
		/*
		activatedPowerup = PowerupScript.PowerupType.none;
		powerupCountdown = 0;
		powerupEffect.DeactivateAllEffects();
		coinCombo = 0;
		transform.position = initialPosition;
		transform.eulerAngles = initialRotation;
		//powerLevel = powerLevelInitial;
		nearbyPlanets.Clear();
		*/
	}

}
