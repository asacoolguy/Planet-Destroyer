using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TournamentProgress : MonoBehaviour {
	private Animator animator;
	public GameObject player1Sprites, player2Sprites, winnerTexts;
	public GameObject[] player1Bars, player2Bars;
	private Image image;
	private AudioSource audiosource;
	public AudioClip increaseBarSound, tournamentWinSound;

	void Awake() {
		animator = GetComponent<Animator>();
		audiosource = GetComponent<AudioSource>();
		image = GetComponent<Image>();
		EnableImageText(false);
		winnerTexts.SetActive(false);
	}


	// play the animation to transition this screen in
	public void TransitionIn(){
		EnableImageText(true);
		animator.SetBool("TransitionIn", true);
	}


	// set up the score bars to display the current scores
	public void SetUpScoreBars(int player1Score, int player2Score){
		// make sure the scores aren't out of bounds
		if (player1Score >= player1Bars.Length || player2Score >= player2Bars.Length){
			Debug.Log("error, score is larger than amount of score bars available!");
			return;
		}

		for (int i = 0; i < player1Score; i++){
			foreach (Image img in player1Bars[i].GetComponentsInChildren<Image>()){
				img.color = Color.yellow;
			}
		}
		for (int i = 0; i < player2Score; i++){
			foreach (Image img in player2Bars[i].GetComponentsInChildren<Image>()){
				img.color = Color.yellow;
			}
		}
	}


	// play animation that shows which player won this round
	// note that if winner is null then it's a tie and nothing happens
	/*
	public void IncrementWinnerScoreBar(PlayerScript winner){
		if (winner != null){
			GameObject[] bars = player1Bars;
			if (winner.GetPlayerID() == 2){
				bars = player2Bars;
			}
			// increment winner's winCount and the bars
			foreach (Image i in bars[winner.GetWinCount()].GetComponentsInChildren<Image>()){
				i.color = Color.yellow;
			}
			winner.SetWinCount(winner.GetWinCount() + 1);
			audiosource.PlayOneShot(increaseBarSound);
		}
	}
	*/

	// display the text to indicate who won the tournament
	public void ShowTournamentWinner(PlayerScript winner){
		winnerTexts.SetActive(true);
		foreach(Text text in winnerTexts.GetComponentsInChildren<Text>()){
			//text.text = "Player " + winner.GetPlayerID() + " wins!";
		}
		audiosource.PlayOneShot(tournamentWinSound);
	}


	// Enable/Disable all the image and text components
	public void EnableImageText(bool b){
		image.enabled = b;
		player1Sprites.SetActive(b);
		player2Sprites.SetActive(b);
		foreach(GameObject bar in player1Bars){
			bar.SetActive(b);
		}
		foreach(GameObject bar in player2Bars){
			bar.SetActive(b);
		}
	}
}
