using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TempGameManager : MonoBehaviour {
	public GameObject playerObj;
	public GameObject playerHUDObj, notificationObj, comboTextObj;
	private float score;
	private int coinCollected, planetDestroyedCount, planetCombo, maxPlanetCombo;
	public float comboFadeTime;
	private float currentComboFadeTime;
	private PlayerHUDScript playerHUD;
	private ComboTextScript comboText;
	private MapManager mapManager;

	public int startingDifficulty = 1;
	private int difficulty;
	private bool canSpawn = true;

	public AudioClip gameOverSound;

	// singleton behavior
	//public static TempGameManager instance = null;
	private bool gameLooping, gameRunning;

	void OnEnable(){
		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	void OnDisable(){
		SceneManager.sceneLoaded -= OnSceneFinishedLoading;
	}

	void Awake () {
		// makes this into a singleton
		/*
		if (instance == null){
			instance = this;
		}
		else if(instance != this){
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
		*/

		mapManager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
		playerHUD = playerHUDObj.GetComponent<PlayerHUDScript>();
		comboText = comboTextObj.GetComponent<ComboTextScript>();
		gameLooping = false;
		gameRunning = false;
		Time.timeScale = 1;
		difficulty = 1;
		score = 0f;
		coinCollected = 0;
		planetCombo = 0;
		maxPlanetCombo = 0;
		planetDestroyedCount = 0;
	}


	void Start(){
		StartCoroutine(GameLoop());
	}


	// main game loop that carries the game through its states
	private IEnumerator GameLoop(){
		if (gameLooping == false){
			gameLooping = true;

			notificationObj.SetActive(false);
			playerHUD.SetIsHUDActive(false);

			// run the game 
			yield return StartCoroutine(RunGame());

			yield return StartCoroutine(GameOver());
		}
		else{
			Debug.Log("error: gameloop cannot start because game is already looping!");
		}
	}


	// game running state. spawn enemies and raise difficulty accordingly
	private IEnumerator RunGame(){
		playerHUD.SetIsHUDActive(true);
		playerHUD.UpdateDifficultyText(difficulty);
		gameRunning = true;

		while (gameRunning){
			// spawn things
			if (canSpawn){
				StartCoroutine(SpawnPlanet(Random.Range(0, difficulty)));
			}

			// raise difficulty based on planets destoryed
			difficulty = planetDestroyedCount / 10 + startingDifficulty;
			playerHUD.UpdateDifficultyText(difficulty);

			if (currentComboFadeTime > 0){
				currentComboFadeTime -= Time.deltaTime;
			}
			else{
				planetCombo = 0;
			}

			yield return null;
		}
	}


	// game over state. show game over screen and result screen
	private IEnumerator GameOver(){
		GetComponent<AudioSource>().PlayOneShot(gameOverSound);
		Time.timeScale = 0;
		notificationObj.SetActive(true);
		notificationObj.transform.Find("Score Text").GetComponent<Text>().text = "Score: " + Mathf.RoundToInt(score);
		notificationObj.transform.Find("Combo Text").GetComponent<Text>().text = "Max Combo: " + maxPlanetCombo;
		playerHUD.SetIsHUDActive(false);

		gameLooping = false;
		gameRunning = false;

		yield return null;	
	}


	private IEnumerator SpawnPlanet(int i){
		mapManager.SpawnPlanet(i);
		canSpawn = false;

		float waitSec = 0f;
		if (difficulty < 4f){
			waitSec = 6f - difficulty + i * 1.5f;
		}
		else{
			waitSec = (i + 1) * 1.5f;
		}

		yield return new WaitForSeconds (6f - difficulty + i * 2);
		canSpawn = true;
	}


	public void ResetMap(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}


	public void StopGameRunning(){
		gameRunning = false;
	}


	// runs when the scene is loaded, sets up variables for this scene
	private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode){
		// make sure not to run this function for the main menu scene
		if (scene.name != "Main Menu"){
			//this.gameState = GameState.PreGameHint;

			//StartCoroutine(GameLoop());
		}
	}


	public void IncrementScore(int value, Vector3 position){
		currentComboFadeTime = comboFadeTime;
		planetCombo += 1;
		maxPlanetCombo = Mathf.Max(planetCombo, maxPlanetCombo);

		float newValue = value;
		if (planetCombo > 2){
			comboText.PlayIncreaseCombosAnimation();
			comboText.SetText(planetCombo + " COMBO!");
			newValue = value * (1f + planetCombo * 0.2f);
		}

		score += newValue;
		playerHUD.UpdateScoreText(score);
		playerHUD.ShowFloatingText(newValue, position);
		planetDestroyedCount += 1;
	}


	public void GetCoin(int value){
		coinCollected += value;
		playerHUD.UpdateCoinText(coinCollected);
	}


	public static float GetAngleFromVector(Vector3 input){
		float angle;
		if (input.x == 0) {
			if (input.y > 0)
				angle = 90;
			else{
				angle = 270;
			}
		}
		else{
			angle =  Mathf.Atan2 (input.y, input.x) * Mathf.Rad2Deg;
		}

		return angle;
	}
}
