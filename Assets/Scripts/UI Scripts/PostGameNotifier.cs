using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostGameNotifier : MonoBehaviour {
	private Image image;
	public GameObject gameOverText, objectiveText, player1ScoreInfo, player2ScoreInfo, player1ScoreText, player2ScoreText, winnerText;

	public float gameOverTextDisplayDuration;
	public int countDownToNextMapTime;
	public AudioClip countdownSound, finishSound, textPopUpSound, scoreSmashSound, winSound;

	private Animator animator;
	private AudioSource audioSource;

	// Use this for initialization
	void Awake () {
		image = this.GetComponent<Image>();
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();

		EnableImageText(false);
		gameOverText.SetActive(false);
	}


	// slowly fade in the image and the text at the designated speed, one by one
	public IEnumerator DisplayNotification(){
		EnableImageText(true);

		// play the animation 
		animator.SetBool("ShowResults", true);

		// wait until we're in the animation state, then wait for the animation to finish
		while(!animator.GetCurrentAnimatorStateInfo(0).IsName("ShowResults")){
			yield return null;
		}
		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
	}


	// play the animation to transition this screen out
	public void TransitionOut(){
		animator.SetBool("TransitionOut", true);
	}

	/*
	// shows countdown to the next map
	public IEnumerator CountDownToNextMap(){
		countDownToNextMapText.SetActive(true);

		for (int i = countDownToNextMapTime; i >= 0; i--){
			foreach(Text text in countDownToNextMapText.GetComponentsInChildren<Text>()){
				text.text = "Next Map in ..." + i;
			}
			yield return new WaitForSeconds(1f);
		}
	}
	*/


	// shows the Game Over Text
	public IEnumerator DisplayGameOverText(){
		gameOverText.SetActive(true);
		audioSource.PlayOneShot(finishSound);
		yield return new WaitForSeconds(gameOverTextDisplayDuration);
		gameOverText.SetActive(false);
	}


	// sets up the text to show the right results for this round
	public void SetUpText(string objectiveString, int p1Score, int p2Score, PlayerScript winner){
		foreach (Text text in objectiveText.GetComponentsInChildren<Text>()){
			text.text = objectiveString;
		}

		foreach (Text text in player1ScoreText.GetComponentsInChildren<Text>()){
			text.text = p1Score.ToString();
		}
		foreach (Text text in player2ScoreText.GetComponentsInChildren<Text>()){
			text.text = p2Score.ToString();
		}


		string winString;
		if (winner == null){
			winString = "Game is Tied.";
		}
		else{
		//	winString = "Player " + winner.GetPlayerID() + " wins the round!";
		}
		foreach (Text text in winnerText.GetComponentsInChildren<Text>()){
			//text.text = winString;
		}
	}

	// sets the image and texts to their default hidden state and then disable them
	private void EnableImageText(bool b){
		image.enabled = b;
		objectiveText.SetActive(b);
		player1ScoreInfo.SetActive(b);
		player2ScoreInfo.SetActive(b);
		player1ScoreText.SetActive(b);
		player2ScoreText.SetActive(b);
		winnerText.SetActive(b);
	}

	public void PlayTextPopUpSound(){
		this.audioSource.PlayOneShot(textPopUpSound);
	}

	public void PlayScoreSmashSound(){
		this.audioSource.PlayOneShot(scoreSmashSound);
	}

	public void PlayWinSound(){
		this.audioSource.PlayOneShot(winSound);
	}

}
