using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class quitfromgameplay : MonoBehaviour {

	public GameObject quitpopup, splashscreen;
	public Text username;
	public GameObject userprofile;
	// Use this for initialization
	void Start () {
		SoundManager.Instance.PlaySound (5);
		#if UNITY_WEBGL
		//webgl
		CommonConstants.AssignTexture(userprofile.GetComponent<RawImage>(),AlertController.instance.profileimg);

		username.text = WebServices.instance.currentUser.firstname;


		#else

		//WhotCommonConstants.AssignTexture(userprofile.GetComponent<RawImage>(),AlertController.instance.profileimg);
		#endif
	}
	void OnEnable()
	{
		splashscreen.SetActive (false);	
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.Escape)) {
			quitpopup.SetActive (true);
		}
	}
}
