using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TempGameManager : MonoBehaviour {
	public GameObject playerObj;
	public GameObject playerHUDObj, notificationObj;
	private int score, planetDestroyedCount;
	private PlayerHUDScript playerHUD;
	private MapManager mapManager;

	private int difficulty;
	private bool canSpawn = true;

	public AudioClip gameOverSound;

	// singleton behavior
	//public static TempGameManager instance = null;
	[SerializeField]private bool gameLooping, gameRunning;

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
		gameLooping = false;
		gameRunning = false;
		Time.timeScale = 1;
		difficulty = 1;
		score = 0;
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
			difficulty = planetDestroyedCount / 10;
			playerHUD.UpdateDifficultyText(difficulty);

			yield return null;
		}
	}


	// game over state. show game over screen and result screen
	private IEnumerator GameOver(){
		GetComponent<AudioSource>().PlayOneShot(gameOverSound);
		Time.timeScale = 0;
		notificationObj.SetActive(true);
		notificationObj.transform.Find("Score Text").GetComponent<Text>().text = "Score: " + score;
		playerHUD.SetIsHUDActive(false);

		gameLooping = false;
		gameRunning = false;

		yield return null;	
	}


	private IEnumerator SpawnPlanet(int i){
		mapManager.SpawnPlanet(i);
		canSpawn = false;

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
		score += value;
		playerHUD.UpdateScoreText(score);
		playerHUD.ShowFloatingText(value, position);
	}

	public void IncrementPlanetDestroyedCount(int value){
		planetDestroyedCount += value;
	}
}
