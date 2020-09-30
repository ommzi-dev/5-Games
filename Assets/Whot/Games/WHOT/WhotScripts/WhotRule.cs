using UnityEngine;


public class WhotRule : MonoBehaviour {

	public static WhotRule instance;
	public string[] ruleNames = {"HoldOn","PickTwo","PickThree","Suspension","GeneralMarket"};
	
	public bool IsCardThrowable(string cardId)
	{
		string lastCardId = WhotManager.instance.topCardPlayed;
		int cardNumPlayed = int.Parse(cardId.Substring (3, cardId.Length - 3));
		int lastCardIdNum = int.Parse(lastCardId.Substring (3, lastCardId.Length - 3));
		string ruleApplied = WhotManager.instance.activeRule;

		if (!string.IsNullOrEmpty (ruleApplied) && (ruleApplied == "PickTwo" || ruleApplied == "PickThree" || ruleApplied == "GeneralMarket" ))
        {			
			int cardNumCanBePlayed = 0;

            switch (ruleApplied)
            {
                case "PickTwo":
                    if (WhotConstants.pickTwoDefend)
                        cardNumCanBePlayed = WhotConstants.pickTwoCardNum;
                    break;
                case "PickThree":
                    if (WhotConstants.pickThreeDefend)
                        cardNumCanBePlayed = WhotConstants.pickThreeCardNum;
                    break;
            }
            if (cardNumCanBePlayed != 0 && cardNumPlayed == cardNumCanBePlayed)
				return true;
			else
				return false;				
		} 
		else
		{
			if ((lastCardId.Contains ("who") && WhotManager.instance.colorSelectedAfterWhot.Equals (cardId.Substring (0, 3))) ||
			   lastCardId.Contains (cardId.Substring (0, 3)) || lastCardIdNum == cardNumPlayed || cardId.Contains ("who"))
            {
				return true;
			}
            else
				return false;
		}
	}

	public void CheckImpact(string cardId)
	{
		int cardNumPlayed = int.Parse(cardId.Substring (3, cardId.Length - 3));
		string activeRule = "";
		if (cardNumPlayed == 20) {
		}
		else if(cardNumPlayed == 1)
		{
			activeRule = ruleNames [0];
			SoundManger.instance.PlaySound ((int)(ListOfSounds.HoldOn));
			// Hold on rule applied: next player turn skipped
		}
		else if(cardNumPlayed == WhotConstants.suspensionCardNum)
		{
			activeRule = ruleNames [3];
			SoundManger.instance.PlaySound ((int)(ListOfSounds.Suspension));
			// Suspension rule applied: Skip all players turn
		}
		else if(cardNumPlayed == WhotConstants.pickTwoCardNum)
		{
            if (PlayerPrefs.GetInt("PickTwo") == 0)     // If this rule is not active at the moment then value is 0.
            {
                PlayerPrefs.SetInt("PickTwo", 1);       // value is 1, when rule is active.
                activeRule = ruleNames[1];
                SoundManger.instance.PlaySound((int)(ListOfSounds.PickTwo));
                // Pick two rule applied: next player play same number or draw two cards from pile   
            }
            else
            {
                PlayerPrefs.SetInt("PickTwo", 0);
            }
		}
		else if(cardNumPlayed == WhotConstants.pickThreeCardNum)
		{
            if (PlayerPrefs.GetInt("PickThree") == 0)     // If this rule is not active at the moment then value is 0.
            {
                PlayerPrefs.SetInt("PickThree", 1);       // value is 1, when rule is active.
                activeRule = ruleNames[2];
                SoundManger.instance.PlaySound((int)(ListOfSounds.PickThree));
                // Pick Three rule applied: next player play same number or draw three cards from pile
            }
            else
            {
                PlayerPrefs.SetInt("PickThree", 0);
            }
        }
		else if(cardNumPlayed == 14)
		{
			activeRule = ruleNames [4];
			SoundManger.instance.PlaySound ((int)(ListOfSounds.GeneralMarket));
			// GENERAL MARKET rule applied: All players must draw 1 card from pile.
		}
		WhotManager.instance.activeRule = activeRule;
	}
}
