using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class WhotPlayer : WhotUserManager {
	public static WhotPlayer instance;

    public List<string> playerCardIds = new List<string> ();
	public CardTumb hintAnimcardname = null;
	public Text cardsLeft;
    public bool isAdmin = UserDetailsManager.isAdminPlayer;
	//public ScrollSnap scroll;
	public ScrollRect scrollRect;
    public string userId, userName, score;
    public int sumOfCards;

    void OnDestroy()
	{
		instance = null;
	}
	void Start()
	{
		instance = this;
        userId = UserDetailsManager.userId;
        userName = UserDetailsManager.userName;
        //Debug.Log("Player: " + gameObject.name);
	}

    public void WithDrawCard(int noOfCards)
	{
		SoundManger.instance.PlaySound ((int)(ListOfSounds.Draw));
        Debug.Log("Withdraw for player");
        if (WhotManager.instance.pileCardIds.Count< noOfCards)
        {
            playerCardIds.InsertRange(0, WithDrawCardFromPile(WhotManager.instance.pileCardIds.Count, 1));
            //WhotManager.instance.AddCardsInPileFromPlayedCardPile();
        }
        else
            playerCardIds.InsertRange(0, WithDrawCardFromPile(noOfCards, 1));
        //UpdateCardTumb ();            //needs to be commented
    }

    public void UpdateCardTumb()
	{
        Debug.Log("Update Card Tumb Called from Player");
        if (hintAnimcardname != null) {
			hintAnimcardname.StopHintAnim ();
		}
        //for(int r=0; r< playerCardIds.Count; r++)
        //    Debug.Log("Player card Id: " + playerCardIds[r]);
        sumOfCards = UpdateCardTumb(playerCardIds,true);       //Sets the card and update the card face image.
        sumText.text = "Sum:" + sumOfCards;
        //scroll.enabled = false;
		//scrollRect.enabled = false;
		Invoke ("HandleScroll", 0.05f);             //Enables scroll if number of cards are more than 5.
	}

	public void UpdateCardLeftNumber()
	{
		cardsLeft.text = "" + playerCardIds.Count;
	}

	void HandleScroll()
	{
		int totalCards = playerCardIds.Count;
		cardsLeft.text = "" + totalCards;
		if(totalCards==1)
			SoundManger.instance.PlaySound ((int)(ListOfSounds.LastCard));

		if(totalCards==0)
			noOfCards.SetActive (false);
		else
			noOfCards.SetActive (true);
		//scrollRect.enabled = true;
		//scroll.enabled = (totalCards <=5)?false:true;
	}

	public void HighlightCard(string cardId)
	{
		if (hintAnimcardname != null) {
			hintAnimcardname.StopHintAnim ();
		}
		hintAnimcardname = transform.Find (cardId).GetComponent<CardTumb> ();
		hintAnimcardname.PlayHintAnim ();
	}

    public void ShowLeftCard()
    {
        float scrollValue = 10f / float.Parse(playerCardIds.Count.ToString());
        Debug.Log("Scroll Rect Current Value: "+ scrollRect.horizontalNormalizedPosition);
        if (scrollRect.horizontalNormalizedPosition > 0f)
        {
            float newScrollPos = scrollRect.horizontalNormalizedPosition - scrollValue;
            Debug.Log("New Scroll Value: " + newScrollPos);
            if (newScrollPos > 0)
                scrollRect.horizontalNormalizedPosition = newScrollPos;
            else
                scrollRect.horizontalNormalizedPosition = 0f;
        }
    }

    public void ShowRightCard()
    {
        float scrollValue = 10f / float.Parse(playerCardIds.Count.ToString());
        Debug.Log("Scroll Rect Current Value: " + scrollRect.horizontalNormalizedPosition);
        if (scrollRect.horizontalNormalizedPosition < 1f)
        {
            float newScrollPos = scrollRect.horizontalNormalizedPosition + scrollValue;
            Debug.Log("New Scroll Value: " + newScrollPos);
            if (newScrollPos < 1)
                scrollRect.horizontalNormalizedPosition = newScrollPos;
            else
                scrollRect.horizontalNormalizedPosition = 1f;
        }
    }
}
