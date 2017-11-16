using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Overall manager of the entire game. Handles scene loading and game states.
/// </summary>
public class OldGameManager : MonoBehaviour {	
	// singleton behavior
	public static OldGameManager instance = null;
	private bool gameLooping;

	// UI stuff
	private PlayerHUDScript player1HUD, player2HUD;
	private PreGameNotifier preGameNotifier;
	private PostGameNotifier postGameNotifier;
	private TournamentProgress tournamentProgress;
	private PlayerScript player1, player2;

	// timer
	private float timeLimit;
	private float timeLeft;

	// tournament info
	[SerializeField]private string[] mapNames;
	private int currentMapIndex;
	[SerializeField]private int roundsRequiredToWin;
	private int currentRound;
	[SerializeField]private int player1WinCount, player2WinCount;

	// getting this map's info
	private MapManager mapManager;

	// developer tools
	public bool showPreNotification = true;
	public bool showPostNotification = true;

	void OnEnable(){
		//SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	void OnDisable(){
		//SceneManager.sceneLoaded -= OnSceneFinishedLoading;
	}

	void Awake () {
		// makes this into a singleton
		if (instance == null){
			instance = this;
		}
		else if(instance != this){
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);

		// initiate the GameState
		//gameState = GameState.MainMenu;
		gameLooping = false;
	}

	void Update () {


	}

	// loop that is run once on every map
	private IEnumerator GameLoop(){
		if (gameLooping == false){
			gameLooping = true;

			// show this map's objective before the game starts
			if (showPreNotification)
				yield return StartCoroutine(ShowGameObjective());

			// run the game 
			yield return StartCoroutine(RunGame());

			// end the game, show results 
			if (showPostNotification)
				yield return StartCoroutine(GameOver());
		}
		else{
			Debug.Log("error: gameloop cannot start because game is already looping!");
		}
	}

	// coroutine that shows the round's objects before the round starts
	private IEnumerator ShowGameObjective(){
		player1HUD.GetComponent<PlayerHUDScript>().SetIsHUDActive(false);
		player2HUD.GetComponent<PlayerHUDScript>().SetIsHUDActive(false);

		// TODO: improve animation for showing objectives
		// initial wait for player to orient themselves with the map 
		yield return new WaitForSeconds(0.5f);

		//preGameNotifier.SetUpText(currentRound, mapManager.GetPreGameObjectiveText());

		yield return StartCoroutine(preGameNotifier.DisplayNotification());

		yield return StartCoroutine(preGameNotifier.HideNotification());
		yield return StartCoroutine(preGameNotifier.DisplayCountdown());
	}


	private IEnumerator RunGame(){
		player1HUD.GetComponent<PlayerHUDScript>().SetIsHUDActive(true);
		player2HUD.GetComponent<PlayerHUDScript>().SetIsHUDActive(true);
		player1.SetCanGetPoints(true);
		player2.SetCanGetPoints(true);

		while(timeLeft > 0){
			// decrease the timer
			timeLeft -= Time.deltaTime;
			//player1HUD.GetComponent<PlayerHUDScript>().UpdateTimerText(timeLeft);
			//player2HUD.GetComponent<PlayerHUDScript>().UpdateTimerText(timeLeft);
			yield return null;
		}
	}

