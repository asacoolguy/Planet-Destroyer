using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDScript : MonoBehaviour {
	public GameObject floatingText;
	private string objectiveString;

	private Text scoreText, difficultyText, coinText;
	private Button pauseButton;


	void Awake () {
		// sets up all the GUI elements
		scoreText = transform.parent.Find("Top Bar").Find("Score").GetComponent<Text>();
		difficultyText = transform.parent.Find("Top Bar").Find("Difficulty").GetComponent<Text>();
		coinText = transform.parent.Find("Top Bar").Find("Coin").GetComponent<Text>();
		pauseButton = transform.parent.Find("Top Bar").Find("Pause Button").GetComponent<Button>();
		pauseButton.onClick.AddListener(() => GameManager.instance.OnPauseButtonClick());

	}


	// Update is called once per frame
	void Update () {

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


	public void SetObjectiveString(string s){
		objectiveString = s;
	}


}
