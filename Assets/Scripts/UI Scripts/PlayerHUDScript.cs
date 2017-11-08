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
	private Text scoreText, difficultyText;
	private Button jumpButton, actionButton;

	public bool isHUDActive;

	// charge variables
	private float jumpButtonHeldTime;
	private bool jumpButtonHeld;
	private bool jumpButtonHeldMaxedOut;

	void Awake () {
		// sets up all the GUI elements
		scoreText = transform.Find("Top Bar").Find("Score").gameObject.GetComponent<Text>();
		difficultyText = transform.Find("Top Bar").Find("Difficulty").gameObject.GetComponent<Text>();
		slider = transform.Find("Power Slider").gameObject.GetComponent<Slider>();
		jumpButton = transform.Find("Jump Button").gameObject.GetComponent<Button>();
		actionButton = transform.Find("Action Button").gameObject.GetComponent<Button>();
		UpdatePlayerObj(GameObject.FindGameObjectWithTag("Player"));

		jumpButtonHeld = false;
		jumpButtonHeldTime = 0f;
		jumpButtonHeldMaxedOut = false;

		//EnableHUDInteraction(false);
		isHUDActive = false;

	}

	void Start(){

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

	}

	public void UpdateScoreText(int score){
		scoreText.text = "Score: " + score;
	}

	public void UpdateDifficultyText(int difficulty){
		difficultyText.text = "Difficulty: " + difficulty;
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
		player.ActivatePlayerAction();
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

	public void ShowFloatingText(int value, Vector3 position){
		string s = "+" + value + "pt";
		if (value > 1){
			s += "s";
		}

		Color c = Color.yellow;
		if (value < 0){
			c = Color.red;
		}

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
