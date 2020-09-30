using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhotOpponent : WhotUserManager {

	public static WhotOpponent instance;

    public string userId, userName, score;
    public List<RawImage> userPic;
	public List<string> opponentCardIds = new List<string>();
    public int sumOfCards;


    void OnDestroy()
	{
		instance = null;
	}
	void Start()
	{
		instance = this;
        if (UserDetailsManager.isAdminPlayer)
            sumText.gameObject.SetActive(true);
        else
            sumText.gameObject.SetActive(false);
	}

    public void UpdateOpponentImage(Texture tex)
    {
        for(int i=0; i< userPic.Count; i++)
        {
            userPic[i].texture = tex;
        }
    }

    public void WithDrawCard(int noOfCards)
	{
        //Debug.Log("WithdrawCard : "+ noOfCards);
        SoundManger.instance.PlaySound ((int)(ListOfSounds.Draw));
        if (WhotManager.instance.pileCardIds.Count < noOfCards)
        {
            opponentCardIds.InsertRange(0, WithDrawCardFromPile(WhotManager.instance.pileCardIds.Count, 2));
            WhotManager.instance.AddCardsInPileFromPlayedCardPile();
        }
        else
            opponentCardIds.InsertRange(0,WithDrawCardFromPile (noOfCards,2));
        //UpdateCardTumb ();            //needs to be commented
    }

    public void UpdateCardsLeftNumber()
    {
        if (opponentCardIds.Count == 0)
            noOfCards.SetActive(false);
        else
            noOfCards.transform.GetChild(0).GetComponent<Text>().text = "" + opponentCardIds.Count;
    }

    public void UpdateCardTumb()
	{
        Debug.Log("Update Card Tumb Called from Opponent");
        // Debug.Log("Is Admin Player: "+ UserDetailsManager.isAdminPlayer);
        sumOfCards = UpdateCardTumb(opponentCardIds, UserDetailsManager.isAdminPlayer);
        UpdateCardsLeftNumber();
        sumText.text = "Sum:" + sumOfCards;
        if (opponentCardIds.Count==1)
			SoundManger.instance.PlaySound ((int)(ListOfSounds.LastCard));
	}
}
