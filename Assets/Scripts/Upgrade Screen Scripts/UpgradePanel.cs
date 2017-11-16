using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour {
	public GameObject[] upgradeIcons;
	private GameObject detailIcon;
	private Text detailText, detailCost;
	private Button purchaseButton;
	private AudioSource myAudiosource;

	public AudioClip selectSound, purchaseSound;

	private GameObject selectedUpgrade = null;
	private int selectedUpgradeLevel, selectedUpgradeMaxLevel, selectedUpgradeCost;

	// Use this for initialization
	void Awake () {
		myAudiosource = GetComponent<AudioSource>();
		detailIcon = transform.Find("Detail Panel").Find("Icon").gameObject;
		detailText = transform.Find("Detail Panel").Find("Description").GetComponent<Text>();
		detailCost = transform.Find("Detail Panel").Find("Cost").GetComponent<Text>();
		purchaseButton = transform.Find("Detail Panel").Find("Purchase Button").GetComponent<Button>();

	}

	void Start(){
		MusicManager.instance.PlayUpgradeMusic();
		UpdateTotalCoinAmount();
		UpdateIconPanel();
		UpdateDetailPanel();
	}


	// selects a particular icon and deselects all others. 
	public void SelectUpgrade(string upgradeName){
		if (selectedUpgrade != null && selectedUpgrade.name == upgradeName){
			selectedUpgrade.transform.Find("Icon").GetComponent<Animator>().SetTrigger("GetSelected");
			myAudiosource.PlayOneShot(selectSound);
			return;
		}

		foreach (GameObject obj in upgradeIcons){
			if (upgradeName != null && obj.name == upgradeName){
				selectedUpgrade = obj;
				obj.transform.Find("Icon").GetComponent<Animator>().SetTrigger("GetSelected");
				obj.transform.Find("Icon").GetComponent<Animator>().SetBool("Selected", true);
				myAudiosource.PlayOneShot(selectSound);
				UpdateDetailPanel();
			}
			else{
				obj.transform.Find("Icon").GetComponent<Animator>().SetBool("Selected", false);
			}
		}
	}


	// updates the icon panel with correct levels using playerUpgrader
	private void UpdateIconPanel(){
		foreach (GameObject obj in upgradeIcons){
			Slider slider = obj.transform.Find("Bar").GetComponent<Slider>();
			slider.value = PlayerUpgrader.GetUpgradeLevel(obj.name);
		}
	}

	// updates the detail panel with info about the currently selected icon
	private void UpdateDetailPanel(){
		if (selectedUpgrade == null){
			// if nothing is selected, don't show any details
			detailIcon.SetActive(false);
			detailCost.gameObject.SetActive(false);
			detailText.gameObject.SetActive(false);
			purchaseButton.gameObject.SetActive(false);
		}
		else{
			// show the detail gameobjects and update their values accordingly
			detailIcon.SetActive(true);
			detailCost.gameObject.SetActive(true);
			detailText.gameObject.SetActive(true);
			purchaseButton.gameObject.SetActive(true);

			selectedUpgradeLevel = PlayerUpgrader.GetUpgradeLevel(selectedUpgrade.name);
			selectedUpgradeMaxLevel = (int) selectedUpgrade.transform.Find("Bar").GetComponent<Slider>().maxValue;
			selectedUpgradeCost = PlayerUpgrader.GetUpgradeCost(selectedUpgrade.name, selectedUpgradeLevel);

			detailIcon.transform.localScale = selectedUpgrade.transform.Find("Icon").localScale;
			detailIcon.GetComponent<Image>().sprite = selectedUpgrade.transform.Find("Icon").GetComponent<Image>().sprite;
			detailText.text = PlayerUpgrader.GetUpgradeDescription(selectedUpgrade.name, selectedUpgradeLevel);
			purchaseButton.interactable = false;
			purchaseButton.transform.Find("Text").GetComponent<Text>().text = "Not Enough";
			if (selectedUpgradeLevel < selectedUpgradeMaxLevel){
				detailCost.text = "Cost: " + selectedUpgradeCost;
				if (selectedUpgradeCost <= GameManager.instance.GetTotalCoins()){
					purchaseButton.interactable = true;
					purchaseButton.transform.Find("Text").GetComponent<Text>().text = "Purchase";
				}
			}
			else{
				detailCost.text = "";
			}
		}

	}


	private void UpdateTotalCoinAmount(){
		transform.Find("Amount").GetComponent<Text>().text = GameManager.instance.GetTotalCoins().ToString();
	}


	public void PurchaseUpgrade(){
		GameManager.instance.SpendTotalCoins(selectedUpgradeCost);
		UpdateTotalCoinAmount();

		PlayerUpgrader.IncreaseUpgradeLevel(selectedUpgrade.name);
		UpdateIconPanel();
		UpdateDetailPanel();
		myAudiosource.PlayOneShot(purchaseSound);
	}


	public void LoadMainMenu(){
		GameManager.instance.LoadMainMenu();
	}


	public void LoadGame(){
		GameManager.instance.LoadGame();
	}
}
