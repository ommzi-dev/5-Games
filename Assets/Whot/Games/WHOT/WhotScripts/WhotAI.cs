using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class WhotAI : MonoBehaviour {

	public static WhotAI instance;

	public List<int> totalColoredCards =  new List<int>(5);
	string[] colors = {"cir","tri","cro","squ","sta" };
	// Use this for initialization
	void Start () {
		instance = this;
	}

	public void ThrowCard()
	{
		Invoke ("PlayWithDelay",2);
	}

	void PlayWithDelay()
	{

		string cardName = "";
		if(WhotConstants.aILevel==1)
			cardName = GetThrowableCard ();
		else if(WhotConstants.aILevel==0)
			cardName = GetEasyThrowableCard ();
		else if(WhotConstants.aILevel==2)
			cardName = GetHardThrowableCard ();
        //Debug.Log("cardName: " + cardName);
		if (cardName != "")
			WhotOpponent.instance.transform.Find (cardName).GetComponent<Button> ().onClick.Invoke ();//.ThrowCard (cardName);
		else
			WhotManager.instance.WithdrawCard ();

		if (cardName.Contains ("who")) {
			if(WhotConstants.aILevel==0)
				WhotManager.instance.WhotColorSelection (GetMostCommonColorcardOfOpponent());
			else
				WhotManager.instance.WhotColorSelection (GetMostCommonColorcard ());
		}
		//			Opponent.instance.WithDrawCardFromPile (WhotManager.instance.cardToPick);
	}

	public string GetMostCommonColorcard()
	{
		var cardList = WhotOpponent.instance.opponentCardIds;
		foreach (string id in cardList) {
			string color = id.Substring (0, 3);
			switch (color) {
			case "cir":
				totalColoredCards [0]++;
				break;
			case "tri":
				totalColoredCards [1]++;
				break;
			case "cro":
				totalColoredCards [2]++;
				break;
			case "squ":
				totalColoredCards [3]++;
				break;
			case "sta":
				totalColoredCards [4]++;
				break;
			}

		}
		return colors[totalColoredCards.IndexOf(totalColoredCards.Max())];
	}

	public string GetMostCommonColorcardOfOpponent()
	{
		var cardList = WhotPlayer.instance.playerCardIds;
		foreach (string id in cardList) {
			string color = id.Substring (0, 3);
			switch (color) {
			case "cir":
				totalColoredCards [0]++;
				break;
			case "tri":
				totalColoredCards [1]++;
				break;
			case "cro":
				totalColoredCards [2]++;
				break;
			case "squ":
				totalColoredCards [3]++;
				break;
			case "sta":
				totalColoredCards [4]++;
				break;
			}

		}
		return colors[totalColoredCards.IndexOf(totalColoredCards.Max())];
	}

	public string GetThrowableCard()
	{
		//Force player to pick from pile or throw the particular num
		string activeRule = WhotManager.instance.activeRule;
		if (!string.IsNullOrEmpty (activeRule)) {
			string topCardPlayed = WhotManager.instance.topCardPlayed;

			int lastCardIdNum = int.Parse (topCardPlayed.Substring (3, topCardPlayed.Length - 3));

			switch (activeRule) {
			case "PickTwo":
				if(WhotConstants.pickTwoDefend)
					return CheckCardNumberFromPlayerList (lastCardIdNum, WhotOpponent.instance.opponentCardIds);
				break;
			case "PickThree":
				if(WhotConstants.pickThreeDefend)
					return CheckCardNumberFromPlayerList (lastCardIdNum,  WhotOpponent.instance.opponentCardIds);
				break;
			default:
				return "";
			}
		} else 
		{
			var cardList = WhotOpponent.instance.opponentCardIds;
			// Check card Number from player's list
			string lastCardId = WhotManager.instance.topCardPlayed;

			foreach (string id in cardList) {
				int cardNumPlayed = int.Parse (id.Substring (3, id.Length - 3));
				int lastCardIdNum = int.Parse (lastCardId.Substring (3, lastCardId.Length - 3));

				if ((lastCardId.Contains ("who") && WhotManager.instance.colorSelectedAfterWhot.Equals (id.Substring (0, 3))) ||
					lastCardId.Contains (id.Substring (0, 3)) || lastCardIdNum == cardNumPlayed) {
					return id;
				}
			}
			if (cardList.Contains ("who20")) {
				return "who20";
			}
		}
		return "";
	}

	public string GetEasyThrowableCard()
	{
		//Force player to pick from pile or throw the particular num
		string activeRule = WhotManager.instance.activeRule;
		if (!string.IsNullOrEmpty (activeRule)) {
			string topCardPlayed = WhotManager.instance.topCardPlayed;
            int lastCardIdNum = int.Parse (topCardPlayed.Substring (3, topCardPlayed.Length - 3));
            
            switch (activeRule) {
			case "PickTwo":
				if(WhotConstants.pickTwoDefend)
					return CheckCardNumberFromPlayerList (lastCardIdNum, WhotOpponent.instance.opponentCardIds);
				break;
			case "PickThree":
				if(WhotConstants.pickThreeDefend)
					return CheckCardNumberFromPlayerList (lastCardIdNum,  WhotOpponent.instance.opponentCardIds);
				break;
			default:
				return "";
			}
		} else 
		{
			int playerTotalCard = WhotPlayer.instance.playerCardIds.Count;
			var cardList = WhotOpponent.instance.opponentCardIds;
			// Check card Number from player's list
			string lastCardId = WhotManager.instance.topCardPlayed;

			foreach (string id in cardList) {

				int cardNumPlayed = int.Parse (id.Substring (3, id.Length - 3));
				int lastCardIdNum = int.Parse (lastCardId.Substring (3, lastCardId.Length - 3));

				if ((lastCardId.Contains ("who") && WhotManager.instance.colorSelectedAfterWhot.Equals (id.Substring (0, 3))) ||
					lastCardId.Contains (id.Substring (0, 3)))// || lastCardIdNum == cardNumPlayed)
				{
					if (playerTotalCard == 1 && (id.Contains ("14") || (id.Contains ("2") && !WhotPlayer.instance.playerCardIds [0].Contains ("2")) ||
						(id.Contains ("5") && !WhotPlayer.instance.playerCardIds [0].Contains ("5")))) {
                        //Since its easy mode, AI won't try to exxtend game and let this opportunity pass.
					}
					else
						return id;
				}
			}
			foreach (string id in cardList) {

				int cardNumPlayed = int.Parse (id.Substring (3, id.Length - 3));
				int lastCardIdNum = int.Parse (lastCardId.Substring (3, lastCardId.Length - 3));

				if (lastCardIdNum == cardNumPlayed)
				{
					return id;
				}
			}
			if (cardList.Contains ("who20")) {
				return "who20";
			}
		}
		return "";
	}


	public string GetHardThrowableCard()
	{
		//Force player to pick from pile or throw the particular num
		string activeRule = WhotManager.instance.activeRule;
		if (!string.IsNullOrEmpty (activeRule)) {
			string topCardPlayed = WhotManager.instance.topCardPlayed;

			int lastCardIdNum = int.Parse (topCardPlayed.Substring (3, topCardPlayed.Length - 3));

			switch (activeRule) {
			case "PickTwo":
				if(WhotConstants.pickTwoDefend)
					return CheckCardNumberFromPlayerList (lastCardIdNum, WhotOpponent.instance.opponentCardIds);
				break;
			case "PickThree":
				if(WhotConstants.pickThreeDefend)
					return CheckCardNumberFromPlayerList (lastCardIdNum,  WhotOpponent.instance.opponentCardIds);
				break;
			default:
				return "";
			}

		} else 
		{
			int playerTotalCard = WhotPlayer.instance.playerCardIds.Count;
			var cardList = WhotOpponent.instance.opponentCardIds;
			// Check card Number from player's list
			string lastCardId = WhotManager.instance.topCardPlayed;

			if (playerTotalCard == 1) {
				// try to keep player in game

				string topCardColor = lastCardId.Substring (0, 3);

				foreach (string id in cardList) {

					int cardNumPlayed = int.Parse (id.Substring (3, id.Length - 3));
					int lastCardIdNum = int.Parse (lastCardId.Substring (3, lastCardId.Length - 3));
					if ((lastCardId.Contains ("who") && (
						id.Contains(WhotManager.instance.colorSelectedAfterWhot+14) || id.Contains(WhotManager.instance.colorSelectedAfterWhot+5)
						|| id.Contains(WhotManager.instance.colorSelectedAfterWhot+2)
					))) {
						return id;
					}
					if (id.Contains (topCardColor + 14) || id.Contains (topCardColor+ 5)
						|| id.Contains (topCardColor + 2)) {
						return id;
					}
				}
			}
			string specialCard = "";
			foreach (string id in cardList)
            {
				int cardNumPlayed = int.Parse (id.Substring (3, id.Length - 3)); 
				int lastCardIdNum = int.Parse (lastCardId.Substring (3, lastCardId.Length - 3));

				if ((lastCardId.Contains ("who") && WhotManager.instance.colorSelectedAfterWhot.Equals (id.Substring (0, 3))) ||
					lastCardId.Contains (id.Substring (0, 3)) || lastCardIdNum == cardNumPlayed) {
					if (id.Contains ("14") || id.Contains ("2") || id.Contains ("5")) {
						specialCard = id;
						continue;
					}
					else
						return id;
				}
			}
			if (!string.IsNullOrEmpty (specialCard))
				return specialCard;
			if (cardList.Contains ("who20")) {
				return "who20";
			}
		}
		return "";
	}

	string CheckCardNumberFromPlayerList(int cardNum,List<string> cardList)
	{
		// Check card Number from player's list
		foreach (string id in cardList) {

			if (id.Substring (3, id.Length - 3).CompareTo ("" + cardNum)==0)
				return id;
		}
		return "";
	}
}
