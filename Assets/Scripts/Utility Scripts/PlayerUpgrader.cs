using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerUpgrader{
	public static void ClearUpgrades(){
		PlayerPrefs.SetInt("Launch", 0);
		PlayerPrefs.SetInt("Cooldown", 0);
		PlayerPrefs.SetInt("Magnet", 0);
		PlayerPrefs.SetInt("Charge", 0);
		PlayerPrefs.SetInt("Rotation", 0);
		PlayerPrefs.SetInt("Push", 0);
	}

	public static int GetUpgradeLevel(string upgradeName){
		return PlayerPrefs.GetInt(upgradeName);
	}


	// given the name of the upgrade, returns the description of the next level
	public static string GetUpgradeDescription(string upgradeName, int upgradeLevel){
		switch(upgradeName){
			case "Launch":
				switch(upgradeLevel){
					case 0:
						return "Level 1: Increase speed when launching off planets";
					case 1:
						return "Level 2: Greatly increase speed when launching off planets";
					//case 2:
						//return "Level 3: Launch off planets faster than you can say 'boom'";
					default:
						return "Fully upgraded";
				}
			case "Cooldown":
				switch(upgradeLevel){
					case 0:
						return "Level 1: Reduce the charging time of tractor beam";
					default:
						return "Fully upgraded";
				}
			case "Magnet":
				switch(upgradeLevel){
					case 0:
						return "Level 1: Increase the coin attraction radius";
					case 1:
						return "Level 2: Further increase the coin attraction radius";
					default:
						return "Fully upgraded";
				}
			case "Charge":
				switch(upgradeLevel){
					case 0:
						return "Level 1: Reduce the time it takes to charge up a jump";
					case 1:
						return "Level 2: Further reduce the time it takes to charge up a jump";
					default:
						return "Fully upgraded";
				}
			case "Rotation":
				switch(upgradeLevel){
					case 0:
						return "Level 1: Increase the rotation speed of the home planet";
					case 1:
						return "Level 2: Further increase the rotation speed of the home planet";
					default:
						return "Fully upgraded";
				}
			case "Push":
				switch(upgradeLevel){
					case 0:
						return "Level 1: Push planets away harder when launching off";
					case 1:
						return "Level 2: Push planets away super harder when launching off";
					default:
						return "Fully upgraded";
				}
			default:
				return "harder, better, faster, stronger";
		}
	}


	// given the name of the upgrade, returns the cost of the next level
	public static int GetUpgradeCost(string upgradeName, int upgradeLevel){
		switch(upgradeName){
			case "Launch":
				switch(upgradeLevel){
					case 0:
						return 250;
					case 1:
						return 450;
					default:
						return 0;
				}
			case "Cooldown":
				switch(upgradeLevel){
					case 0:
						return 450;
					default:
						return 0;
				}
			case "Magnet":
				switch(upgradeLevel){
					case 0:
						return 300;
					case 1:
						return 500;
					default:
						return 0;
				}
			case "Charge":
				switch(upgradeLevel){
					case 0:
						return 400;
					case 1:
						return 600;
					default:
						return 0;
				}
			case "Rotation":
				switch(upgradeLevel){
					case 0:
						return 500;
					case 1:
						return 750;
					default:
						return 0;
				}
			case "Push":
				switch(upgradeLevel){
					case 0:
						return 400;
					case 1:
						return 600;
					default:
						return 0;
				}
			default:
				switch(upgradeLevel){
					case 0:
						return 250;
					case 1:
						return 450;
					default:
						return 0;
				}
		}
	}


	public static void IncreaseUpgradeLevel(string upgradeName){
		int newLevel = PlayerPrefs.GetInt(upgradeName) + 1;
		PlayerPrefs.SetInt(upgradeName, newLevel);
	}


	// changes player object's stats based on upgrade level
	public static void ApplyPlayerUpgrades(ref PlayerScript player){
		switch(PlayerPrefs.GetInt("Launch")){
			default:
				player.maxChargeSpeed = 14f;
				break;
			case 0:
				player.maxChargeSpeed = 14f;
				break;
			case 1: 
				player.maxChargeSpeed = 17f;
				break;
			case 2: 
				player.maxChargeSpeed = 20f;
				break;
		}

		switch(PlayerPrefs.GetInt("Cooldown")){
			default:
				//player.actionChargesMax = 1;
				break;
			case 0: 
				//player.actionChargesMax = 1;
				break;
			case 1:
				//player.actionChargesMax = 2f;
				break;
		}
		switch(PlayerPrefs.GetInt("Magnet")){
			default:
				player.coinAttractionRadius = 5f;
				player.coinAttractionSpeed = 12f;
				break;
			case 0: 
				player.coinAttractionRadius = 5f;
				player.coinAttractionSpeed = 12f;
				break;
			case 1:
				player.coinAttractionRadius = 8f;
				player.coinAttractionSpeed = 16f;
				break;
			case 2:
				player.coinAttractionRadius = 11f;
				player.coinAttractionSpeed = 20f;
				break;
		}
		switch(PlayerPrefs.GetInt("Charge")){
			default:
				player.maxChargeTime = 1.5f;
				break;
			case 0: 
				player.maxChargeTime = 1.5f;
				break;
			case 1:
				player.maxChargeTime = 1f;
				break;
			case 2:
				player.maxChargeTime = 1f;
				break;
		}
		switch(PlayerPrefs.GetInt("Rotation")){
			default:
				player.homePlanetRotationSpeed = 60f;
				break;
			case 0: 
				player.homePlanetRotationSpeed = 60f;
				break;
			case 1:
				player.homePlanetRotationSpeed = 90f;
				break;
			case 2:
				player.homePlanetRotationSpeed = 120f;
				break;
		}
		switch(PlayerPrefs.GetInt("Push")){
			default:
				player.planetPushPower = 1f;
				break;
			case 0: 
				player.planetPushPower = 1f;
				break;
			case 1:
				player.planetPushPower = 1.5f;
				break;
			case 2:
				player.planetPushPower = 2f;
				break;
		}
	}
}
