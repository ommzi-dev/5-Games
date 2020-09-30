using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundcontroller : MonoBehaviour {
	public AudioSource win, lose, draw, popup;
	public static soundcontroller instance;
	void Awake()
	{
		if(instance==null)	
			instance = this;
	}
	void Start()
	{
        //Debug.Log("Sound Controller:" + gameObject.name);
	}
	public void PlayAudio(AudioSource audio)
	{
		if (string.Equals( PlayerPrefs.GetString (WhotCommonConstants.SoundKey, WhotCommonConstants.soundOn),WhotCommonConstants.soundOn)) {

			audio.Play ();
		}
	}
}
