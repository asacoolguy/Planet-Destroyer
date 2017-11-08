using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {
	private static MusicManager _instance;

	public static MusicManager instance{
		get{
			if (_instance == null){
				_instance = GameObject.FindObjectOfType<MusicManager>();
				DontDestroyOnLoad(_instance.gameObject);
			}

			return instance;
		}
	}

	void Awake(){
		if (_instance == null){
			_instance = this;
			DontDestroyOnLoad(this);
		}
		else{
			if (this != _instance)
				Destroy(this.gameObject);
		}
	}

	public void Play(){
		GetComponent<AudioSource> ().Play ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
