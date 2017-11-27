using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the set up and charging animations of the jetpack bar, which shows how many charges of the jetpack is left.
/// </summary>
public class JetpackBar : MonoBehaviour {
	public GameObject barObject;
	private RectTransform[] bars;

	[SerializeField]private int chargeAmount;
	[SerializeField]private int currentChargingIndex; // index of the bar that is currently being charged
	private float chargeTime;

	// Use this for initialization
	void Awake () {
		SetupBars(1, 5f);
	}

	void Start(){
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	// sets up and positions the bars based on number of charges
	public void SetupBars(int amount, float time){
		chargeAmount = amount;
		chargeTime = time;
		currentChargingIndex = 0;

		// first set up the bars array and populate it
		bars = new RectTransform[chargeAmount];
		bars[0] = barObject.GetComponent<RectTransform>();
		barObject.GetComponent<Slider>().maxValue = chargeTime;
		for (int i = 1; i < chargeAmount; i++){
			GameObject newBar = Instantiate(barObject, barObject.transform.parent);
			newBar.transform.SetAsFirstSibling();
			newBar.GetComponent<Slider>().maxValue = chargeTime;
			bars[i] = newBar.GetComponent<RectTransform>();;
		}

		if (chargeAmount == 1){
			bars[0].sizeDelta = new Vector2(800, 100);
			bars[0].localPosition = Vector3.zero;
		}
		else{
			float width = 800f / chargeAmount;
			for (int i = 0; i < chargeAmount; i++){
				bars[i].sizeDelta = new Vector2(width, 100f);
				bars[i].localPosition = new Vector3(width * ((float)i - ((float)chargeAmount - 1f) / 2f) + 10f * i, 0, 0);
			}
		}
	}



	public void UpdateChargeBar(float amount){
		if (currentChargingIndex < chargeAmount){
			bars[currentChargingIndex].GetComponent<Slider>().value = amount;
			if (amount > chargeTime){
				bars[currentChargingIndex].GetComponent<Animator>().SetTrigger("Charged");
				currentChargingIndex += 1;
			}
		}
	}


	public void DepleteCurrentChargeBar(){
		if (chargeAmount == 1){
			if (currentChargingIndex == 1){
				currentChargingIndex = 0;
				bars[0].GetComponent<Slider>().value = 0f;
			}
			else{
				Debug.Log("Error: not enough charges to use for jetpack");
			}
		}
		else if (chargeAmount > 1){
			if (currentChargingIndex == chargeAmount){
				currentChargingIndex -= 1;
				bars[currentChargingIndex].GetComponent<Slider>().value = 0f;
			}
			else if (currentChargingIndex > 0){
				bars[currentChargingIndex - 1].GetComponent<Slider>().value = bars[currentChargingIndex].GetComponent<Slider>().value;
				bars[currentChargingIndex].GetComponent<Slider>().value = 0f;
				currentChargingIndex -= 1;
			}
			else{
				Debug.Log("Error: not enough charges to use for jetpack");
			}
		}
	}
}