	private IEnumerator GameOver(){
		// disable controls
		player1HUD.GetComponent<PlayerHUDScript>().SetIsHUDActive(false);
		player2HUD.GetComponent<PlayerHUDScript>().SetIsHUDActive(false);

		player1.SetCanGetPoints(true);
		player2.SetCanGetPoints(false);

		// find the winner
		// TODO: for now score is king. when other win conditions come in, move this code elsewhere
		//PlayerScript winner = null; 
		//if (player1.GetScore() > player2.GetScore()){
		//	winner = player1;
		//}
		//else if(player2.GetScore() > player1.GetScore()){
		//	winner = player2;
		//}

		// show the game over text first
		yield return StartCoroutine(postGameNotifier.DisplayGameOverText());

		// give info to postGameNotifier
		//postGameNotifier.SetUpText(mapManager.GetPostGameObjectiveText(), player1.GetScore(), player2.GetScore(), winner);

		// display the results of the game
		yield return StartCoroutine(postGameNotifier.DisplayNotification());

		yield return new WaitForSeconds(2f);

		// hide the results and show tournament progress
		postGameNotifier.TransitionOut();
		//tournamentProgress.SetUpScoreBars(player1.GetWinCount(), player2.GetWinCount());
		tournamentProgress.TransitionIn();

		yield return new WaitForSeconds(2f);

		// increment the winCount of the player and show it with the bar scores
		//tournamentProgress.IncrementWinnerScoreBar(winner);

		yield return new WaitForSeconds(2f);

		/*
		// if a player has won the tournament, display tournament results
		if (Mathf.Max(player1.GetWinCount(), player2.GetWinCount()) >= roundsRequiredToWin){
			// TODO: make a nice tournament results menu
			if (player1.GetWinCount() > player2.GetWinCount()){
				tournamentProgress.ShowTournamentWinner(player1);
			}
			else{
				tournamentProgress.ShowTournamentWinner(player2);
			}
			yield return new WaitForSeconds(5f);

			gameLooping = false;
			SceneManager.LoadScene("Main Menu");
		}
		// otherwise the tournament keeps going
		else{
			// countdown to the next map
			//yield return StartCoroutine(postGameNotifier.CountDownToNextMap());

			// log down the winCount of the players
			player1WinCount = player1.GetWinCount();
			player2WinCount = player2.GetWinCount();

			gameLooping = false;

			// load the next map
			currentMapIndex += 1;
			// if we are on the last map 
			LoadMap(mapNames[currentMapIndex % mapNames.Length]);
		}
		*/
	}

	// runs when the scene is loaded, sets up variables for this scene
	/*
	private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode){
		// make sure not to run this function for the main menu scene
		if (scene.name != "Main Menu"){
			//this.gameState = GameState.PreGameHint;

			// find the local mapManager
			mapManager = GameObject.FindGameObjectsWithTag("MapManager")[0].GetComponent<MapManager>();
			// get player elements from mapManager
			player1 = mapManager.player1.GetComponent<PlayerScript>();
			// get UI elements from mapManager
			player1HUD = mapManager.gameCanvas.transform.Find("Player1 HUD").GetComponent<PlayerHUDScript>();
			player2HUD = mapManager.gameCanvas.transform.Find("Player2 HUD").GetComponent<PlayerHUDScript>();
			player1HUD.SetObjectiveString(mapManager.GetPostGameObjectiveText());
			player2HUD.SetObjectiveString(mapManager.GetPostGameObjectiveText());
			preGameNotifier = mapManager.gameCanvas.transform.Find("PreGame Notification").GetComponent<PreGameNotifier>();
			postGameNotifier = mapManager.gameCanvas.transform.Find("PostGame Notification").GetComponent<PostGameNotifier>();
			tournamentProgress = mapManager.gameCanvas.transform.Find("Tournament Progress").GetComponent<TournamentProgress>();
			// get Map info from mapManager
			timeLimit = mapManager.timeLimit;
			timeLeft = timeLimit;	
			// increase round 
			currentRound += 1;

			StartCoroutine(GameLoop());
		}
	}
	*/

	// resets this map
	public void ResetMap(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void LoadMap(string sceneName){
		if (gameLooping == false){
			SceneManager.LoadScene(sceneName);
		}
	}

	// called by Main Menu to start a certain tournament, given all the required info
	public void StartGalaxy(int roundsRequired, string[] mapNames){
		if (gameLooping == false){
			roundsRequiredToWin = roundsRequired;
			currentRound = 0;
			player1WinCount = 0;
			player2WinCount = 0;

			this.mapNames = mapNames;
			currentMapIndex = 0;

			LoadMap(this.mapNames[currentMapIndex]);
		}
	}
}
