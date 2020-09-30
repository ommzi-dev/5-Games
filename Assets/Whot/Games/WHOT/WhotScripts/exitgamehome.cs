using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exitgamehome : MonoBehaviour {

	void Start()
	{
        SoundManager.Instance.PlaySound(5);
    }
	// Use this for initialization
	public void OnClickYes()
	{
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginSplash");
		Screen.orientation = ScreenOrientation.Landscape;
		PlayerPrefs.SetInt ("Exit",1);
		
	}
	public void onClickNo()
	{
		gameObject.SetActive (false);
	}
}
