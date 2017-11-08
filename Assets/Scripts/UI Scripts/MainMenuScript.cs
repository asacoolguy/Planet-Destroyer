using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour {
	public GameObject stageSelectionBox;
	public GameObject mainMenuBox;

	private GameManager gameManager;

	// Use this for initialization
	void Awake () {
		HideStageSelectionBox();
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowStageSelectionBox(){
		stageSelectionBox.SetActive(true);
		mainMenuBox.SetActive(false);
	}

	public void HideStageSelectionBox(){
		stageSelectionBox.SetActive(false);
		mainMenuBox.SetActive(true);
	}

	// give GameManager the info needed to run galaxy 1
	public void LoadGalaxyOne(){
		string[] mapNames = new string[3];
		mapNames[0] = "Coin Intro";
		mapNames[1] = "Planet Intro";
		mapNames[2] = "Warp Intro";
		gameManager.StartGalaxy(3, mapNames);
	}
}
