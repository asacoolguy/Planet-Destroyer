using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	private GameObject notificationObj, pauseBoxObj, difficultyIncreaseText;
	private float score;
	private int planetDestroyedCount, planetCombo, maxPlanetCombo;
	private int coinsCollected;
	public float comboFadeTime;
	private float currentComboFadeTime;
	private PlayerHUDScript playerHUD;
	private ComboTextScript comboText;
	private MapManager mapManager;

	private int difficulty;
	private bool canSpawn = true;
	private bool gamePaused = false;
	[SerializeField]private int planetsOnScreenCount;

	public AudioClip gameOverSound, difficultyIncreaseSound;


	// singleton behavior
	private static GameManager _instance;
	public static GameManager instance{
		get{
			if (_instance == null){
				_instance = GameObject.FindObjectOfType<GameManager>();
				DontDestroyOnLoad(_instance.gameObject);
			}

			return _instance;
		}
	}


	private bool gameRunning, gameLooping;

	void OnEnable(){
		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	void OnDisable(){
		SceneManager.sceneLoaded -= OnSceneFinishedLoading;
	}

	void Awake () {
		if (_instance == null){
			_instance = this;
			DontDestroyOnLoad(this);
		}
		else{
			if (this != _instance)
				Destroy(this.gameObject);
		}
	}

	void Update(){

	}


	// main game loop that carries the game through its states
	private IEnumerator GameLoop(){
		if (!gameLooping){
			gameLooping = true;
			// run the game 
			yield return StartCoroutine(RunGame());

			yield return StartCoroutine(GameOver());
		}
	}


	// game running state. spawn enemies and raise difficulty accordingly
	private IEnumerator RunGame(){
		playerHUD.SetIsHUDActive(true);
		playerHUD.UpdateDifficultyText(difficulty);
		gameRunning = true;

		while (gameLooping && gameRunning){
			// spawn planets based on difficulty
			if (canSpawn){
				int r = 0;
				float waitTime = 6f;
				float speed = 0.3f;
				if (difficulty == 1){
					r = Random.Range(0, 2);
					waitTime = 6f;
					speed = 0.3f;
					StartCoroutine(SpawnPlanet(Random.Range(0, difficulty), waitTime, speed));
				}
				else if(difficulty == 2){
					r = Random.Range(0, 3);
					waitTime = 5f;
					speed = 0.3f;
					StartCoroutine(SpawnPlanet(Random.Range(0, difficulty), waitTime, speed));
				}
				else if(difficulty == 3){
					r = Random.Range(0, 3);
					waitTime = 4f;
					speed = 0.4f;
					StartCoroutine(SpawnPlanet(Random.Range(0, difficulty), waitTime, speed));
				}
				else if(difficulty > 3){
					r = Random.Range(0, 3);
					waitTime = 3.5f;
					speed = 0.5f;
					StartCoroutine(SpawnPlanet(Random.Range(0, difficulty), waitTime, speed));
				}
			}

			// raise difficulty based on planets destoryed
			if (planetDestroyedCount >= 10 && planetDestroyedCount < 25){
				difficulty = 2;
				playerHUD.UpdateDifficultyText(difficulty);

			}
			else if (planetDestroyedCount >= 25){
				difficulty = (planetDestroyedCount - 25) / 20 + 3;
				playerHUD.UpdateDifficultyText(difficulty);
			}


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
		if (gameLooping && !gameRunning){
			GetComponent<AudioSource>().PlayOneShot(gameOverSound);
			Time.timeScale = 0;
			notificationObj.SetActive(true);

			string scoreString = "Score: " + Mathf.RoundToInt(score);
			if (score > PlayerPrefs.GetFloat("HighScore")){
				scoreString = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">New High Score: " + Mathf.RoundToInt(score) + "</color>";
				PlayerPrefs.SetFloat("HighScore", score);
			}

			PlayerPrefs.SetInt("TotalCoins", coinsCollected + PlayerPrefs.GetInt("TotalCoins"));

			notificationObj.transform.Find("Score Text").GetComponent<Text>().text = scoreString ;
			notificationObj.transform.Find("Combo Text").GetComponent<Text>().text = "Max Combo: " + maxPlanetCombo;
			notificationObj.transform.Find("Coin Text").GetComponent<Text>().text = "Coins Collected: " + coinsCollected;

			playerHUD.SetIsHUDActive(false);


			while (GetComponent<AudioSource>().isPlaying){
				yield return null;
			}

			gameLooping = false;
		}
	}


	private IEnumerator SpawnPlanet(int i, float waitTime, float speed){
		mapManager.SpawnPlanet(i, speed);
		canSpawn = false;

		yield return new WaitForSeconds (waitTime);
		canSpawn = true;
	}


	public void LoadGame(){
		gameLooping = false;
		gameRunning = false;
		SceneManager.LoadScene("Planet Defense");
	}

	public void LoadMainMenu(){
		gameLooping = false;
		gameRunning = false;
		SceneManager.LoadScene("Main Menu");
	}

	public void LoadUpgrade(){
		gameLooping = false;
		gameRunning = false;
		SceneManager.LoadScene("Upgrade");
	}


	public void StopGameRunning(){
		gameRunning = false;
	}


	// runs when the scene is loaded, sets up variables for this scene
	private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode){
		// make sure not to run this function for the main menu scene
		Time.timeScale = 1;
	}

	public void InitialSetup(){
		gameRunning = false;
		gameLooping = false;
		difficulty = 1;
		score = 0f;
		coinsCollected = 0;
		planetCombo = 0;
		maxPlanetCombo = 0;
		planetDestroyedCount = 0;
		planetsOnScreenCount = 0;
		mapManager = GameObject.FindObjectOfType<MapManager>();
		GameObject canvas = GameObject.FindGameObjectWithTag("GameCanvas");
		playerHUD = canvas.transform.Find("Player HUD").GetComponent<PlayerHUDScript>();
		comboText = canvas.transform.Find("Combo Text").GetComponent<ComboTextScript>();

		notificationObj = canvas.transform.Find("Notification").gameObject;
		notificationObj.SetActive(false);
		notificationObj.transform.Find("Restart Button").GetComponent<Button>().onClick.AddListener(() => LoadGame());
		notificationObj.transform.Find("Upgrades Button").GetComponent<Button>().onClick.AddListener(() => LoadUpgrade());
		notificationObj.transform.Find("Main Menu Button").GetComponent<Button>().onClick.AddListener(() => LoadMainMenu());

		pauseBoxObj = canvas.transform.Find("Pause Box").gameObject;
		pauseBoxObj.SetActive(false);
		pauseBoxObj.transform.Find("Resume Button").GetComponent<Button>().onClick.AddListener(() => OnPauseButtonClick());
		pauseBoxObj.transform.Find("Restart Button").GetComponent<Button>().onClick.AddListener(() => LoadGame());
		pauseBoxObj.transform.Find("Upgrades Button").GetComponent<Button>().onClick.AddListener(() => LoadUpgrade());
		pauseBoxObj.transform.Find("Main Menu Button").GetComponent<Button>().onClick.AddListener(() => LoadMainMenu());
		gamePaused = false;

		difficultyIncreaseText = canvas.transform.Find("Difficulty Increase Text").gameObject;
		difficultyIncreaseText.SetActive(false);

		playerHUD.SetIsHUDActive(false);
	}


	public void StartGameLoop(){
		if (!gameLooping){
			StartCoroutine(GameLoop());
		}
		else{
			Debug.Log("Error: game loop cannot start because game is already running!");
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


	public void CollectCoin(int value){
		coinsCollected += value;
		playerHUD.UpdateCoinText(coinsCollected);
	}


	public int GetTotalCoins(){
		return PlayerPrefs.GetInt("TotalCoins");
	}

	public void SpendTotalCoins(int coinsSpent){
		PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins") - coinsSpent);
	}

	public void ChangePlanetsOnScreenCount(int i){
		planetsOnScreenCount += i;
	}

	private IEnumerator ShowDifficultyIncreaseText(){
		difficultyIncreaseText.SetActive(true);
		GetComponent<AudioSource>().PlayOneShot(difficultyIncreaseSound);
		yield return new WaitForSeconds(1f);
		difficultyIncreaseText.SetActive(false);
	}


	// handles handing when the pause button is clicked
	public void OnPauseButtonClick(){
		if (gamePaused){
			// if game is already paused, unpause the game
			pauseBoxObj.SetActive(false);
			playerHUD.SetIsHUDActive(true);
			Time.timeScale = 1f;
			gamePaused = false;
		}
		else{
			// if game is not paused, pause the game
			pauseBoxObj.SetActive(true);
			playerHUD.SetIsHUDActive(false);
			Time.timeScale = 0f;
			gamePaused = true;
		}
	}


	public void ClearData(){
		PlayerPrefs.SetFloat("HighScore", 0f);
		PlayerPrefs.SetInt("TotalCoins", 0);
		PlayerUpgrader.ClearUpgrades();
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
