using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Update view for pile card and played cards
/// </summary>
public class PileCards : MonoBehaviour {

	public CardTumb pileCardTumb;
	public CardTumb playedCardTumb;
	public Transform pileParent, playedparent;
	public GameObject colorinfopanel;
    public Material standardMat;
	public Text colorname;
    float pileZPos = 0f, playedCardZPos = 0f, pileXPos =0f;
//	List<CardTumb> pileCards = new List<CardTumb>();
//	List<CardTumb> playedList = new List<CardTumb>();
	// Use this for initialization
	void Start () {
        Debug.Log("Played Cards: " + (playedparent.childCount));
    }

	public void AddCardInPileList(string id)
	{
		CardTumb card = (WhotConstants.CreateDuplicate (pileCardTumb.gameObject)).GetComponent<CardTumb>();
        card.GetComponent<RectTransform>().sizeDelta = new Vector2(190f, 250f);
        card.name = id;
        if(!WhotManager.instance.pileCardIds.Contains(id))
            WhotManager.instance.pileCardIds.Add(id);
		card.SetCardName(id,false);
		card.transform.localPosition = new Vector3(pileCardTumb.transform.localPosition.x, pileCardTumb.transform.localPosition.y, pileZPos);
        pileZPos -= 0.01f;
	}
	public void AddCardInPlayedList(string id)
	{
        
		CardTumb card = (WhotConstants.CreateDuplicate (playedCardTumb.gameObject)).GetComponent<CardTumb>();
		card.name = id;
        if (!WhotManager.instance.playedCardIds.Contains(id))
            WhotManager.instance.playedCardIds.Add(id);
        card.SetCardName(id);
		card.transform.localPosition = new Vector3(playedCardTumb.transform.localPosition.x, playedCardTumb.transform.localPosition.y, playedCardZPos);
        card.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0, 359f));
		playedCardZPos -= 0.01f;
        if (playedparent.transform.childCount > 3)
        {
            Transform lastPlayedCard = playedparent.GetChild(playedparent.transform.childCount - 2);
            GameObject fbxCard = lastPlayedCard.Find("whot").gameObject;
            fbxCard.GetComponent<MeshRenderer>().material = standardMat;
        }
    }

	public void RemoveCardFromPileList(List<string> noOfCards, int pos, int type)
	{
        Debug.Log("Removing Cards: " +noOfCards.Count + " "+ WhotManager.instance.pileCardIds.Count);
        Debug.Log("Pile Child Count: "+ pileParent.childCount);
		for (int i = 0; i < noOfCards.Count; i++) {
            Debug.Log("Removing: " +noOfCards[i]);
			pileParent.Find(noOfCards[i]).GetComponent<CardTumb> ().PlayTween (type);
			pileZPos += 0.01f;
            WhotManager.instance.pileCardIds.Remove(noOfCards[i]);
            WhotManager.instance.UpdateCardLeftNumber();            
        }
        WhotManager.instance.ShowTopPileCard();
        //if(no)
        //WhotManager.instance.pileCardIds.RemoveRange(0, noOfCards);
        //{
        //    for(int i =0; i< noOfCards-1;i++)
        //        Destroy(pileParent.GetChild(WhotManager.instance.pileCardIds.Count + 2).gameObject);
        //}
    }

    public void RemoveCardFromPlayedList(int noOfCards)
    {
        Debug.Log("Removing "+ noOfCards + "cards from played List");
        Debug.Log("Played Cards: "+ (playedparent.childCount - 2));
        Debug.Log("Played Card List<string>: "+ WhotManager.instance.playedCardIds.Count);
        if (playedparent.childCount >= noOfCards +3)
        {
            for (int i = 3; i < noOfCards+3; i++)
            {
                GameObject card = playedparent.GetChild(i).gameObject;
                Debug.Log("Removing "+ card.name);
                WhotManager.instance.playedCardIds.Remove(card.name);
                Destroy(card);
                playedCardZPos += 0.01f;
            }
        }
        else
        {
            Debug.Log("Played Card list doesn't have enough cards to be removed.");
        }
    }

	public void WithdrawCardFromPile()
	{
        Debug.Log("WithdrawCardFromPile()");
		if ((WhotConstants.isAI && WhotManager.instance.playerTurn)) {
			SoundManger.instance.PlaySound("ButtonSound");
			WhotManager.instance.WithdrawCard ();
		}

		else if ( !WhotConstants.isAI)
		{
			if  (WhotManager.instance.playerTurn )
			{
				WhotManager.instance.WithdrawCard ();
			}
		}
	}

	public void SetColorAfterWhot(string color)
	{
		colorinfopanel.SetActive (true);
		colorname.text = WhotConstants.GetColorFolder(color);
		int soundId = -1;
		switch (colorname.text) {
		case "Circle":
			soundId = (int)ListOfSounds.Circle;
			break;
		case "Triangle":
			soundId = (int)ListOfSounds.Triangle;
			break;
		case "Crosses":
			soundId = (int)ListOfSounds.Crosses;
			break;
		case "Square":
			soundId = (int)ListOfSounds.Square;
			break;
		case "Star":
			soundId = (int)ListOfSounds.Star;
			break;
		}
		if(soundId!=-1)
			SoundManger.instance.PlaySound (soundId);
	}

	public void HideColorAfterWhot()
	{
		colorinfopanel.SetActive (false);
	}

}
