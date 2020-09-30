using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oreint : MonoBehaviour {

	public static oreint instance;
	public GameObject orient;
	void Awake()
	{
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
//			DontDestroyOnLoad (orient);
//			DontDestroyOnLoad (this);
		} 
	}
	public void OnClickWhot()
	{
		//oreint.instance.orient.SetActive (true);
		Screen.orientation = ScreenOrientation.Landscape;

		InvokeRepeating ("whotorientdelay",0.1f,0.1f);
	}
	void  whotorientdelay()
	{
		if (Screen.orientation == ScreenOrientation.Landscape) 
		{
			Screen.orientation = ScreenOrientation.Landscape;
			CancelInvoke ();
			StartCoroutine (delay());
		}

	}
	void  whotorientdelay1()
	{
		if (Screen.orientation == ScreenOrientation.Landscape) 
		{
			Screen.orientation = ScreenOrientation.Landscape;
			CancelInvoke ();
			StartCoroutine (delay());

		}

	}
	public void OnClickExitWhot()
	{
		oreint.instance.orient.SetActive (true);
		Screen.orientation = ScreenOrientation.Landscape;
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginSplash");
		InvokeRepeating ("whotorientdelay1",0.1f,0.1f);
	}
	IEnumerator delay()
	{
		yield return new WaitForSeconds (2);
		oreint.instance.orient.SetActive (false);
	}
}
