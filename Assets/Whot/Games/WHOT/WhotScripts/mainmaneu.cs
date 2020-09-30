using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mainmaneu : MonoBehaviour {

	public GameObject quitpopup;
	public Button wagermodeBtn;
	public AudioSource bgsound;
	// Use this for initialization
	void Start () {

		if (string.Equals(PlayerPrefs.GetString ("Music", "true"),"true"))	
		{
			bgsound.Play ();
		}

	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown (KeyCode.Escape)) {
			quitpopup.SetActive (true);
		}
	}
}
