using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitScript : MonoBehaviour {
	public float rotationDegPerSec;
	public bool clockwise;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (clockwise){
			transform.Rotate (0, 0, Time.deltaTime * rotationDegPerSec * -1);
		}
		else{
			transform.Rotate (0, 0, Time.deltaTime * rotationDegPerSec);
		}

	}
}
