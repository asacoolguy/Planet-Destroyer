using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour {
	//public GameObject stageSelectionBox;
	public GameObject mainMenuBox;

	// Use this for initialization
	void Start () {
		//HideStageSelectionBox();
		MusicManager.instance.PlayMainMenuMusic();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowStageSelectionBox(){
		//stageSelectionBox.SetActive(true);
		mainMenuBox.SetActive(false);
	}

	public void HideStageSelectionBox(){
		//stageSelectionBox.SetActive(false);
		mainMenuBox.SetActive(true);
	}

	// give tell gameManager to start the game
	public void LoadGame(){
		GameManager.instance.LoadGame();
	}


	public void LoadUpgrade(){
		GameManager.instance.LoadUpgrade();
	}


	public void ClearData(){
		GameManager.instance.ClearData();
	}
}
