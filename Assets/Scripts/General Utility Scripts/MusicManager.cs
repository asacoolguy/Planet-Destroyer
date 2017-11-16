using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {
	public AudioClip mainMenuMusic, gameMusic, upgradeMusic;
	private AudioSource myAudioSource;

	private static MusicManager _instance;

	public static MusicManager instance{
		get{
			if (_instance == null){
				_instance = GameObject.FindObjectOfType<MusicManager>();
				DontDestroyOnLoad(_instance.gameObject);
			}

			return _instance;
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

		myAudioSource = GetComponent<AudioSource>();
	}

	public void Play(){
		GetComponent<AudioSource> ().Play ();
	}

	public void PlayMainMenuMusic(){
		myAudioSource.clip = mainMenuMusic;
		myAudioSource.Play();
	}


	public void PlayGameMusic(){
		if (myAudioSource.clip != gameMusic || !myAudioSource.isPlaying){
			myAudioSource.clip = gameMusic;
			myAudioSource.Play();
		}
	}


	public void PlayUpgradeMusic(){
		if (myAudioSource.clip != upgradeMusic || !myAudioSource.isPlaying){
			myAudioSource.clip = upgradeMusic;
			myAudioSource.Play();
		}
	}

}
