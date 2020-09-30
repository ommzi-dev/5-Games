using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhotConstants : MonoBehaviour {

	public GameObject menu, gamePlay;
	public AddResponsiveness responsive;
	public static bool loadGameplay = false;


	public static int initialCards = 6;
	public static int aILevel = 1;
	public static bool isAI = true;
	public static Dictionary<string,string> colorFolder = new Dictionary<string, string>();
	public static bool isHint=false, isAlert=true, isHoldOn = false;

	//OptionInfo

	public static int suspensionCardNum = 8;
	public static bool suspensionWin = true;

	public static bool crownWin = true;

	public static int pickTwoCardNum = 2;
	public static bool pickTwoWin = true;
	public static bool pickTwoDefend = true;


	public static int pickThreeCardNum =5;
	public static bool pickThreeWin = true;
	public static bool pickThreeDefend = true;

	void Start()
	{
		if (loadGameplay) {
			menu.SetActive (false);
			gamePlay.SetActive (true);
			WhotManager.instance.OnStart ();
			responsive.enabled = true;
			loadGameplay = false;
		}
	}

	public void SetsuspenstionCardNum(Text cardNum)
	{
		suspensionCardNum = int.Parse (cardNum.text);
		PlayerPrefs.SetInt("suspensionCardNum",suspensionCardNum);
	}
	public void SetsuspensionWin(Text cardNum)
	{
				suspensionWin = ConvertYesOrNoToBool (cardNum.text);
		PlayerPrefs.SetInt("suspensionWin",suspensionWin==true?1:0);
	}

	public void SetcrownWin(Text cardNum)
	{
				crownWin = ConvertYesOrNoToBool (cardNum.text);
		PlayerPrefs.SetInt("crownWin",crownWin==true?1:0);

	}

	public void SetpickTwoCardNum(Text cardNum)
	{
		pickTwoCardNum = int.Parse (cardNum.text);
		PlayerPrefs.SetInt("pickTwoCardNum",pickTwoCardNum);
	}
	public void SetpickTwoWin(Text cardNum)
	{
				pickTwoWin = ConvertYesOrNoToBool (cardNum.text);
		PlayerPrefs.SetInt("pickTwoWin",pickTwoWin==true?1:0);

	}
	public void SetpickTwoDefend(Text cardNum)
	{
        //pickTwoDefend = ConvertYesOrNoToBool (cardNum.text);      //Commented on 17thFeb; Removed defending for pick2.
        pickTwoDefend = false;
		PlayerPrefs.SetInt("pickTwoDefend",pickTwoDefend==true?1:0);

	}


	public void SetpickThreeCardNum(Text cardNum)
	{
		pickThreeCardNum = int.Parse (cardNum.text);
		PlayerPrefs.SetInt("pickThreeCardNum",pickThreeCardNum);
	}
	public void SetpickThreeWin(Text cardNum)
	{
				pickThreeWin = ConvertYesOrNoToBool(cardNum.text);
		PlayerPrefs.SetInt("pickThreeWin",pickThreeWin==true?1:0);

	}
	public void SetpickThreeDefend(Text cardNum)
	{
		pickThreeDefend = ConvertYesOrNoToBool(cardNum.text);
		PlayerPrefs.SetInt("pickThreeDefend",pickThreeDefend==true?1:0);
		PlayerPrefs.Save ();
	}

	bool ConvertYesOrNoToBool(string input)
	{
		if (input.Contains ("Y")) {
			return true;	
		} else
			return false;
	}

	public static GameObject CreateDuplicate(GameObject tumb)
	{
		GameObject go = Instantiate (tumb) as GameObject;
		go.transform.SetParent (tumb.transform.parent);
		go.SetActive (true);



		go.transform.localScale = Vector3.one;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.Euler (Vector3.zero);
		var rect = go.GetComponent<RectTransform> ();
		if(rect)
		{
			rect.anchoredPosition = Vector2.zero;
			rect.sizeDelta = Vector2.zero;
		}
		return go;
	}

	public static void SetColorFolderDic()
	{
		if (colorFolder.Count > 0)
			return;

		if (!colorFolder.ContainsKey ("cri")) {
			colorFolder.Add ("cir","Circle");
		}

		if (!colorFolder.ContainsKey ("tri")) {
			colorFolder.Add ("tri","Triangle");
		}

		if (!colorFolder.ContainsKey ("cro")) {
			colorFolder.Add ("cro","Crosses");
		}

		if (!colorFolder.ContainsKey ("squ")) {
			colorFolder.Add ("squ","Square");
		}

		if (!colorFolder.ContainsKey ("sta")) {
			colorFolder.Add ("sta","Star");
		}

        if (!colorFolder.ContainsKey ("who")) {
			colorFolder.Add ("who","Whot");
		}

		suspensionCardNum = PlayerPrefs.GetInt ("suspensionCardNum", 8);
		suspensionWin = (PlayerPrefs.GetInt ("suspensionWin", 1) == 1?true:false);

		crownWin = (PlayerPrefs.GetInt ("crownWin", 1) == 1?true:false);

		pickTwoCardNum = PlayerPrefs.GetInt ("pickTwoCardNum", 2);
		pickTwoWin = (PlayerPrefs.GetInt ("pickTwoWin", 1) == 1?true:false);
       // pickTwoDefend = (PlayerPrefs.GetInt ("pickTwoDefend", 1) == 1?true:false);        
        pickTwoDefend = false;
		pickThreeCardNum = PlayerPrefs.GetInt ("pickThreeCardNum", 5);
		pickThreeWin = (PlayerPrefs.GetInt ("pickThreeWin", 1) == 1?true:false);
		pickThreeDefend = (PlayerPrefs.GetInt ("pickThreeDefend", 1) == 1?true:false);
	}

	public static string GetColorFolder(string shortName)
	{
		return colorFolder[shortName];
	}

	public void EnableHint(bool enable)
	{
		isHint = enable;
	}

	public void EnableAlert(bool enable)
	{
		isAlert = enable;
	}

	public void EnableHoldOn(bool enable)
	{
		isHoldOn = enable;
	}


	public void SetAILevel(int level)
	{
		aILevel = level;
	}
}

