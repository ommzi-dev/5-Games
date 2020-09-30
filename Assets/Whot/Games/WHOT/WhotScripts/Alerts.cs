using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

enum AlertMsgName{

	GoMarket = 0,
	Pick1Card,
	Pick2Card,
	Pick3Card,
	WrongCardAlert,
	NoWinAlert,



}

public class Alerts : MonoBehaviour {
	public GameObject alertPanel;
	public Text alert;

	List<string> alertMsgs = new List<string>();
	Vector3 initialTrans;
	// Use this for initialization
	void Start () {
		initialTrans = transform.localPosition;
		alertMsgs.Add ("Take One ");
		alertMsgs.Add ("Pick One");
		alertMsgs.Add ("Pick Two");
		alertMsgs.Add ("Pick Three");
		alertMsgs.Add ("Card can either be a WHOT or of same type or number as the card on played deck.");
		alertMsgs.Add ("You cannot win with this card");
		alertPanel.GetComponent<DOTweenAnimation> ().DOPlayForward ();
		Invoke ("HideAlert", 0.1f);
//		HideAlert ();
	}


	public void ShowAlert(int index)
	{
        //Debug.Log ("index of alert is "+gameObject.activeInHierarchy);
        SoundManager.Instance.PlaySound(5);
		if (gameObject.activeInHierarchy || !WhotConstants.isAlert)
			return;
		gameObject.SetActive (true);
		alert.text = alertMsgs[index];
		alertPanel.GetComponent<DOTweenAnimation> ().DORestart ();
        
	}

	public void HideAlert()
	{
		transform.localPosition = initialTrans;
		alertPanel.GetComponent<DOTweenAnimation> ().DOComplete ();
        
        gameObject.SetActive (false);
	}

}
