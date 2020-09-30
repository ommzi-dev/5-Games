using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Networking;
using System;

// Ids of cards start with first 3 letters followed by card digits.

//Circles	1	2	3	4	5	 	7	8	 	10	11	12	13	14
//Triangles	1	2	3	4	5		7	8		10	11	12	13	14
//Crosses	1	2	3		5		7			10	11		13	14
//Squares	1	2	3		5		7			10	11		13	14
//Stars	    1	2	3	4	5		7	8
//Whot!	Four or five cards, numbered "20" in some packs.


public class WhotManager : MonoBehaviour
{

    public static WhotManager instance;
    // List of card IDs
    public List<string> cardIdList = new List<string>();
    //	public List<string> playerCardIds = new List<string> ();
    //	public List<string> opponentCardIds = new List<string>();
    public List<string> pileCardIds = new List<string>();
    public List<string> playedCardIds = new List<string>();

    public string topCardPlayed = "";
    public string activeRule = "";
    public int cardToPick = 1, disconnectTimer;

    public string colorSelectedAfterWhot = "cir";
    public bool playerTurn = true;

    public PileCards pileCardManager;

    public GameObject playerTurnMsg, opponentTurnMsg, playerDisable, opponentDisable, whotColorSelectionScreen, messagePopup;
    public bool gameOver = false;
    public bool isOnlineMode;
    public int myTurn;


    public Alerts whotAlerts;
    public GameObject goMarket, DefendObject, chatOptions;
	public GameObject topPileCard, pileArea, whotCard;

    public Text CardLeftText, opponentDisconnectTimer;
    public GameObject obj_3seconds, opponentDisconnectPopup, ModeSelectionScreen, RulesScreen, OptionsScreen, ProfileScreen, Loader;

    //public Text tokenAmount1, tokenAmount2;
    public Text winTokenAmount, lostTokenAmount;

    public Text WageredAmountText1, WageredAmountText2;

    public Text CPUName;
    public Text OpponentNameText, PlayerNameText;
    public Texture defaultImage;
    public RawImage OpponentImage;
    public GameObject turnMsg;
    bool intiateGame = false;
    bool checkPoolStatusAfterDisconnect = false;
   bool opponentState = true;
    bool playerState = true;
    void Awake()
    {
        instance = this;
        PhotonNetwork.OnEventCall += this.OnEvent;
    }

