using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class slectwagerfrndly : MonoBehaviour {

	public GameObject wageramnt, amnthg,frndly, wagered, listt, warning_text;
	public Text textdisp;
	public Button playbtn;

	void Start()
	{
		WHOTMultiplayerManager.wagerAmount = 1;

	}
	void OnEnable()
	{
		//warning_text.SetActive (false);
		//playbtn.interactable = true;
		/*frndly.SetActive (true);      //Don't need selection options as it is only based on bet selection #RK
		wagered.SetActive (false);    
		wageramnt.SetActive (false);*/
		//listt.SetActive (false);
	}
	public void OnFriendlypressed()
	{
		warning_text.SetActive (false);
		playbtn.interactable = true;

		WHOTMultiplayerManager.playingWagered = false;
		
		frndly.SetActive (true);
		wagered.SetActive (false);

		wageramnt.SetActive (false);
		listt.SetActive (false);
	}
	public void OnWageredpressed()
	{
//		if (!AlertController.instance.isWagerModeEnable) {
//			warning_text.SetActive (true);
//			playbtn.interactable = false;
//
//		} else {
			WHOTMultiplayerManager.playingWagered = true;

			frndly.SetActive (false);
			wagered.SetActive (true);

			wageramnt.SetActive (true);
			playbtn.interactable = true;
			warning_text.SetActive (false);
		//}
	}
	public void OnClickAmnt(GameObject g)
	{
		//amnthg.transform.position = new Vector3 (g.transform.position.x, amnthg.transform.position.y,amnthg.transform.position.z);
		textdisp.text = g.GetComponent<Text> ().text;
		WHOTMultiplayerManager.wagerAmount = int.Parse (textdisp.text);
	}
}
