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

	protected PlayerAudioScript playerAudio;
	protected TempGameManager gameManager;
	protected TrailRenderer tail;

	// score tracking variables
	private int coinCombo;
	public bool canGetPoints;

	// specialAction timers
	protected bool canAction;
	protected bool actionTaken;
	public float actionTimeLimit;
	private float currentActionTime;

	public void Awake(){
		playerAudio = GetComponent<PlayerAudioScript>();
		initialPosition = transform.position;
		initialRotation = transform.rotation.eulerAngles;
		powerupEffect = transform.GetChild(0).GetComponent<PowerupEffectScript>();
		tail = GetComponent<TrailRenderer>();
	}

	public void Start () {
		cameraShaker = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShakeScript>();
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TempGameManager>();

		// puts player in its initial state
		ResetPlayerStates();
	}
	
	// Update is called once per frame
	public void Update () {
		if (!isDead){ 
			if (tag == "Player"){
				if (!isLanded){ 
					// if player is drifting, it should move based on gravity from nearby planets and rotate to face forwards
					GetComponent<Rigidbody2D>().velocity += CalculateGravityPull();

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
					GetComponent<Rigidbody2D>().velocity = Vector2.zero;

					// increase power level when landed. if powerup charge is active, charge at the super fast rate
					float chargeRate = currentPlanet.powerChargePerSecond;
					if (activatedPowerup == PowerupScript.PowerupType.charge){
						chargeRate = powerupChargeRate;
					}
				}
			}
		}
		else{ 
			// if player is dead, decrease the countdown timer
			respawnTimeCount -= Time.deltaTime;
			this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
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
	void OnTriggerEnter2D(Collider2D other) {
		if ((other.gameObject.tag == "Planet" || other.gameObject.tag == "HomePlanet") && isDead == false) {
			if (currentPlanet == null && isLanded == false){
				LandOnPlanet(other.gameObject);
			}
			else{
				// make sure that enough time has passed since the last planet transfer or that this is a new planet
				if (Time.time > lastPlanetTransferTime + 1f || lastPlanet != other.gameObject){ 
					lastPlanet = currentPlanet;
					currentPlanet = other.GetComponent<PlanetScript>();
					nearbyPlanets.Remove (currentPlanet.gameObject);
					// set the player position and rotation accordingly
					StandOnCurrentPlanet();
					RotateToCurrentPlanet();
					GetComponent<Rigidbody2D>().freezeRotation = true;
					transform.parent = currentPlanet.transform;

					lastPlanetTransferTime = Time.time;
				}
			}
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
		coinCombo = 0;
		transform.position = initialPosition;
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
			coinCombo = 0;
			currentPlanet = planet.GetComponent<PlanetScript>();
			playerAudio.PlayLandingSound();

			// rotates and repositions the player accordingly
			StandOnCurrentPlanet();
			RotateToCurrentPlanet();

			GetComponent<Rigidbody2D>().freezeRotation = true;
			transform.parent = planet.transform;
			nearbyPlanets.Remove (currentPlanet.gameObject);
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

			Vector2 leavingAngle = new Vector2 (transform.position.x - currentPlanet.transform.position.x,
			                            transform.position.y - currentPlanet.transform.position.y).normalized;
            // TODO: check if applyForce is better
            float leavingSpeed = maxChargeSpeed * 2f / 3f;
            if (chargeTime >= maxChargeTime){
            	leavingSpeed = maxChargeSpeed;
            }
            if (activatedPowerup == PowerupScript.PowerupType.lighting){
            	leavingSpeed *= powerupSpeed;
            }
			GetComponent<Rigidbody2D>().velocity = leavingAngle * leavingSpeed;
			GetComponent<Rigidbody2D>().freezeRotation = false;

			lastPlanet = currentPlanet;

			// crack/destroy the planet if it's not the home planet
			if (currentPlanet.tag != "HomePlanet"){
				// if the planet will explode, get points
				if (currentPlanet.WillExplodeNext()){
					gameManager.IncrementScore(currentPlanet.GetPointValue(), this.transform.position);
					gameManager.IncrementPlanetDestroyedCount(1);
				}

				//nearbyPlanets.Remove (currentPlanet.gameObject);
				currentPlanet.SelfDestruct();
				currentPlanet = null;
			}
			else { // otherwise just take off
				currentPlanet = null;
			}
		}
	}


	// helper function that rotates the player to "stand on" the current planet
	private void RotateToCurrentPlanet(){
		float angle;
		Vector3 difference = currentPlanet.transform.position - transform.position;

		if (difference.x == 0) {
			if (difference.y > 0)
				angle = 0;
			else{
				angle = 180;
			}
		}
		else{
			angle =  Mathf.Atan2 (difference.y, difference.x) * Mathf.Rad2Deg + 90;
		}

		transform.eulerAngles = new Vector3 (0, 0, angle);
	}


	// helper function that repositions the player to "stand on" the current planet
	private void StandOnCurrentPlanet(){
		Vector3 difference = transform.position - currentPlanet.transform.position;
		float radius = currentPlanet.GetComponent<CircleCollider2D>().radius * currentPlanet.transform.lossyScale.x + 
						this.GetComponent<BoxCollider2D>().size.y * this.transform.lossyScale.x / 2;
		Vector3 newDifference = difference.normalized * radius;
		transform.position = transform.position - difference + newDifference;
	}

	// helper function that rotates the player to face its moving direction
	private void RotateToVelocity(){
		float angle;
		Vector2 direction = GetComponent<Rigidbody2D>().velocity;

		if (direction.x == 0) {
			if (direction.y > 0)
				angle = 0;
			else{
				angle = 180;
			}
		}
		else{
			angle =  Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg - 90;
		}

		transform.eulerAngles = new Vector3 (0, 0, angle);
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

	// function that the Orb object calls when it's been picked up by the player
	// TODO rework this
	public void AcquiredOrb(int orbValue, Vector3 orbPosition){
		coinCombo += 1;
		int gainedPts = orbValue *coinCombo;
		//gameManager.IncrementScore(gainedPts);
		//playerHUD.ShowFloatingText(gainedPts, orbPosition);
		//playerAudio.PlayOrbSound(coinCombo);
	}


	// changes the length of the trail 
	private IEnumerator EnableTail(bool b, float t){
		if (b){
			tail.time = 1f;
		}
		else{
			float counter = 0f;
			while (isLanded && counter < t){
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

	public float GetPowerLevel(){
		return powerLevel;
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

}
