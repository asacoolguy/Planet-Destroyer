using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreGameNotifier : MonoBehaviour {
	private Image image;
	public GameObject countdown, roundText, objectiveText;

	public float displayDuration;
	public AudioClip countdownSound, startSound;

	private Animator animator;

	// Use this for initialization
	void Awake () {
		image = this.GetComponent<Image>();

		EnableImageText(false);
		countdown.SetActive(false);

		animator = GetComponent<Animator>();
	}


	// slowly fade in the image and the text at the designated speed, one by one
	public IEnumerator DisplayNotification(){
		// first enable the images and texts back up
		EnableImageText(true);

		// play the animation to show notifications
		animator.SetBool("ShowNotification", true);

		yield return null;

		// wait until we're in the animation state, then wait for the animation to finish
		while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Fade In")){
			yield return null;
		}
		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

		// show the info for a set amount of seconds
		yield return new WaitForSeconds(displayDuration);
	}


	// slowly fade out the image and the text at the designated speed, all together
	public IEnumerator HideNotification(){
		animator.SetBool("ShowNotification", false);

		// wait until we're in the animation state, then wait for the animation to finish
		while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Fade Out")){
			yield return null;
		}
		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

		// make sure the values are at the target and disable them
		EnableImageText(false);
	}


	// shows the countdown animaion
	public IEnumerator DisplayCountdown(){
		countdown.SetActive(true);

		for (int i = 3; i > 0; i--){
			foreach(Text text in countdown.GetComponentsInChildren<Text>()){
				text.text = i.ToString();
			}
			this.GetComponent<AudioSource>().PlayOneShot(countdownSound);
			yield return new WaitForSeconds(1f);
		}

		foreach(Text text in countdown.GetComponentsInChildren<Text>()){
			text.text = "GO";
		}
		this.GetComponent<AudioSource>().PlayOneShot(startSound);

		yield return new WaitForSeconds(1f);
		countdown.SetActive(false);
	}


	// sets up the text objects with info
	public void SetUpText(int roundNumber, string objective){
		foreach(Text text in roundText.GetComponentsInChildren<Text>()){
			text.text = "Round " + roundNumber;
		}
		foreach(Text text in objectiveText.GetComponentsInChildren<Text>()){
			text.text = objective;
		}
	}


	// disable image and test
	private void EnableImageText(bool b){
		image.enabled = b;
		roundText.SetActive(b);
		objectiveText.SetActive(b);
	}

}
