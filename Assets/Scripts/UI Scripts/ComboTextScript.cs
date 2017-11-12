using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboTextScript : MonoBehaviour {
	private Animator myAnimator;
	private Text text;

	// Use this for initialization
	void Awake () {
		myAnimator = this.GetComponent<Animator>();
		text = this.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayIncreaseCombosAnimation(){
		myAnimator.SetTrigger("IncreaseCombo");
	}

	public void SetText(string s){
		text.text = s;
	}

}