    void Update()
    {
        if (isOnlineMode)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                WhotUiManager.instance.connecting.SetActive(true);
                checkPoolStatusAfterDisconnect = true;
                // OnApplicationFocus(false);
                Time.timeScale = 0;
                Debug.Log("Error. Check internet connection!");
            }
            else
            {
                //OnApplicationFocus(true);
                if(!opponentDisconnectPopup.activeSelf && playerState && opponentState)
                    Time.timeScale = 1;
                if(playerState)
                WhotUiManager.instance.connecting.SetActive(false);
                if (checkPoolStatusAfterDisconnect)
                {
                    checkPoolStatusOnInternetBreakDown();
                }
            }
        }
        if (!gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (WhotMenu.go.activeSelf)
                    return;
                if (WhotUiManager.instance.exitPopUp.gameObject.activeSelf)
                {
                    //QuitPopUp.instance.OnClickNo ();
                    //WhotUiManager.instance.NoOnExitPopClicked();
                }
                else
                {
                    WhotUiManager.instance.ShowExitPopUp();
                    //GamePlayPanel.instance.OnClickExit ();
                }
            }
        }
    }
    void checkPoolStatusOnInternetBreakDown()
    {
        checkPoolStatusAfterDisconnect = false;
        StartCoroutine(checkPoolStatusOnBreak());
    }
    IEnumerator checkPoolStatusOnBreak()
    {
        Debug.Log("checkpoolstatus" + WHOTMultiplayerManager.Instance.poolId);
        WWWForm form = new WWWForm();

        form.AddField("poolid", WHOTMultiplayerManager.Instance.poolId);
        yield return new WaitForSecondsRealtime(3f);
        UnityWebRequest www = UnityWebRequest.Post("http://18.191.157.16:4000/apis/getpoolresult", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();

        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Transation Completed with error: " + www.error);
        }
        else
        {

            Debug.Log("checkpoolstatus" + www.downloadHandler.text);
            var statsList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var result = (IDictionary)statsList["result"];

            if (www.downloadHandler.text.Contains("Room does not exists!"))
            {
                yield return null;
            }
            else if (Convert.ToBoolean(result["success"].ToString()))
            {
                if (result["userStatus"].ToString() == "Winner")
                {
                    WhotManager.instance.AnnounceWinner(true);
                }
                else if (result["userStatus"].ToString() == "Looser")
                {
                    WhotManager.instance.AnnounceWinner(false);
                }
                else
                {
                    PhotonNetwork.ReconnectAndRejoin();
                    StartCoroutine(sendFeedback());
                }
            }
        }
    }
    public void removeOnEventCall()
    {
        PhotonNetwork.OnEventCall -= this.OnEvent;
    }
    private void OnEvent(byte eventcode, object content, int senderid)
    {
        if (eventcode == 151)
        {
            if (WHOTMultiplayerManager.Instance.gameStarted)
            {
                StartDisconnectTimer();
                Time.timeScale = 0f;
                opponentState = false;
                Debug.Log("Opponent Left!");
            }
        }
        else if (eventcode == 152)
        {
            if(playerState)
            Time.timeScale = 1f;
            StopDisconnectTimer();
            opponentState = true;
            Debug.Log("Opponent Joined Back!");
        }
        else if (eventcode == 153)
        {
            Debug.Log("Opponent Quit");

           AnnounceWinner(true);
            StopDisconnectTimer();
        }
    }

    public void StartDisconnectTimer()
    {
        Debug.Log("Starting Disconnect Timer!");
        disconnectTimer = 180;
        opponentDisconnectTimer.text = disconnectTimer.ToString();
        opponentDisconnectPopup.SetActive(true);
        StartCoroutine(UpdateDisconnectTimer());
        Time.timeScale = 0f;
    }

    IEnumerator UpdateDisconnectTimer()
    {
        yield return new WaitForSecondsRealtime(1f);
        disconnectTimer -= 1;
        Debug.Log("Disconnect Timer: "+ disconnectTimer);
        opponentDisconnectTimer.text = disconnectTimer.ToString();
        if (disconnectTimer > 0 && !WhotUiManager.instance.resultDisplayed)
        {
            if (Time.timeScale == 0)
                StartCoroutine(UpdateDisconnectTimer());
        }
        else
        {
            AnnounceWinner(true);
            StopDisconnectTimer();
        }
    }

    public void StopDisconnectTimer()
    {
        //Time.timeScale = 1f;
        Debug.Log("Stopping Disconnect Timer!");
        opponentDisconnectPopup.SetActive(false);
        StopCoroutine(UpdateDisconnectTimer());
    }

    private void OnApplicationFocus(bool focus)
    {
        playerState = focus;
        if (!isOnlineMode)
            return;
        if (focus)
        {
            
            Debug.Log("Application resume");
            if(opponentState )
            Time.timeScale = 1f;
            WhotUiManager.instance.connecting.SetActive(true);
            StartCoroutine(checkPoolStatus());
            if ( PhotonNetwork.room!=null)
            {
               
                WhotUiManager.instance.connecting.SetActive(false);
                PhotonNetwork.RaiseEvent(152, 1, true, null);
                PhotonNetwork.SendOutgoingCommands();
            }
                       
           
        }
        else
        {
           // playerState = false;
            Debug.Log("Application pause");
            WhotUiManager.instance.connecting.SetActive(true);
            Time.timeScale = 0f;
            PhotonNetwork.RaiseEvent(151, 1, true, null);
            PhotonNetwork.SendOutgoingCommands();


        }
    }
    private void OnApplicationQuit()
    {
        if (WHOTMultiplayerManager.Instance != null && WHOTMultiplayerManager.Instance.gameStarted)
        {
            if (PhotonNetwork.room != null)
            {
                PhotonNetwork.RaiseEvent(153, 1, true, null);
               // PhotonNetwork.SendOutgoingCommands();
            }
        }
    }
    IEnumerator checkPoolStatus()
    {
        Debug.Log("checkpoolstatus" + WHOTMultiplayerManager.Instance.poolId);
        WWWForm form = new WWWForm();
       
        form.AddField("poolid", WHOTMultiplayerManager.Instance.poolId);
        yield return new WaitForSeconds(2f);
        UnityWebRequest www = UnityWebRequest.Post("http://18.191.157.16:4000/apis/getpoolresult", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();
       
        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Transation Completed with error: " + www.error);
        }
        else
        {
            
            Debug.Log("checkpoolstatus" + www.downloadHandler.text);
            var statsList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var result = (IDictionary)statsList["result"];
            
            if( www.downloadHandler.text.Contains("Room does not exists!"))
            {
                yield return null;
            }
            else if (Convert.ToBoolean(result["success"].ToString()))
            {
                if (result["userStatus"].ToString() == "Winner")
                {
                    WhotManager.instance.AnnounceWinner(true);
                }
                else if(result["userStatus"].ToString() == "Looser")
                {
                    WhotManager.instance.AnnounceWinner(false);
                }
                else
                {
                    PhotonNetwork.ReconnectAndRejoin();
                    StartCoroutine(sendFeedback());
                }
            }
        }
    }
    IEnumerator sendFeedback()
    {
        yield return new WaitForSecondsRealtime(1f);

        if (PhotonNetwork.room != null)
        {
         
            PhotonNetwork.RaiseEvent(152, 1, true, null);
            PhotonNetwork.SendOutgoingCommands();
            WhotUiManager.instance.connecting.SetActive(false);
        }
        else
        {
            StartCoroutine(sendFeedback());
        }
    }
    void OnDestroy()
    {
        instance = null;
        PhotonNetwork.OnEventCall -= this.OnEvent;

    }
    // Use this for initialization
    public void InitiateGame()
    {
        if (intiateGame)
        {
            return;
        }
        intiateGame = true;
        Debug.Log("Initiating Game!!");
        WhotConstants.SetColorFolderDic();
        PlayerPrefs.SetInt("PickTwo", 0);
        PlayerPrefs.SetInt("PickThree", 0);

        if (!WhotConstants.isAI)
        {
            isOnlineMode = true;
            WhotUiManager.instance.resultDisplayed = false;
            myRemainingLives.transform.parent.gameObject.SetActive(true);
            opponentRemainingLives.transform.parent.gameObject.SetActive(true);

            if (WHOTMultiplayerManager.Instance.myTurn == 0)
            {
                //StartTimer ();        //*commented before modification
                WhotManager.instance.turnMsg.SetActive(true);
                myTurn = 0;
                playerTurn = true;
                AddCardsInPile();
                Debug.Log("Distribute cards under whotmanager 285");
                Invoke("DistributeCards", 1);
                UpdateUserAreaAndMsg();
                //WHOTMultiplayerManager.instance.SendProfile();        //No need to send profile as we are getting info from photon 
            }
            else
            {
                WhotManager.instance.turnMsg.SetActive(false);
                myTurn = 1;
                playerTurn = false;
                //DistributeForSecondPlayer();
                //Invoke ("AddOneCardInPlayedList", 1.1f);                
            }
            StartCoroutine(ResetUIForGame());
        }
        else
        {
            myRemainingLives.transform.parent.gameObject.SetActive(false);
            opponentRemainingLives.transform.parent.gameObject.SetActive(false);
        }
    }

    IEnumerator ResetUIForGame()
    {
        yield return new WaitForSeconds(1f);
        opponentDisconnectPopup.SetActive(false);
        ModeSelectionScreen.SetActive(false);
        RulesScreen.SetActive(false);
        OptionsScreen.SetActive(false);
        Loader.SetActive(false);
        WHOTMultiplayerManager.Instance.mainMenu.SetActive(false);
        WHOTMultiplayerManager.Instance.multiplayerOptions.SetActive(false);
        WHOTMultiplayerManager.Instance.gamePlay.SetActive(true);
        WHOTMultiplayerManager.Instance.betSelectionPanel.SetActive(false);
        WHOTMultiplayerManager.Instance.availableRooms.SetActive(false);
        WHOTMultiplayerManager.Instance.SearchOpponentPanel.SetActive(false);
        WHOTMultiplayerManager.Instance.challengeScreen.SetActive(false);
        WHOTMultiplayerManager.Instance.createChallengeDialog.SetActive(false);
        WhotOpponent.instance.UpdateOpponentImage(defaultImage);
        //WHOTMultiplayerManager.instance.opponentName.text = "OPPONENT";
    }

    public void SetOppoPro(string url, string name)
    {
        StartCoroutine(StartUrl(url));
        OpponentNameText.text = WhotOpponent.instance.userName;
    }

    IEnumerator StartUrl(string url)
    {
        // Start a download of the given URL
        using (WWW www = new WWW(url))
        {
            // Wait for download to complete
            yield return www;

            // assign texture
            OpponentImage.texture = www.texture;
        }
    }
    public Text MatchTime;
    public int remainingMatchTime;

    public System.DateTime startedTime;
    public System.DateTime endingTime;

    IEnumerator UpdateMatchTimer()
    {
        System.TimeSpan t = endingTime - System.DateTime.Now;
        System.TimeSpan t1 = new System.TimeSpan(0, 5, 0);
        MatchTime.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);

        if (t <= t1 && (t >= new System.TimeSpan(0, 0, 0)))
        {
            if (!gameOver)
            {
                MatchTime.text = "MATCH TIME : " + "0" + t.Minutes + ":" + t.Seconds;
                StartCoroutine(UpdateMatchTimer());
            }
        }
        else
        {
            StopCoroutine(UpdateMatchTimer());
            //WHOTMultiplayerManager.instance.Disconnect ();
        }
    }

    public void OnStart()
    {
        
        WhotConstants.SetColorFolderDic();


        if (WhotConstants.isAI)
        {
            isOnlineMode = false;
            AddCardsInPile();
            Debug.Log("Distribute cards under whotmanager");
            Invoke("DistributeCards", 1);
            UpdateUserAreaAndMsg();
            List<string> AINames = new List<string> { "Rainbow", "Sunshine", "Marigold" };
            CPUName.text = AINames[UnityEngine.Random.Range(0, 2)];
            PlayerNameText.text = UserDetailsManager.userName;
            chatOptions.SetActive(false);
        }
        
    }

    public int remainingTime = 60;      //#RK
    public Text myRemainingLives;
    public Text opponentRemainingLives;

    [SerializeField] int myRemainingLivesCounter = 3;
    public int opponentRemainingLivesCounter = 3;

    public void StartTimer()            
    {
        //Debug.Log("Start Timer Called!");
        CancelInvoke("UpdateTimer");
        StopTimer();
        if (playerTurn)
        {
            turnMsg.SetActive(true);
            remainingTime = 60;
            Invoke("UpdateTimer", 1);
            playerTurnMsg.SetActive(true);
            opponentTurnMsg.SetActive(false);
            playerTurnMsg.GetComponent<Image>().DOKill();
            playerTurnMsg.GetComponent<Image>().fillAmount = 1;
            playerTurnMsg.GetComponent<Image>().DOFillAmount(0, 60).SetEase(Ease.Linear);
            if(!string.IsNullOrEmpty(activeRule) && pileCardIds.Count <= 0)
            {
                AddCardsInPileFromPlayedCardPile();
            }
        }
        else
        {
            //oppoTemerObj.gameObject.SetActive (true);
            //myTemerObj.gameObject.SetActive (false);
            //oppoTemerObj.text = "00:20";
            remainingTime = 60;
            playerTurnMsg.SetActive(false);
            opponentTurnMsg.SetActive(true);
            opponentTurnMsg.GetComponent<Image>().fillAmount = 1;
            opponentTurnMsg.GetComponent<Image>().DOFillAmount(0, 60).SetEase(Ease.Linear);
            Invoke("UpdateTimer", 1);
        }
    }

    public void StopTimer()
    {
        //Debug.Log("Stop Timer Called!");
        obj_3seconds.SetActive(false);
        CancelInvoke("UpdateTimer");
        opponentTurnMsg.gameObject.SetActive(false);
        playerTurnMsg.gameObject.SetActive(false);
        turnMsg.SetActive(false);
        opponentTurnMsg.transform.DOKill(true);
        playerTurnMsg.transform.DOKill(true);
        opponentTurnMsg.GetComponent<Image>().DOKill(true);
        playerTurnMsg.GetComponent<Image>().DOKill(true);
    }

    public void UpdateTimer()
    {

        remainingTime--;
        //Debug.Log("Update Timer Called! Remaining Time: "+ remainingTime);
        if (remainingTime > 0)
        {
            Invoke("UpdateTimer", 1);

            if (playerTurn)
            {
                if (remainingTime <= 3)
                {
                    obj_3seconds.SetActive(true);
                    obj_3seconds.transform.GetChild(0).GetComponent<Text>().text = "" + remainingTime.ToString();
                }
            }
        }
        else
        {
            StopTimer();

            if (playerTurn)
            {
                myRemainingLivesCounter--;
                if (myRemainingLivesCounter == 0)
                {
                    WhotManager.instance.gameOver = true;
                    
                    //WHOTMultiplayerManager.instance.Disconnect ();
                    //---
                    //OnBackOutOfGamePanel();
                    //outOfTheGamePanel.SetActive (true);
                    //WHOTMultiplayerManager.instance.gamePlay.SetActive(false);
                    WHOTMultiplayerManager.Instance.OutOfLives.SetActive(true);
                   // StartCoroutine(EndGame());
                    return;
                }
                //	Player.instance.WithDrawCard (cardToPick);
                if (playedCardIds[playedCardIds.Count - 1].Substring(0, 3) == "who")
                {
                    WhotColorSelection("cir");
                }
                else
                {
                    PickPlayerCard();
                    ChangeTurn();
                    WHOTMultiplayerManager.Instance.SendChangeTurn();
                }
                //emit
                myRemainingLives.text = myRemainingLivesCounter.ToString();                
            }
        }
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(2f);
        WHOTMultiplayerManager.Instance.OutOfLives.SetActive(false);
        WhotUiManager.instance.ShowWinLosePopUp(false);
        
    }

    /// <summary>
    /// Adds the cards in pile.
    /// 54 cards randomaly assigned in list
    /// </summary>
    void AddCardsInPile()
    {
        List<string> cardList = new List<string>(cardIdList);
        for (int i = 0; i < cardIdList.Count; i++)
        {
            int rand = UnityEngine.Random.Range(0, cardList.Count);
            pileCardIds.Add(cardList[rand]);
            cardList.RemoveAt(rand);
        }
        UpdateCardLeftNumber();

        for (int i = pileCardIds.Count - 1; i >= 0; i--)
        {
            pileCardManager.AddCardInPileList(pileCardIds[i]);
        }
        if (!WhotConstants.isAI)
        {
            if (myTurn == 0)
            {
                WHOTMultiplayerManager.Instance.SendData();
            }
        }
    }

    public void AddCardsInPileForSecond()
    {
        UpdateCardLeftNumber();
        for (int i = pileCardIds.Count - 1; i >= 0; i--)
        {
            pileCardManager.AddCardInPileList(pileCardIds[i]);
        }
        //DistributeForSecondPlayer();
        Invoke("DistributeForSecondPlayer", 1f);
    }

    /// <summary>
    /// Adds the cards in pile from played card pile when pile is getting empty.
    /// </summary>
    public void AddCardsInPileFromPlayedCardPile()
    {
        messagePopup.SetActive(true);
        StartCoroutine(DisplayResult());
        
        /*      //Display Result: Player with least sum of Cards Win!
        List<string> cardList = (playedCardIds);
        Debug.Log(cardList);
        for (int i = cardList.Count - 1; i >= 0; i--)
            pileCardManager.AddCardInPileList(cardList[i]);
        pileCardManager.RemoveCardFromPlayedList(cardList.Count-1);
        */
       /* for (int i = 0; i < playedCardIds.Count - 1; i++)
        {
            int rand = Random.Range(0, cardList.Count);
            pileCardIds.Add(cardList[rand]);
            cardList.RemoveAt(rand);
            UpdateCardLeftNumber();
        }*/        
    }

    IEnumerator DisplayResult()
    {
        yield return new WaitForSeconds(1f);
        messagePopup.SetActive(false);
        int playerSum = WhotPlayer.instance.sumOfCards;
        int opponentSum = WhotOpponent.instance.sumOfCards;
        if (playerSum > opponentSum)
            AnnounceWinner(false);
        else
            AnnounceWinner(true);
    }
    
    void DistributeCards()
    {
        Debug.Log("Distribute Cards Called!" + pileCardIds.Count);
        float a = 0f;
        for (int i = 0; i < WhotConstants.initialCards; i++)
        {
            Invoke("AddPlayerCard", 1 + (i * 0.5f));
            a = 1 + (i * 0.5f);
        }
        Invoke("AddOneCardInPlayedList", 2f + a);
        
    }

    void AddPlayerCard()
    {
        Debug.Log("Add player Card!!");
        WhotPlayer.instance.WithDrawCard(1);
        Invoke("AddOpponentCard", 0.25f);
    }

    void AddOpponentCard()
    {
        WhotOpponent.instance.WithDrawCard(1);
    }

    public void DistributeForSecondPlayer()
    {
        float a = 0f;
        for (int i = 0; i < WhotConstants.initialCards; i++)
        {
            Invoke("AddOpponentCardForSecondPlayer", 1 + (i * 0.5f));
            a = 1 + (i * 0.5f);
        }
        Invoke("AddOneCardInPlayedList", 2f + a);
        UpdateUserAreaAndMsg();
        //StartTimer ();        //*commented before modification
    }
    void AddOpponentCardForSecondPlayer()
    {
        WhotOpponent.instance.WithDrawCard(1);
        Invoke("AddPlayerCardForSecond", 0.25f);
    }

    void AddPlayerCardForSecond()
    {
        WhotPlayer.instance.WithDrawCard(1);
    }
    //----

    /// <summary>
    /// Distributes the 6 cards for each player.
    /// </summary>

    /// <summary>
    /// Adds the one card in played list in the begining.w
    /// </summary>
    void AddOneCardInPlayedList()
    {
        if (!WhotConstants.isAI)
        {
            startedTime = System.DateTime.Now;
            System.TimeSpan t = new System.TimeSpan(0, 5, 0);
            endingTime = startedTime + t;
            //StartCoroutine(UpdateMatchTimer());
            StartTimer();
        }

        for (int count = 0; count < 54; count++)
        {
            if (pileCardIds[count].Contains("who"))
            {
                continue;
            }
            playedCardIds.Add(pileCardIds[count]);
            UpdateCardLeftNumber();
            
            SoundManger.instance.PlaySound((int)(ListOfSounds.Draw));
            pileCardManager.RemoveCardFromPileList(new List<string> { pileCardIds[count] }, count, 3);
            pileCardManager.AddCardInPlayedList(playedCardIds[0]);
            topCardPlayed = playedCardIds[0];
            break;
        }

        if (WhotConstants.isAI)
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                playerTurn = true;
            }
            else
            {
                ChangeTurn();
            }
        }
    }

    public bool PlayAITurn()
    {
        if (WhotConstants.isAI && !playerTurn)
        {
            WhotAI.instance.ThrowCard();
            return true;
        }
        return false;
    }

    public void PickPlayerCard()
    {
        switch (activeRule)
        {
            case "PickTwo":
                cardToPick = 2;
                PlayerPrefs.SetInt("PickTwo", 0);
                break;
            case "PickThree":
                cardToPick = 3;
                PlayerPrefs.SetInt("PickThree", 0);
                break;
            case "GeneralMarket":
                cardToPick = 1;
                break;
            default:
                cardToPick = 1;
                break;
        }

        WhotPlayer.instance.WithDrawCard(cardToPick);
            goMarket.SetActive(false);
            whotAlerts.HideAlert();
            activeRule = "";        
    }

    public void PickOpponentCard()
    {
        switch (activeRule)
        {
            case "PickTwo":
                cardToPick = 2;
                PlayerPrefs.SetInt("PickTwo", 0);
                break;
            case "PickThree":
                cardToPick = 3;
                PlayerPrefs.SetInt("PickThree", 0);
                break;
            case "GeneralMarket":
                cardToPick = 1;
                break;
            default:
                cardToPick = 1;
                break;
        }
        WhotOpponent.instance.WithDrawCard(cardToPick);
            goMarket.SetActive(false);
            whotAlerts.HideAlert();
            activeRule = "";        
    }

    /// <summary>
    /// Changes the turn of user.
    /// </summary>
    public void ChangeTurn()
    {
        DefendObject.SetActive(false);
        //		pileCardManger.HideColorAfterWhot ();
        playerTurn = !playerTurn;
        UpdateUserAreaAndMsg();
        if (WhotConstants.isAI)
        {
            if (PlayAITurn())
            {
               turnMsg.SetActive(false);
                return;
            }
            else
            {
                turnMsg.SetActive(true);
            }
        }
        else 
        {
            StartTimer();
        }

        //Force player to pick from pile or throw the particular num
        if (!string.IsNullOrEmpty(activeRule))
        {
            int lastCardIdNum = int.Parse(topCardPlayed.Substring(3, topCardPlayed.Length - 3));
            switch (activeRule)
            {
                case "PickTwo":
                    /*if (CheckCardNumberFromPlayerList(lastCardIdNum, GetActivePlayerList()) != "")
                    {
                        DefendObject.SetActive(true);
                        SoundManger.instance.PlaySound((int)(ListOfSounds.Defend));
                        //highlight 2 rule ended
                    }
                    else
                    {*/
                        if (playerTurn)
                            whotAlerts.ShowAlert((int)AlertMsgName.Pick2Card);
                   // }
                    break;
                case "PickThree":
                    if (CheckCardNumberFromPlayerList(lastCardIdNum, GetActivePlayerList()) != "")
                    {
                        DefendObject.SetActive(true);
                        //highlight 5
                        SoundManger.instance.PlaySound((int)(ListOfSounds.Defend));
                    }
                    else
                    {
                        if (playerTurn)
                            whotAlerts.ShowAlert((int)AlertMsgName.Pick3Card);
                        //Force pick from pile
                    }
                    break;
                case "GeneralMarket":
                    //Force pick from pile
                   // playerDisable.SetActive(false);           //Commented on 17th feb to enable scroll
                    //opponentDisable.SetActive (false);
                    goMarket.SetActive(true);
                    if (playerTurn)
                        whotAlerts.ShowAlert((int)AlertMsgName.GoMarket);
                    break;
            }
        }
    }


    /// <summary>
    /// Checks the card number from player list.
    /// Check if we have specific num of card
    /// </summary>
    /// <returns><c>true</c>, if card number from player list was checked, <c>false</c> otherwise.</returns>
    /// <param name="cardNum">Card number.</param>
    /// <param name="cardList">Card list.</param>
    string CheckCardNumberFromPlayerList(int cardNum, List<string> cardList)
    {
        // Check card Number from player's list
        foreach (string id in cardList)
        {
            if (id.Substring(3, id.Length - 3).CompareTo("" + cardNum) == 0)
                return id;
        }
        return "";
    }

    /// <summary>
    /// Gets the active player list.
    /// </summary>
    /// <returns>The active player list.</returns>
    List<string> GetActivePlayerList()
    {
        if (playerTurn)
            return WhotPlayer.instance.playerCardIds;
        else
            return WhotOpponent.instance.opponentCardIds;
    }

    /// <summary>
    /// Withdraws the card.
    /// </summary>
    public void WithdrawCard()
    {
        //		if (Constants.isAI && playerDisable.activeInHierarchy &&(playerTurn))
        //			return;
        Debug.Log("WithdrawCard");
        goMarket.SetActive(false);
        whotAlerts.HideAlert();
        Debug.Log("Card To Pick:" + cardToPick);

        if (CheckIfCardIdThrowable())      
        {
            return;
        }
        if (!WhotConstants.isAI)
        {
            StopTimer();
        }

        Debug.Log("Card To Pick11:" + cardToPick);

        if (PlayerPrefs.GetInt("PickTwo") == 1)
        {
            cardToPick = 2;
            PlayerPrefs.SetInt("PickTwo", 0);
        }
        else if (PlayerPrefs.GetInt("PickThree") == 1)
        {
            cardToPick = 3;
            PlayerPrefs.SetInt("PickThree", 0);
        }
        else
        {
            cardToPick = 1;
        }
        activeRule = "";

        if (playerTurn)
        {
            Debug.Log("WithdrawCard 1: "+ WhotConstants.isAI);            
            WhotPlayer.instance.WithDrawCard(cardToPick);
            if (!WhotConstants.isAI)
            {
                WHOTMultiplayerManager.Instance.SendMove(2, "");
            }
        }
        else
        {
            Debug.Log("WithdrawCard2");
            WhotOpponent.instance.WithDrawCard(cardToPick);
        }
        cardToPick = 1;
        //if (pileCardIds.Count < cardToPick)
        //    AddCardsInPileFromPlayedCardPile();
        ChangeTurn();
    }

    public void WithdrawCardForOpponent()
    {
        goMarket.SetActive(false);
        whotAlerts.HideAlert();
        Debug.Log("Card To Pick Oppo:" + cardToPick);
        if (CheckIfCardIdThrowable())
        {    if(WhotConstants.isAI)       
                return;
        }
        Debug.Log("Card To Pick Oppo11:" + cardToPick);
        if (!WhotConstants.isAI)
        {
            StopTimer();
        }
        if(PlayerPrefs.GetInt("PickTwo") == 1)
        {
            cardToPick = 2;
            PlayerPrefs.SetInt("PickTwo", 0);
        }
        else if(PlayerPrefs.GetInt("PickThree") == 1)
        {
            cardToPick = 3;
            PlayerPrefs.SetInt("PickThree", 0);
        }
        else
        {
            cardToPick = 1;
        }
        
        WhotOpponent.instance.WithDrawCard(cardToPick);
        activeRule = "";
        //if (pileCardIds.Count < cardToPick)
        //    AddCardsInPileFromPlayedCardPile();
        ChangeTurn();
    }

    /// <summary>
    /// Checks if card identifier throwable.
    /// when user withdraw a card 
    /// </summary>
    /// <returns><c>true</c>, if card identifier throwable was checked, <c>false</c> otherwise.</returns>
    bool CheckIfCardIdThrowable()
    {
        //Force player to pick from pile or throw the particular num
        if (!string.IsNullOrEmpty(activeRule))
        {
            int lastCardIdNum = int.Parse(topCardPlayed.Substring(3, topCardPlayed.Length - 3));
            switch (activeRule)
            {
                case "PickTwo":
                    string id = CheckCardNumberFromPlayerList(lastCardIdNum, GetActivePlayerList());
                    if (id != "" && WhotConstants.pickTwoDefend)
                    {
                        if (playerTurn && WhotConstants.isHint)
                            WhotPlayer.instance.HighlightCard(id);
                        //highlight 2 rule ended
                        return true;
                    }
                    break;
                case "PickThree":
                    string id1 = CheckCardNumberFromPlayerList(lastCardIdNum, GetActivePlayerList());
                    if (id1 != "" && WhotConstants.pickThreeDefend)
                    {
                        if (playerTurn && WhotConstants.isHint)
                            WhotPlayer.instance.HighlightCard(id1);
                        //highlight 5
                        return true;
                    }
                    break;
            }
        }
        else
        {
            //			var cardList = GetActivePlayerList ();
            //			// Check card Number from player's list
            //			string lastCardId = topCardPlayed;
            //
            //			foreach (string id in cardList) {
            //
            //				int cardNumPlayed = int.Parse (id.Substring (3, id.Length - 3));
            //				int lastCardIdNum = int.Parse (lastCardId.Substring (3, lastCardId.Length - 3));
            //
            //				if (!id.Contains("who")&&((lastCardId.Contains ("who") && colorSelectedAfterWhot.Equals (id.Substring (0, 3))) ||
            //					lastCardId.Contains (id.Substring (0, 3)) || lastCardIdNum == cardNumPlayed)) {
            //					if (playerTurn)
            //						Player.instance.HighlightCard (id);
            //					return true;
            //				}
            //			}
        }
        return false;
    }

    /// <summary>
    /// When user Throws the card.
    /// </summary>
    /// <param name="id">Identifier.</param>
    public void ThrowCard(string id)
    {
        Debug.Log("Throw Card: "+ id);
        pileCardManager.HideColorAfterWhot();
        goMarket.SetActive(false);
        whotAlerts.HideAlert();
        playedCardIds.Add(id);
        pileCardManager.AddCardInPlayedList(id);
        topCardPlayed = id;
        if (!playerTurn && !WhotConstants.isAI)
        {
            StopTimer();
        }
        if (playerTurn && !WhotConstants.isAI && id.Substring(0, 3) != "who")
        {
            StopTimer();
            WHOTMultiplayerManager.Instance.SendMove(1, id);
        }
    }

    /// <summary>
    /// Gets the turn.
    /// </summary>
    /// <returns><c>true</c>, if turn was gotten, <c>false</c> otherwise.</returns>
    public bool GetTurn()
    {
        return playerTurn;
    }

    /// <summary>
    /// Updates the user area and message.
    /// </summary>
    public void UpdateUserAreaAndMsg()
    {
        WhotManager.instance.turnMsg.SetActive(playerTurn);
        playerTurnMsg.SetActive(playerTurn);
        opponentTurnMsg.SetActive(!playerTurn);
        //playerDisable.SetActive(!playerTurn);     //Commented on 17th feb to enable scroll
        //opponentDisable.SetActive (playerTurn);
    }

    /// <summary>
    /// Announces the winner.
    /// </summary>
    /// <param name="i">The index.</param>
    public void AnnounceWinner(bool isPlayerWinner)
    {
        gameOver = true;
        WhotUiManager.instance.ShowWinLosePopUp(isPlayerWinner);
        //WhotPlayer.instance.scroll.enabled = false;
    }

    string idPlayed = "";
    /// <summary>
    /// color selection made after whot card
    /// </summary>
    /// <param name="color">Color.</param>
    public void WhotColorSelection(string color)
    {
        if (!WhotConstants.isAI && playerTurn)
        {
            WHOTMultiplayerManager.Instance.SendMove(3, topCardPlayed, color);
        }
        colorSelectedAfterWhot = color;
        WhotManager.instance.whotColorSelectionScreen.SetActive(false);
        ChangeTurn();
        pileCardManager.SetColorAfterWhot(color);
    }

    public void UpdateCardLeftNumber()
    {
        CardLeftText.text = "" + pileCardIds.Count;
    }

    public void ShowTopPileCard()
    {
        if (pileCardIds.Count > 0)
        {
            //Debug.Log("Pile Cards Count: " + (pileCardIds.Count));
            topPileCard = pileArea.transform.Find(pileCardIds[0]).gameObject;
            //Debug.Log("Top Pile Card: " + topPileCard.name);
            topPileCard.GetComponent<Button>().interactable = true;
            whotCard = topPileCard.transform.Find("whot").gameObject;
            //whotCard.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture>("cards/" + folderName + "/" + cardNum);

            if (UserDetailsManager.isAdminPlayer)
                whotCard.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            else
                whotCard.transform.localRotation = Quaternion.identity;
        }
        else
        {
            AddCardsInPileFromPlayedCardPile();
        }
    }
}
