using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour {
	public float scrollSpeed;
	private Vector2 initialOffset;
	private Material background;
	private string textureName;

	// Use this for initialization
	void Start () {
		background = GetComponent<MeshRenderer>().material;
		textureName = background.mainTexture.name;
		initialOffset = background.GetTextureOffset("_MainTex");
	}
	
	// Update is called once per frame
	void Update () {
		float offsetY = Mathf.Repeat(Time.time * scrollSpeed, 1f);
		background.SetTextureOffset("_MainTex", new Vector2(initialOffset.x, initialOffset.y + offsetY));
	}
}
