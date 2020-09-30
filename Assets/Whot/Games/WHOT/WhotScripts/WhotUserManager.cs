using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// User manger.
/// </summary>
public class WhotUserManager : MonoBehaviour {

	int pickTwoRuleCardCount = 2;
	int pickThreeRuleCardCount = 3;
	public WhotRule rule;
	public CardTumb tumb,selectedCard;
    public GameObject gameplayCanvas;
	public GameObject noOfCards;
    public Text sumText;

    public Material defaultMaterial;
	// Use this for initialization
	void Start () {
        Debug.Log("UserManager: " + gameObject.name);
        gameplayCanvas = GameObject.FindGameObjectWithTag("Canvas");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// Updates the displayed cards.
	/// </summary>
	/// <param name="userCards">User cards.</param>
	public int UpdateCardTumb( List<string> userCards , bool showCard)
	{
		Transform parent = tumb.transform.parent;
        int sumOfCards = 0;
		for(int i = 1; i<parent.childCount;i++)
			Destroy(parent.GetChild(i).gameObject);
		foreach (string id in userCards)
        {
			CardTumb card = (WhotConstants.CreateDuplicate (tumb.gameObject)).GetComponent<CardTumb>();
			card.name = id;
			card.SetCardName(id,showCard);
            int cardNumPlayed = int.Parse(id.Substring(3, id.Length - 3));
            sumOfCards += cardNumPlayed;
        }
        return sumOfCards;
	}

	/// <summary>
	/// Withdraws card from pile.
	/// </summary>
	/// <param name="numberOfCards">Number of cards.</param>
	/// 
	public virtual List<string> WithDrawCardFromPile(int numberOfCards,int type)
	{
       // Debug.Log("WithDrawCardFromPile: " + numberOfCards + " Pile cards: "+ WhotManager.instance.pileCardIds.Count);
        return UpdatePileAndPlayerList (numberOfCards,type);
	}

	List<string> UpdatePileAndPlayerList(int cardCount,int type)
	{
        Debug.Log("UpdatePileAndPlayerList: " + cardCount+ " type: "+ type);
        WhotManager.instance.activeRule = "";
		if (WhotManager.instance.pileCardIds.Count < cardCount)
			WhotManager.instance.AddCardsInPileFromPlayedCardPile ();
		List<string> topNCards = WhotManager.instance.pileCardIds.GetRange (0, cardCount);
        for(int i = 0; i<topNCards.Count; i++)
            Debug.Log("TopCard:" + topNCards[i]);
        WhotManager.instance.pileCardManager.RemoveCardFromPileList (topNCards, 0,type);  
        //WhotManager.instance.pileCardIds.RemoveRange (0, cardCount);
        //WhotManager.instance.UpdateCardLeftNumber ();

        return topNCards;
	}




	/// <summary>
	/// When user Throws the card.
	/// </summary>
	/// <param name="cardName">Card name.</param>
	public virtual void ThrowCard(Text cardName)
	{
		string cardId = cardName.text;
		ThrowCard (cardId);	
	}

	public virtual void ThrowCard(CardTumb card)
	{
        if (!WhotManager.instance.playerTurn)
            return;
        Debug.Log("Throw Card: " + card.name);
        string cardId = card.cardName.text;
		selectedCard = card;
		ThrowCard (cardId);
	}

	public virtual void ThrowCard(string cardId)
	{
		Debug.Log ("card is: "+cardId);
		if (rule.IsCardThrowable (cardId)) 
		{
            if (selectedCard != null)
            {

                //selectedCard.transform.parent = gameplayCanvas.transform;
                //selectedCard.transform.position = new Vector3(1850f, -550f, 30f);
               // selectedCard.transform.Find("whot").gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
                //selectedCard.transform.Find("whot").rotation = Quaternion.identity;
                selectedCard.GetComponent<DOTweenAnimation>().DOPlayForward();
            }
			selectedCard = null;
			UpdatePlayedPileandPlayerList (cardId);
			if (WhotManager.instance.gameOver)
				return;
			if (cardId.Contains ("who")) {
				WhotManager.instance.whotColorSelectionScreen.SetActive (true);
				return;
			}

// yes this card is throw to played card pile.
//			if (string.IsNullOrEmpty (WhotManager.instance.activeRule))
				rule.CheckImpact (cardId);
//			else
//				WhotManager.instance.activeRule = "";
			ChangeTurn ();
		}
		else 
		{
			selectedCard = null;
			WhotManager.instance.whotAlerts.ShowAlert ((int)AlertMsgName.WrongCardAlert);
			//Please take out one card from pile you are not able to throw this.
		}
	}

    /// <summary>
    /// Updates the played pile and player list when user throw a card.
    /// </summary>
    /// <param name="cardId">Card identifier.</param>
    List<int> resumeGameCards = new List<int> { 1, 8, 14, 20 };
	void UpdatePlayedPileandPlayerList(string cardId)
	{
		if (WhotManager.instance.GetTurn ()) 
		{
			WhotPlayer.instance.playerCardIds.Remove (cardId);
			if (WhotPlayer.instance.playerCardIds.Count <= 0) {
                //if (CheckIfRuleAppliedAndWin ()) {
                Debug.Log("Player Cards Empty");
                WhotPlayer.instance.noOfCards.SetActive (false);
                int cardNumPlayed = int.Parse(cardId.Substring(3, cardId.Length - 3));
                if(!resumeGameCards.Contains(cardNumPlayed))
                    WhotManager.instance.AnnounceWinner (true);
			    WhotManager.instance.ThrowCard (cardId);
                    
					return;
				//}
			} else {

				WhotManager.instance.whotAlerts.ShowAlert ((int)AlertMsgName.NoWinAlert);
			}
			//UpdateCardTumb (WhotPlayer.instance.playerCardIds, true);       //needs to be commented
			//WhotPlayer.instance.UpdateCardLeftNumber();
		}
		else
		{
			WhotOpponent.instance.opponentCardIds.Remove (cardId);
            if (WhotOpponent.instance.opponentCardIds.Count <= 0) {
                //if (CheckIfRuleAppliedAndWin ()) {
                Debug.Log("Opponent Cards Empty");
                WhotOpponent.instance.noOfCards.SetActive(false);
                int cardNumPlayed = int.Parse(cardId.Substring(3, cardId.Length - 3));
                if (!resumeGameCards.Contains(cardNumPlayed))
                    WhotManager.instance.AnnounceWinner (false);
				WhotManager.instance.ThrowCard (cardId);
					return;
				//}
			}
			//UpdateCardTumb (WhotOpponent.instance.opponentCardIds, UserDetailsManager.isAdminPlayer);       //needs to be commented
		}
		WhotManager.instance.ThrowCard (cardId);
	}

	bool CheckIfRuleAppliedAndWin()
	{
		string ruleApplied = WhotManager.instance.activeRule;
		if (!string.IsNullOrEmpty (ruleApplied)) {
			switch (ruleApplied) {
			case "Suspension":
				if (WhotConstants.suspensionWin)
					return true;
				break;
			case "PickTwo":
				if (WhotConstants.pickTwoWin)
					return true;
				break;
			case "PickThree":
				if (WhotConstants.pickThreeWin)
					return true;
				// turn changed
				break;
			}
			return false;
		} else
			return true;
	}

	/// <summary>
	/// Changes the turn and assign rule variables.
	/// </summary>
	void ChangeTurn()
	{
		string ruleApplied = WhotManager.instance.activeRule;
		WhotManager.instance.cardToPick = 1;
		if (!string.IsNullOrEmpty (ruleApplied)) {            
            switch (ruleApplied) {
			case "HoldOn":
                    WhotManager.instance.turnMsg.SetActive(true);
                    if (WhotConstants.isAI) {
                       
                        WhotManager.instance.PlayAITurn ();
				}
				else
				{
                      
                        WhotManager.instance.StartTimer ();
				}
				break;
			case "Suspension":
                    // opponent turn skipped
                    WhotManager.instance.turnMsg.SetActive(true);
                    WhotManager.instance.activeRule = "";
				if (WhotConstants.isAI) {
                       
                    }
				else
				{
                       
                        WhotManager.instance.StartTimer ();
				}
				WhotManager.instance.PlayAITurn ();
				break;
			case "PickTwo":
				//turn changed 
				WhotManager.instance.cardToPick = pickTwoRuleCardCount;
				WhotManager.instance.ChangeTurn ();
				break;
			case "PickThree":
				WhotManager.instance.cardToPick = pickThreeRuleCardCount;
				WhotManager.instance.ChangeTurn ();
				// turn changed
				break;
			case "GeneralMarket":
				WhotManager.instance.cardToPick = 1;
				WhotManager.instance.ChangeTurn ();
				break;
			}
            if (WhotManager.instance.pileCardIds.Count <= 0)
            {
                WhotManager.instance.AddCardsInPileFromPlayedCardPile();
            }
        } else
			WhotManager.instance.ChangeTurn ();
	}

}
