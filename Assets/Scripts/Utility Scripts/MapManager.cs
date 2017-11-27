using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains info about the current map.
/// Handles enemy spawning and player warping
/// </summary>
public class MapManager : MonoBehaviour {
	public GameObject asteroid, planet_big, planetMedium;

	public int warpAngle = 20;
	private float lastSpawnAngle = 0f;

	private Vector2 boundaries;
	public float boundaryOffset;
	private PlayerScript player;
	private PlayerHUDScript playerHUD;
	private JetpackBar jetpackBar;


	// Use this for initialization
	void Awake () {
		boundaries = new Vector2(this.GetComponent<BoxCollider2D> ().size.x / 2,
								 this.GetComponent<BoxCollider2D> ().size.y / 2);
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
		playerHUD = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<PlayerHUDScript>();
		jetpackBar = playerHUD.GetComponent<JetpackBar>();
	}

	void Start(){
		MusicManager.instance.PlayGameMusic();
		GameManager.instance.InitialSetup();
		PlayerUpgrader.ApplyPlayerUpgrades(ref player);
		jetpackBar.SetupBars(player.actionChargesMax, player.actionChargeTime);
		GameManager.instance.StartGameLoop();
	}

	void Update () {
		if (!IsPlayerInBoundary() && !player.GetIsLanded()){
			WarpPlayer();
		}

	}


	void OnTriggerExit2D(Collider2D other){
		if (other.gameObject.tag == "Planet" && other.GetComponent<PlanetScript>().GetPushForce().magnitude > 1f){
			PlanetScript planet = other.GetComponent<PlanetScript>();
			planet.transform.position = GetWarpedPosition(planet.transform.position);
			// clears the planet from player's interacted list if 
			if (player.GetLastPlanet() == planet.gameObject){
				player.ClearLastPlanet();
			}
		}
		else if (other.gameObject.tag == "CoinOrb" && other.GetComponent<OrbScript>().GetBlastVelocity().magnitude > 1f){
			other.gameObject.transform.position = GetWarpedPosition(other.gameObject.transform.position);
		}
	}


	private bool IsPlayerInBoundary(){
		Vector3 pos = player.transform.position;
		if (pos.x > boundaries.x + boundaryOffset || pos.x < -boundaries.x - boundaryOffset
			 || pos.y > boundaries.y + boundaryOffset || pos.y < -boundaries.y - boundaryOffset){
			return false;
		}
		else{
			return true;
		}
	}


	// creates a new copy of the player at the new place and destroys the old one after a few seconds
	// this is done to preserve the trail particle effect
	private void WarpPlayer(){
		Vector3 newPos = GetWarpedPosition(player.transform.position);

		// clone the player and put the new one in the new position. destroy the old one in a few seconds
		// this is done to preserve the trail effect
		GameObject oldPlayerObj = player.gameObject;
		GameObject newPlayerObj = Instantiate(oldPlayerObj, newPos, oldPlayerObj.transform.rotation);
		player = newPlayerObj.GetComponent<PlayerScript>();
		player.GetDataFromOldPlayer(oldPlayerObj.GetComponent<PlayerScript>());
		player.gameObject.name = "Player1";
		oldPlayerObj.tag = "Untagged";
		oldPlayerObj.name = "OldPlayer1";
		oldPlayerObj.GetComponent<BoxCollider2D>().enabled = false;
		GameManager.instance.UpdatePlayerObject(player);

		StartCoroutine(TimedDestroyObject(oldPlayerObj, 5f));

	}


	// helper function that destroys a given game object after t seconds
	private IEnumerator TimedDestroyObject(GameObject obj, float t){
		yield return new WaitForSeconds(t);
		Destroy(obj);
	}


	// helper function that finds the warped position of a given obj
	private Vector3 GetWarpedPosition(Vector3 position){
		// find whether or not the new warpped position will be inversed
		int factor = 1;
		/*
		Vector2 direction = player.GetComponent<Rigidbody2D>().velocity;
		float angle = Mathf.Abs(Mathf.Atan(direction.x / direction.y) * Mathf.Rad2Deg);
		if (angle > warpAngle && angle < 90 - warpAngle){
			factor = -1;
		}
		*/

		// find the new warpped position
		Vector3 newPos = position;
		if (newPos.x > boundaries.x)
			newPos = new Vector3(-boundaries.x, factor * newPos.y, 0);
		else if (newPos.x < -boundaries.x)
			newPos = new Vector3(boundaries.x, factor * newPos.y, 0);
		else if (newPos.y > boundaries.y)
			newPos = new Vector3(factor * newPos.x, -boundaries.y, 0);
		else if (newPos.y < -boundaries.y)
			newPos = new Vector3(factor * newPos.x, boundaries.y, 0);

		return newPos;
	}


	// spawns a random planet at a random place that's a set distance away from the last spawnpoint
	public void SpawnPlanet(int i, float planetSpeed){
		float spawnAngle = (lastSpawnAngle + Random.Range(30, 330)) % 360f;
		lastSpawnAngle = spawnAngle;

		GameObject planet;
		switch (i){
			case 0:
				planet = this.asteroid;
				break;
			case 1:
				planet = this.planetMedium;
				break;
			case 2:
				planet = this.planet_big;
				break;
			default:
				planet = this.asteroid;
				break;
		}

		float planetRadius = planet.GetComponent<CircleCollider2D>().radius * planet.transform.lossyScale.x;

		float hypotenuse = Mathf.Max(boundaries.x, boundaries.y) + planetRadius;
		float xPos = Mathf.Cos(spawnAngle * Mathf.Deg2Rad) * hypotenuse;
		float yPos = Mathf.Sin(spawnAngle * Mathf.Deg2Rad) * hypotenuse;
		if (xPos > boundaries.x){
			xPos = boundaries.x - planetRadius;
		}
		else if (xPos < -boundaries.x){
			xPos = -boundaries.x + planetRadius;
		}
		if (yPos > boundaries.y){
			yPos = boundaries.y - planetRadius;
		}
		else if (yPos < -boundaries.y){
			yPos = -boundaries.y + planetRadius;
		}

		GameObject obj = Instantiate(planet, new Vector3(xPos, yPos, 0), Quaternion.identity);
		GameManager.instance.ChangePlanetsOnScreenCount(1);
	}

}