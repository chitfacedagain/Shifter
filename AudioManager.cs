using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public AudioClip musicTrack;

	public AudioSource musicSource;
	public AudioSource sfxSource;

	void Start () {
		musicSource = GetComponent<AudioSource> ();
		PlaySource ();
	}
	
	void PlaySource(){
		musicSource.Play ();
		Invoke ("PlaySource", musicSource.clip.length);
	}


}
