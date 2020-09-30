using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NumberSelectionFroRules : MonoBehaviour {

	public List<int>  allNumbers = new List<int>{1,2,3,4,5,7,8,10,11,12,13,14};
	public List<int>  elligibleNumbers = new List<int> {1,3,4,7,10,11,12,13,14};

	int totalcount = 10;
	int count = 0;

	public List<Text> numText;
	public List<Text> winText;
	public List<Text> defendText;
	// Use this for initialization
	void Start () {
		int susNum = PlayerPrefs.GetInt ("suspensionCardNum", 8);
		numText[0].text = ""+ susNum;
		allNumbers.Remove (susNum);

		int pickTwo = PlayerPrefs.GetInt ("pickTwoCardNum", 2);
		numText[1].text = ""+ pickTwo;
		allNumbers.Remove (pickTwo);

		int pickThree = PlayerPrefs.GetInt ("pickThreeCardNum", 5);
		numText[2].text = ""+ pickThree;
		allNumbers.Remove (pickThree);


		winText[0].text = PlayerPrefs.GetInt ("suspensionWin", 1) == 1?"YES":"NO";
		winText[1].text = PlayerPrefs.GetInt ("crownWin", 1) == 1?"YES":"NO";
		winText[2].text= PlayerPrefs.GetInt ("pickTwoWin", 1) == 1?"YES":"NO";
		winText[3].text = PlayerPrefs.GetInt ("pickThreeWin", 1) == 1?"YES":"NO";

		defendText[0].text = PlayerPrefs.GetInt ("pickTwoDefend", 1) == 1?"YES":"NO";
		defendText[1].text = PlayerPrefs.GetInt ("pickThreeDefend", 1) == 1?"YES":"NO";


		elligibleNumbers = allNumbers;

	}

	public void PlusClicked(Text num)
	{
		Debug.Log ("Current num is "+num.text);
		int displayedNum = int.Parse (num.text);
		elligibleNumbers.Add (displayedNum);
		elligibleNumbers.Sort ();
		int nextNum = GetNextIndexNumber(elligibleNumbers.IndexOf(displayedNum));
		Debug.Log ("Next num is "+nextNum);

		elligibleNumbers.Remove (nextNum);
		num.text = "" + nextNum;

	}
	int GetNextIndexNumber(int index)
	{
		index++;
		if (index > (totalcount - 1))
			index = 0;
		return elligibleNumbers [index];
	}

	int GetPreviousIndexNumber(int index)
	{
		index--;
		if (index < 0)
			index = 9;
		return elligibleNumbers [index];
	}
	public void MinusClicked(Text num)
	{
		int displayedNum = int.Parse (num.text);
		elligibleNumbers.Add (displayedNum);
		elligibleNumbers.Sort ();
		int nextNum = GetPreviousIndexNumber(elligibleNumbers.IndexOf(displayedNum));
		elligibleNumbers.Remove (nextNum);
		num.text = "" + nextNum;
	}
}
