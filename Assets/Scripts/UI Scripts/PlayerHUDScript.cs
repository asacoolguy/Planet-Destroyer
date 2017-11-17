using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDScript : MonoBehaviour {
	public GameObject floatingText;
	private PlayerScript player;
	private PlayerAudioScript playerAudio;
	private string objectiveString;

	private Slider slider;
	private Text scoreText, difficultyText, coinText;
	private Button jumpButton, actionButton, pauseButton;

	public bool isHUDActive;

	// charge variables
	private float jumpButtonHeldTime;
	private bool jumpButtonHeld;
	private bool jumpButtonHeldMaxedOut;

	void Awake () {
		// sets up all the GUI elements
		scoreText = transform.parent.Find("Top Bar").Find("Score").GetComponent<Text>();
		difficultyText = transform.parent.Find("Top Bar").Find("Difficulty").GetComponent<Text>();
		coinText = transform.parent.Find("Top Bar").Find("Coin").GetComponent<Text>();
		pauseButton = transform.parent.Find("Top Bar").Find("Pause Button").GetComponent<Button>();
		pauseButton.onClick.AddListener(() => GameManager.instance.OnPauseButtonClick());

		slider = transform.Find("Power Slider").GetComponent<Slider>();
		jumpButton = transform.Find("Jump Button").GetComponent<Button>();
		actionButton = transform.Find("Action Button").GetComponent<Button>();
		UpdatePlayerObj(GameObject.FindGameObjectWithTag("Player"));

		jumpButtonHeld = false;
		jumpButtonHeldTime = 0f;
		jumpButtonHeldMaxedOut = false;

		//EnableHUDInteraction(false);
		isHUDActive = false;

	}


	// Update is called once per frame
	void Update () {
		UpdateSliderValue();


		if (isHUDActive && player.GetIsDead() == false){
			if (player.GetIsLanded() && player.GetCanJump()){
				EnableJumpButtonInteraction(true);
			}
			else{
				EnableJumpButtonInteraction(false);
			}
			if (!player.GetIsLanded() && player.GetCanAction()){
				EnableActionButtonInteraction(true);
			}
			else{
				EnableActionButtonInteraction(false);
			}
		}

		if (jumpButtonHeld){
			jumpButtonHeldTime += Time.deltaTime;
		}
		if (jumpButtonHeld && jumpButtonHeldMaxedOut == false && jumpButtonHeldTime > slider.maxValue){
			jumpButtonHeldMaxedOut = true;
			playerAudio.PlayMaxedChargingSound();
		}


		if (Input.GetKeyDown(KeyCode.X)){
			StartJumpButtonPress();
		}
		if (Input.GetKeyUp(KeyCode.X)){
			EndJumpButtonPress();
		}
		if (Input.GetKeyDown(KeyCode.Z)){
			OnActionButtonPress();
		}

	}

	public void UpdateScoreText(float score){
		scoreText.text = "Score: " + Mathf.RoundToInt(score);
	}

	public void UpdateDifficultyText(int difficulty){
		difficultyText.text = "Difficulty: " + difficulty;
	}

	public void UpdateCoinText(int coinCollected){
		coinText.text = "Coins Collected: " + coinCollected;
	}

	public void UpdateSliderValue(){
		slider.value = Mathf.MoveTowards(slider.value, jumpButtonHeldTime, 1f);
	}

	public void StartJumpButtonPress(){
		if (isHUDActive && player.GetCanJump()){
			jumpButtonHeld = true;
			playerAudio.PlayChargingSound();
		}
	}

	public void EndJumpButtonPress(){
		if (isHUDActive && player.GetCanJump()){
			player.LeavePlanet(jumpButtonHeldTime);
			ResetChargeButton();
		}
	}

	public void OnActionButtonPress(){
		if (isHUDActive && player.GetCanAction()){
			player.ActivatePlayerAction();
		}
	}

	public void EnableJumpButtonInteraction(bool b){
		jumpButton.interactable = b;

		// stop charging when the HUD is disabled
		if (b == false){
			ResetChargeButton();
		}
	}

	public void EnableActionButtonInteraction(bool b){
		actionButton.interactable = b;
	}

	public void ShowFloatingText(float value, Vector3 position){
		string s = "+" + Mathf.RoundToInt(value) + "pt";
		if (value > 1f){
			s += "s";
		}

		Color c = Color.yellow;
		GameObject text = Instantiate(floatingText, Vector3.zero, Quaternion.identity) as GameObject;
		text.transform.SetParent(transform.parent, false);
		text.transform.eulerAngles = Vector3.zero;
		text.GetComponent<RectTransform>().position = position;
		text.GetComponent<Text>().text = s;
		text.GetComponent<Text>().color = c;

		StartCoroutine(DestroyText(text, 1f));
	}

	private IEnumerator DestroyText(GameObject text, float seconds){
		yield return new WaitForSeconds(seconds);
		Destroy(text);
	}

	private void ResetChargeButton(){
		jumpButtonHeld = false;
		jumpButtonHeldMaxedOut = false;
		playerAudio.StopChargingSound();
		jumpButtonHeldTime = 0f;
	}

	public void SetIsHUDActive(bool b){
		isHUDActive = b;
		pauseButton.interactable = b;
	}

	public void SetObjectiveString(string s){
		objectiveString = s;
	}


	public void UpdatePlayerObj(GameObject playerObj){
		player = playerObj.GetComponent<PlayerScript>();
		playerAudio = player.GetPlayerAudio();
		slider.maxValue = player.maxChargeTime;
	}

}
