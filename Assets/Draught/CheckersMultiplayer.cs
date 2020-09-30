using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using Photon.Chat;
using System;
using UnityEngine.Networking;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class CheckersMultiplayer : Photon.PunBehaviour, IChatClientListener, IPunObservable
{
    
    public static CheckersMultiplayer Instance = null;
    public static bool playingWagered;
    public static int wagerAmount;

    [Header("Gameplay Values")]
    public int myTurn;
    public int winAmt, betAmount;
    public string MatchMakingString = "checkers";
    public string poolId, receiverName;
    public bool gameStarted, isPlayerReady, isOpponentReady, isConnected, hasJoinedLobby, canLeavePool, isChallenge;
    
    [Header("Screen References")]
    public GameObject gamePlay;
    public GameObject mainMenu, multiplayerOptions;
    public GameObject betSelectionPanel, availableRooms, buyCoins;
    public GameObject SearchOpponentPanel, challengeScreen, createChallengeDialog;
    public GameObject opponentLeftPanel;
    public GameObject disconnectedPanel, OutOfLives;
    public GameObject playButton;
    public GameObject chatText;
    public InputField chatInputText, betAmountText;
    public Text mychatText, opponentName, loadingText, betAmtError;
    public Button startGameButton;
    public ChatClient chatClient;
    bool hideNote = false;
    [SerializeField] List<GameObject> roomPrefabs = new List<GameObject>();
    public GameObject roomPrefab, roomParentObj;
    public string playerUserId, playerName;
    public Text PlayingPlayerText, PlayingPlayerText10;
    public GameObject table8x8,table10x10;
    public GameObject board8, board10;
    public GameObject mainCanvas;
    public string table;
    public Text opponentNameonBoard,opponentNameBoard10;
    public bool IsMultiPlayer;
    public bool IsTableTen = false;
    public bool IsTurnPlayer;
    public string turnof;
    public bool isJoiner = false;
    public bool resultDisplayed, OtherPlayerForceQuit;
    bool checkPoolStatusAfterDisconnect = false;
    public bool gameOver = false;
    bool playerState = true;
    bool opponentState = true;
    public GameObject connecting;
    public GameObject homeButttonMultiplayer;
    public GameObject homeButttonSinglePlayer;

    // public string rpcTurn;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           // Instance = this;
            PhotonNetwork.PhotonServerSettings.AppID = UserDetailsManager.PhotonAppID;
            PhotonNetwork.PhotonServerSettings.ChatAppID = UserDetailsManager.PhotonChatID;
        }

        if (PlayerPrefs.GetString("table10") == "yes")
        {
            IsTableTen = true;
        }
        else
        {
            IsTableTen = false;
        }
        OtherPlayerForceQuit = false;
       
        PhotonNetwork.OnEventCall += this.OnEvent;
    }
    void checkPoolStatusOnInternetBreakDown()
    {
        checkPoolStatusAfterDisconnect = false;
        StartCoroutine(checkPoolStatusOnBreak());
    }
    IEnumerator checkPoolStatusOnBreak()
    {
        WWWForm form = new WWWForm();

        form.AddField("poolid", poolId);
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
                    GameController.instance.ShowResultPanel("WIN !", true);
                }
                else if (result["userStatus"].ToString() == "Looser")
                {
                    GameController.instance.ShowResultPanel("LOSE !", false);
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
    void Start()
    {
        GetPhotonToken();
        turnof = "player";
        //PhotonNetwork.playerName = UserDetailsManager.userId;     //Set in PlayFab Manager on Authentication Success.
    }
    [PunRPC]
    void onReceivedTurn(string temp)
    {
        turnof = temp;
    }


    
    void Update()
    {
        if (isPlayerReady && isOpponentReady && !gameStarted)
        {
            loadingText.text = "LOADING GAME ...";
            //StartCoroutine(StartGameOnServer());
            isPlayerReady = false;
            isOpponentReady = false;
        }


        if (IsMultiPlayer)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                connecting.SetActive(true);
                checkPoolStatusAfterDisconnect = true;
                // OnApplicationFocus(false);
                Time.timeScale = 0;
                Debug.Log("Error. Check internet connection!");
            }
            else
            {
                //OnApplicationFocus(true);
                if (!opponentDisconnectPopup.activeSelf && playerState && opponentState)
                    Time.timeScale = 1;
                if (playerState)
                    connecting.SetActive(false);
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

    public bool challengeMode;

    public void GetPhotonToken()
    {
        //Debug.Log("Get Photon Token Called!!");
        Application.runInBackground = true;
        if (!PhotonNetwork.connected)
        {
            Debug.Log("Connecting Photon!");
            PhotonNetwork.networkingPeer.DisconnectTimeout = 180000;
            PhotonNetwork.ConnectUsingSettings("1.0");

        }
        else
        {
            StartCoroutine(FetchRoom());
        }
        //connectToChat();
        if(ChatGui.instance!=null)
        if (!ChatGui.instance.chatClient.CanChat)
        {
            ChatGui.instance.Connect();
        }
    }
    
    public void connectToChat()
    {
        /*   Debug.Log("Trying to connect 0");
           this.chatClient = new ChatClient(this);
           UserDetailsManager.chatClient = chatClient;
           // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
           Photon.Chat.AuthenticationValues authValues = new Photon.Chat.AuthenticationValues();
           authValues.UserId = UserDetailsManager.userId;
           authValues.AuthType = Photon.Chat.CustomAuthenticationType.Custom;
           authValues.AddAuthParameter("username", UserDetailsManager.userName);

           chatClient.Connect(UserDetailsManager.PhotonChatID, "1.0", authValues);
           Debug.Log("Trying to connect 1");
          */
    }

    public void OnConnected()
    {
        Debug.Log("Photon Chat connected!!!");
        chatClient.Subscribe(new string[] { "invitationsChannel" });
    }

    IEnumerator FetchRoom()
    {
        OnReceivedRoomListUpdate();
        yield return new WaitForSeconds(5f);
        StartCoroutine(FetchRoom());
        
    }

    public void JoinRoomAndStartGame()
    {

        int betAmount = 0;

        Debug.Log("Bet Amt: " + betAmountText.text);
        Debug.Log("UserDetailsManager.userCoins: " + UserDetailsManager.userCoins + " betAmount: " + betAmount);

        if (string.IsNullOrEmpty(betAmountText.text) || string.IsNullOrWhiteSpace(betAmountText.text) || int.Parse(betAmountText.text) < 500)
        {
            betAmtError.text = "Bet Amount needs to be 500 or above.";
            buyCoins.SetActive(true);
            return;
        }
        else if (UserDetailsManager.userCoins < int.Parse(betAmountText.text))
        {
            betAmtError.text = "You don't have enough coins!!";
            buyCoins.SetActive(true);
            return;
        }
        else
        {
            betAmount = int.Parse(betAmountText.text);
            Debug.Log("PlayerId: " + PhotonNetwork.player.UserId);
            PhotonNetwork.player.NickName = UserDetailsManager.userName;
            PhotonNetwork.player.UserId = UserDetailsManager.userId;
            if (isChallenge)
                StartCoroutine(CreatePool());
            else
                CheckIfRoomAvailable();
        }

        
    }

    void CheckIfRoomAvailable()
    {

        if (IsTableTen)
        {
            table = "table10";
        }
        else
        {
            table = "table8";

        }
        Debug.Log("PhotonNetwork.GetRoomList()" + PhotonNetwork.GetRoomList().Length);
        foreach (RoomInfo temp in PhotonNetwork.GetRoomList())
        {
            ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = temp.CustomProperties;

            if (expectedCustomRoomProperties["table"].ToString() == table)
            { 
                if (expectedCustomRoomProperties["game"].ToString() == "checker" && !bool.Parse(expectedCustomRoomProperties["isChallenge"].ToString()))
                {
                    if (expectedCustomRoomProperties["bet"].ToString() == betAmountText.text)
                    {
                        Debug.Log("Kuch to mila" + temp.Name);
                        RoomDetails room = roomParentObj.transform.Find(temp.Name).GetComponent<RoomDetails>();
                        room.onJoin();
                        return;
                    }
                }
        }
        }
        StartCoroutine(CreatePool());
    }

    public override void OnReceivedRoomListUpdate()
    {
        //Debug.Log("Room List Count:" + PhotonNetwork.GetRoomList().Length);
        //waitingPanel.SetActive(false);     
        if (IsTableTen)
        {
            table = "table10";
        }
        else
        {
            table = "table8";

        }
        // Debug.Log("OnCreatedRoom" + PhotonNetwork.room.Name + "aaaaa" + PhotonNetwork.GetRoomList().Length + PhotonNetwork.inRoom);
        //Debug.Log("Room List Count:" + PhotonNetwork.GetRoomList().Length);
        //waitingPanel.SetActive(false);
        foreach (GameObject temp in roomPrefabs)
        {
            Destroy(temp);
        }
        roomPrefabs.Clear();

        foreach (RoomInfo temp in PhotonNetwork.GetRoomList())
        {
            //waitingPanel.SetActive(true);
            ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = temp.CustomProperties;
            //Debug.Log("Room Created By:" + expectedCustomRoomProperties["ownername"].ToString());
            //Debug.Log("Room Created By ID:" + expectedCustomRoomProperties["ownerid"].ToString());
            //Debug.Log("Bet Amount:" + expectedCustomRoomProperties["bet"].ToString());
            //Debug.Log("Pool Id:" + expectedCustomRoomProperties["poolId"].ToString());
            //Debug.Log("isChallenge:" + expectedCustomRoomProperties["isChallenge"].ToString());
            if (expectedCustomRoomProperties["game"].ToString() == "checker" && expectedCustomRoomProperties["table"].ToString()==table && !bool.Parse(expectedCustomRoomProperties["isChallenge"].ToString()) && temp.PlayerCount < 2)
            {
                Debug.Log("here is it he " + expectedCustomRoomProperties["ownerid"].ToString());
                GameObject roomInfo = Instantiate(roomPrefab, roomParentObj.transform);
                roomPrefabs.Add(roomInfo);
                roomInfo.name = temp.Name;
                roomInfo.GetComponent<RoomDetailsCheckers>().playerId = expectedCustomRoomProperties["ownerid"].ToString();
                roomInfo.GetComponent<RoomDetailsCheckers>().playerName = expectedCustomRoomProperties["ownername"].ToString();
                roomInfo.GetComponent<RoomDetailsCheckers>().poolId = expectedCustomRoomProperties["poolId"].ToString();
                roomInfo.GetComponent<RoomDetailsCheckers>().betAmount = int.Parse(expectedCustomRoomProperties["bet"].ToString());
                roomInfo.GetComponent<RoomDetailsCheckers>().winningAmount = int.Parse(expectedCustomRoomProperties["winAmt"].ToString());
                roomInfo.GetComponent<RoomDetailsCheckers>().SetDetails();
                //roomInfo.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "USER NAME : " + expectedCustomRoomProperties["ownername"].ToString();

                if (expectedCustomRoomProperties["ownername"].ToString() == UserDetailsManager.userName)
                {
                    Debug.Log("OWNER ");
                    roomInfo.transform.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("joiner = = =");
                }
                if (PhotonNetwork.room != null)
                    roomInfo.transform.GetChild(1).GetComponent<Button>().interactable = false;
                //betAmount = int.Parse(expectedCustomRoomProperties["bet"].ToString());
                //roomInfo.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = "BET AMOUNT : " + betAmount;
                //roomInfo.transform.GetChild(2).GetComponent<Text>().text = "USER ID : "+ expectedCustomRoomProperties["ownerid"].ToString();
                //roomInfo.transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().text = "WINNING AMOUNT: " + GetWinAmount(betAmount);
            }
        }
    }

    public void OnPhotonRandomJoinFailed()
    {
        int betAmount = int.Parse(betAmountText.text);
        StartCoroutine(CreatePool());
    }

    public void OnJoinRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }

    public override void OnLeftRoom()
    {
    }

    public override void OnJoinedRoom()
    {
        if (gameStarted)
        {
            return;
        }
        Debug.Log("RoomJoined: " + PhotonNetwork.room.Name+"Player count: "+ PhotonNetwork.playerList.Length);
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            if (PhotonNetwork.playerList[i].UserId != PhotonNetwork.player.UserId)
            {
                Debug.Log("Opponent Joined Details:");
            }
            else
            {
                Debug.Log("Player Joined Details:");
            }
            Debug.Log("Photon Player UserId:" + PhotonNetwork.playerList[i].UserId);
            Debug.Log("Photon Player NickName:" + PhotonNetwork.playerList[i].NickName);
        }
        if (PhotonNetwork.playerList.Length == 2)
        {
            Debug.Log(PhotonNetwork.otherPlayers[0].CustomProperties["data"].ToString());
            PlayerData pd = JsonUtility.FromJson<PlayerData>(PhotonNetwork.otherPlayers[0].CustomProperties["data"].ToString());
            Debug.Log("opponent Name from hashTable: " + pd.playerName);
            Debug.Log("opponent Id from hashTable: " + pd.userid);
            playerUserId = pd.userid;
            playerName = pd.playerName;
            opponentName.text = pd.playerName;
            opponentNameonBoard.text= pd.playerName;
            opponentNameBoard10.text = pd.playerName;

            //loadingText.text = "LOADING GAME ...";
            loadingText.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(false);
            SearchOpponentPanel.SetActive(true);
            photonView.RPC("SendPoolDetail", PhotonTargets.All, poolId);
            Debug.LogError("OnROOM JOIN OPPONENT");
            isJoiner = true;
            board8.transform.eulerAngles = new Vector3(0, 0, 180);
            board10.transform.eulerAngles = new Vector3(0, 0, 180);
        }

        /*
        if (PhotonNetwork.isMasterClient)
            GameController.instance.isPlayerTurn = true;

       */
        //Debug.Log("playerName" + newPlayer.NickName);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("gameStarted" + gameStarted);
        if (gameStarted)
        {
            return;
        }
        // roomOwner = true;
        Debug.Log("OnCreatedRoom: \n Room Name:" + PhotonNetwork.room.Name + " RoomList Length:" + PhotonNetwork.GetRoomList().Length + " In Room:" + PhotonNetwork.inRoom);
        loadingText.text = "SEARCHING...";
        startGameButton.gameObject.SetActive(false);
        SearchOpponentPanel.SetActive(true);
        if (isChallenge)
        {
            hideNote = true;
            StartCoroutine(hidenotification());
            
        }
    }

    public IEnumerator TransactionPool(string winner, string looser)
    {
        Debug.Log("TransactionPool: " +poolId);
        WWWForm form = new WWWForm();
        form.AddField("looser_id", looser);
        form.AddField("winner_id", winner);
        form.AddField("game_id", "2"); /// 2 represents checkers game
        form.AddField("poolid", CheckersMultiplayer.Instance.poolId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "transaction", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();
        poolId = "";
        StartCoroutine(GetUserStats());
        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Transation Completed with error: " + www.error);
        }
        else
        {
            Debug.Log("Transation Completed Successfully" + www.downloadHandler.text);
        }
    }


    public IEnumerator DrawPool()
    {
        WWWForm form = new WWWForm();
      
        form.AddField("poolid", CheckersMultiplayer.Instance.poolId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "draw", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();
        poolId = "";
       // StartCoroutine(GetUserStats());
        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Transation Completed with error: " + www.error);
        }
        else
        {
            Debug.Log("Transation Completed Successfully" + www.downloadHandler.text);
        }
    }

    public void StopHideNotification()
    {
        hideNote = false;
        StopCoroutine(hidenotification());
    }
    
    IEnumerator hidenotification()
    {
        yield return new WaitForSecondsRealtime(120f);
        Debug.Log("Hiding Notification called!!" + hideNote);
        if (hideNote)
        {
            Debug.Log("Hiding Notification!!");
            isChallenge = false;
            loadingText.text = "Opponent Not Available!";
            Debug.Log("gameStarted" + gameStarted);
            if (!gameStarted)
            {
                DisconnectFromPhoton();
            }
            yield return new WaitForSeconds(2f);
            SearchOpponentPanel.SetActive(false);
        }           
    }

    int GetWinAmount(int bet)
    {
        int totalAmt = bet * 2;
        winAmt = totalAmt - Mathf.RoundToInt(UserDetailsManager.betServerPercent * float.Parse(totalAmt.ToString()));
        return winAmt;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {

        Debug.Log(PhotonNetwork.otherPlayers[0].CustomProperties["data"].ToString());

        Debug.Log("OnPhotonPlayerConnected() " + newPlayer.NickName); // not seen if you're the player connecting
        if (gameStarted && newPlayer.NickName == UserDetailsManager.userName)
        {
            Time.timeScale = 1f;
            WhotManager.instance.StopDisconnectTimer();
        }
        PlayerData pd = JsonUtility.FromJson<PlayerData>(PhotonNetwork.otherPlayers[0].CustomProperties["data"].ToString());
        Debug.Log("Name from hashTable: " + pd.playerName);
        Debug.Log("Id from hashTable: " + pd.userid);
        playerUserId = pd.userid;
        playerName = pd.playerName;
        opponentName.text = pd.playerName;
        opponentNameonBoard.text = pd.playerName;
        opponentNameBoard10.text = pd.playerName;
        if (PhotonNetwork.room.PlayerCount == 2)
        {
            Debug.Log("PlayerAc2 : " + newPlayer.NickName);
            Debug.Log("Player2 UserId: " + newPlayer.UserId);
            PhotonView pv = PhotonView.Get(photonView);
            pv.RPC("SendPoolDetail", PhotonTargets.Others, poolId);
            StartCoroutine(InternetChecker.Instance.checkInternetConnection((isConnected) =>
            {
                if (isConnected && SearchOpponentPanel.activeSelf)
                {
                    loadingText.gameObject.SetActive(false);
                    startGameButton.gameObject.SetActive(true);
                }
                else
                {
                    InternetChecker.Instance.DisplayInternetError();
                }
            }));
        }


        if (PhotonNetwork.isMasterClient)

        {
            
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected
        }
        

       

    }

    public void StartGame()
    {
        loadingText.gameObject.SetActive(true);
        if (isOpponentReady)
        {
            StartCoroutine(StartGameOnServer());
        }
        else
        {
            loadingText.text = "WAITING FOR OPPONENT RESPONSE ...";
            Debug.Log("WAITING FOR OPPONENT RESPONSE ...");
        }
        isPlayerReady = true;
        startGameButton.gameObject.SetActive(false);
        PhotonView pv = PhotonView.Get(photonView);
        pv.RPC("GameStartResponse", PhotonTargets.OthersBuffered, "1");
    }

    public IEnumerator StartGameOnServer()
    {
        Debug.Log("StartCheckersGame()");
        WWWForm form = new WWWForm();
        form.AddField("poolid", poolId);
        Debug.Log("poolid" + poolId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "startgame", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 120;
        yield return www.SendWebRequest();
        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Error in Start Game API: " + www.error);
            // Display Error Message
        }
        else
        {
            IsTurnPlayer = true;
          //  turnof = "opponent";
            Debug.Log("Start Game API Successful: " + www.downloadHandler.text);
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("currentplayer", UserDetailsManager.userId);
            photonView.RPC("OnGameStart", PhotonTargets.AllBuffered, jsonObj.ToString());
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.automaticallySyncScene = false;
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }
    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from photon");
        PhotonNetwork.ReconnectAndRejoin();
        //switchUser();
    }

    public void DisconnectFromPhoton()
    {
        CancelInvoke("TimeOutSearch");
        if (PhotonNetwork.isMasterClient)
            StartCoroutine(DeletePool());
        else 
            StartCoroutine(LeavePool());
        if (PhotonNetwork.inRoom)
        {
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
            PhotonNetwork.LeaveRoom();
        }
        if(gameStarted)
            StopDisconnectTimer();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer disconnectingplayer)
    {
        Debug.Log("Other player Disconnected: " + disconnectingplayer.NickName);
        if (gameStarted && OtherPlayerForceQuit)
           
        GameController.instance.ShowResultPanel("WIN !", true);
        else if (gameStarted && !OtherPlayerForceQuit)
        {
            Debug.Log("Player Disconnected .. no ForceQuit");
            if (!opponentDisconnectPopup.activeSelf)
            {
                Debug.Log("Other player Disconnected from photon .. starting disconnect Timer!!");
               StartDisconnectTimer();
            }
        }
        else
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "OPPONENT NOT READY!";
            DisconnectFromPhoton();
            Invoke("HideWaitForOpponent", 2f);
        }
    }

    public void HideWaitForOpponent()
    {
        SearchOpponentPanel.SetActive(false);
        WHOTMultiplayerManager.Instance.opponentName.text = "OPPONENT";
        WhotOpponent.instance.UpdateOpponentImage(WhotManager.instance.defaultImage);
    }

    public void StartDisconnectTimer()
    {
        Debug.Log("Starting Disconnect Timer!");
        disconnectTimer = 180;
        opponentDisconnectTimer.text = disconnectTimer.ToString();
        opponentDisconnectPopup.SetActive(true);
        Time.timeScale = 0f;
        StartCoroutine(UpdateDisconnectTimer());

    }
    public void StopDisconnectTimer()
    {
        //Time.timeScale = 1f;
        Debug.Log("Stopping Disconnect Timer!");
        opponentDisconnectPopup.SetActive(false);
        StopCoroutine(UpdateDisconnectTimer());
    }
    IEnumerator UpdateDisconnectTimer()
    {
        yield return new WaitForSecondsRealtime(1f);
        disconnectTimer -= 1;
        Debug.Log("Disconnect Timer: " + disconnectTimer);
        opponentDisconnectTimer.text = disconnectTimer.ToString();
        if (disconnectTimer > 0 && !resultDisplayed)
        {
            if (Time.timeScale == 0)
                StartCoroutine(UpdateDisconnectTimer());
        }
        else
        {
            GameController.instance.ShowResultPanel("WIN !",true);
            StopDisconnectTimer();
        }
    }

    IEnumerator checkPoolStatus()
    {
        
        WWWForm form = new WWWForm();

        form.AddField("poolid", poolId);
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

            if (www.downloadHandler.text.Contains("Room does not exists!"))
            {
                yield return null;
            }
            else if (Convert.ToBoolean(result["success"].ToString()))
            {
                if (result["userStatus"].ToString() == "Winner")
                {
                    //WhotManager.instance.AnnounceWinner(true);
                    Debug.Log("Player is Winner!!");
                }
                else if (result["userStatus"].ToString() == "Looser")
                {
                    //WhotManager.instance.AnnounceWinner(false);
                    Debug.Log("Player is Looser!!");
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
            if (gameStarted)
            {
                StartDisconnectTimer();
                Time.timeScale = 0f;
                opponentState = false;
                Debug.Log("Opponent Left!");
            }
        }
        else if (eventcode == 152)
        {
            if (playerState)
                Time.timeScale = 1f;
            StopDisconnectTimer();
            opponentState = true;
            Debug.Log("Opponent Joined Back!");
        }
        else if (eventcode == 153)
        {
            Debug.Log("Opponent Quit");

            GameController.instance.ShowResultPanel("WIN !",true);
            StopDisconnectTimer();
        }
    }


    void OnDestroy()
    {
        Instance = null;
        PhotonNetwork.OnEventCall -= this.OnEvent;

    }

    

    private void OnApplicationFocus(bool focus)
    {
        playerState = focus;
        if (!IsMultiPlayer)
            return;
        if (focus)
        {

            Debug.Log("Application resume");
            if (opponentState)
                Time.timeScale = 1f;
            connecting.SetActive(true);
            StartCoroutine(checkPoolStatus());
            if (PhotonNetwork.room != null)
            {

                connecting.SetActive(false);
                PhotonNetwork.RaiseEvent(152, 1, true, null);
                PhotonNetwork.SendOutgoingCommands();
            }


        }
        else
        {
            // playerState = false;
            Debug.Log("Application pause");
            connecting.SetActive(true);
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
    public int disconnectTimer;
    public Text opponentDisconnectTimer;
    public GameObject opponentDisconnectPopup;
    public override void OnJoinedLobby()
    {

        if (IsTableTen)
        {
            table = "table10";
        }
        else
        {
            table = "table8";

        }
        Debug.Log("OnJoinedLobby Called!");
        if (PhotonNetwork.insideLobby)
        {
            base.OnJoinedLobby();

            playButton.SetActive(true);
            PhotonNetwork.player.UserId = UserDetailsManager.userId;
            PhotonNetwork.player.NickName = UserDetailsManager.userName;
            Debug.Log("PhotonNetwork UserId: " + PhotonNetwork.player.UserId);
            Debug.Log("PhotonNetwork NickName: " + PhotonNetwork.player.NickName);

            PlayerData p = new PlayerData();
            p.playerName = UserDetailsManager.userName;
            p.userid = UserDetailsManager.userId;

            p.avatarTex = UserDetailsManager.userImageString;
          //  p.avatarCode = 1;


            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h.Add("data", JsonUtility.ToJson(p));
            PhotonNetwork.player.SetCustomProperties(h);

            connectToChat();
            if (gameStarted)
            {
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.PublishUserId = true;
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "ownername", "ownerid", "bet", "isAvailable", "appVer", "poolId", "isChallenge", "winAmt","game","table"};
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ownername", UserDetailsManager.userName }, { "ownerid", UserDetailsManager.userId }, { "bet", betAmount }, { "isAvailable", true }, { "appVer", Application.version }, { "poolId", poolId }, { "isChallenge", isChallenge }, { "winAmt", winAmt}, { "game", "checker" },{"table",table } };
                roomOptions.MaxPlayers = 2;
                roomOptions.IsVisible = true;
                roomOptions.IsOpen = true;

                PhotonNetwork.JoinOrCreateRoom(poolId, roomOptions, TypedLobby.Default);
            }
        }
    }


    public GameObject noPlayerFound;

    void TimeOutSearch()
    {
        CancelInvoke("TimeOutSearch");
        noPlayerFound.SetActive(true);
        //	Disconnect ();
    }

    public IEnumerator DeletePool()
    {
        Debug.Log("Delete Pool: " + poolId);
        betAmount = 0;
        winAmt = 0;
        isChallenge = false;
        gameStarted = false;
        canLeavePool = true;
        if (!string.IsNullOrEmpty(poolId))
        {            

            WWWForm form = new WWWForm();
            form.AddField("poolid", poolId);
            UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "deletepool", form);
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

            www.timeout = 15;
            yield return www.SendWebRequest();
            poolId = "";
            Debug.Log("Delete Pool Response: " + www.downloadHandler.text);
            var joinPoolDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            

            if (www.downloadHandler.text.Contains("error"))
            {
                Debug.Log("Error while trying to delete pool: " + www.error);
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = www.error;
                WhotUiManager.instance.errorPopup.SetActive(true);
            }
            else
            {
                //if (www.downloadHandler.text.Contains("success"))
               // {
                    Debug.Log("Deleted Pool Successfully!");
                    poolId = "";
               // }
                //else
              //  {
               //     Debug.Log("Delete Pool Response: " + www.downloadHandler.text);
               // }
            }
        }
    }

    IEnumerator LeavePool()
    {
        Debug.Log("Leave Pool: " + canLeavePool + poolId);
        if (!canLeavePool)
            StartCoroutine(DeletePool());
        if (!string.IsNullOrEmpty(poolId) && canLeavePool)
        {
            WWWForm form = new WWWForm();
            form.AddField("poolid", poolId);
            UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "leavepool", form);
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

            www.timeout = 15;
            yield return www.SendWebRequest();
            Debug.Log("Leave Pool Response: " + www.downloadHandler.text);
            var joinPoolDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            if (www.downloadHandler.text.Contains("error"))
            {
                Debug.Log("Error while trying to leave pool: " + www.error);
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = www.error;
                WhotUiManager.instance.errorPopup.SetActive(true);
            }
            else
            {
                if (www.downloadHandler.text.Contains("success"))
                {
                    Debug.Log("Left Pool Successfully!");
                    poolId = "";
                }
                else
                {
                    Debug.Log("Leave Pool Response: " + www.downloadHandler.text);
                }
            }
        }
    }
   

    public void OnUserSubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }
    public void YesOnExitPopClicked()
    {

        if (IsMultiPlayer)
        {
            if (!gameOver && gameStarted)
            {
                GameController.instance.ShowResultPanel("LOSE !", false);
               
                checkForceQuit();
                PhotonNetwork.LeaveRoom();
            }
            gameOver = true;
            //WHOTLultiplayerManager.instance.Disconnect ();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LeaveCurrentRoom()
    {
        CancelInvoke("TimeOutSearch");
        PhotonNetwork.LeaveRoom();
    }

    public void OpponentLeft()
    {
        if (!gameOver)
        {
            gameOver = true;
            GameController.instance.ShowResultPanel("WIN !", true);
        }
    }

    public void OnClose()
    {
        CancelInvoke("TimeOutSearch");

        isConnected = false;
        if (!gameOver && gameStarted)
        {
            gameOver = true;
            gamePlay.SetActive(false);
            if (OutOfLives.activeSelf)
            {
            }
            else
            {
                disconnectedPanel.SetActive(true);
            }
            WhotManager.instance.lostTokenAmount.text = "-" + (WHOTMultiplayerManager.wagerAmount).ToString();
        }
        else
        {
            WhotManager.instance.gameOver = true;
        }
    }

    public void OnMoveReceived(JSONArray jsonArray)
    {
        for (int i = 0; i < jsonArray.Count; i++)
        {
            WhotManager.instance.pileCardIds.Add(jsonArray[i].Value.ToString());
        }
        WhotManager.instance.AddCardsInPileForSecond();
    }

    public void SendData()
    {
        JSONObject jsonObj = new JSONObject();
        jsonObj.Add("flag", "1");
        jsonObj.Add("stage", "1");
        jsonObj.Add("currentplayer", WhotManager.instance.myTurn.ToString());

        JSONArray arr = new JSONArray();
        for (int i = 0; i < WhotManager.instance.pileCardIds.Count; i++)
        {
            arr.Add(WhotManager.instance.pileCardIds[i].ToString());
        }

        jsonObj.Add("list", arr);
        PhotonView pv = PhotonView.Get(photonView);
        pv.RPC("OnChat", PhotonTargets.Others, jsonObj.ToString());
    }


    public void SendMove(int type, string id, string color = "")
    {
        JSONObject jsonObj = new JSONObject();
        jsonObj.Add("flag", "1");
        jsonObj.Add("stage", "2");
        jsonObj.Add("type", type.ToString());
        jsonObj.Add("id", id.ToString());
        jsonObj.Add("color", color.ToString());
        jsonObj.Add("currentplayer", WhotManager.instance.myTurn.ToString());
        PhotonView pv = PhotonView.Get(photonView);
        pv.RPC("OnChat", PhotonTargets.OthersBuffered, jsonObj.ToString());
    }

    public void OnMoveReceived2(JSONObject obj)
    {
        try
        {
            if (WhotManager.instance.GetTurn())
                return;
            if (obj["type"].Value.ToString() == "1")
            {
                WhotOpponent.instance.transform.Find(obj["id"].Value.ToString()).GetComponent<Button>().onClick.Invoke();//.ThrowCard (cardName);
            }
            else if (obj["type"].Value.ToString() == "2")
            {
                WhotManager.instance.WithdrawCardForOpponent();
            }
            else if (obj["type"].Value.ToString() == "3")
            {
                WhotOpponent.instance.transform.Find(obj["id"].Value.ToString()).GetComponent<Button>().onClick.Invoke();//.ThrowCard (cardName);
                WhotManager.instance.WhotColorSelection(obj["color"].Value.ToString());
            }
        } catch (Exception e)
        {
            Debug.Log("Debug Exception :" + e);
        }
    }

    public IEnumerator GetUserStats()
    {
        string url = UserDetailsManager.serverUrl + "getuserstats";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
        www.timeout = 30;
        yield return www.SendWebRequest();

        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("result " + www.error + "Time: " + Time.time);
        }
        else
        {
            Debug.Log("User Stats Response: " + www.downloadHandler.text);
            var statsList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var userDetails = (IDictionary)statsList["result"];
            UserDetailsManager.userCoins = Mathf.RoundToInt(float.Parse(userDetails["coins"].ToString()));
            UserDetailsManager.userCoinsWon = int.Parse(userDetails["coins_won"].ToString());
          //  mobileNumText.text = UserDetailsManager.userPhone;
          //  CoinsWon.text = "Total Winnings: " + UserDetailsManager.userCoinsWon.ToString();
           // UpdateCoins();
        }
    }
    [PunRPC]
    public void OnGameStart(string str)
    {
        JSONObject jObject = (JSONObject)JSONObject.Parse(str);
        Debug.Log("Player Name: " + UserDetailsManager.userName);
        PlayingPlayerText.text = UserDetailsManager.userName;
        PlayingPlayerText10.text = UserDetailsManager.userName;

      

        // WhotConstants.isAI = false;
        CancelInvoke("TimeOutSearch");
        gameStarted = true;
        if (jObject["currentplayer"].Value.ToString() == UserDetailsManager.userId)
        {
            CheckersMultiplayer.Instance.myTurn = 0;
            myTurn = 0;
        }
        else
        {
            CheckersMultiplayer.Instance.myTurn = 0;

            myTurn = 1;
        }
        //Initiating Game!!
        StartCoroutine(GetUserStats());
        if(CheckersMultiplayer.Instance.IsTableTen)
        {
            if(IsMultiPlayer)
            {
                homeButttonMultiplayer.SetActive(true);
                homeButttonSinglePlayer.SetActive(!true);
            }
           else
            {
                homeButttonSinglePlayer.SetActive(true);
                homeButttonMultiplayer.SetActive(!true);

            }
            table10x10.SetActive(true);
            table8x8.SetActive(false);

        }
        else
        {
            table10x10.SetActive(false);

            table8x8.SetActive(true);
            if (IsMultiPlayer)
            {
                homeButttonMultiplayer.SetActive(true);
                homeButttonSinglePlayer.SetActive(!true);
            }
            else
            {
                homeButttonSinglePlayer.SetActive(true);
                homeButttonMultiplayer.SetActive(!true);

            }
        }
        mainCanvas.SetActive(false);
      //  WhotManager.instance.InitiateGame();
    }

    [PunRPC]
    public void GameStartResponse(string isReady)
    {
        if (isReady == "1")
        {
            isOpponentReady = true;
            loadingText.text = "OPPONENT READY!";
            loadingText.gameObject.SetActive(true);
            if (!PhotonNetwork.isMasterClient)
                startGameButton.gameObject.SetActive(true);
        }
    }

    public void SendProfile()
    {
        JSONObject jsonObj = new JSONObject();
        jsonObj.Add("flag", "3");
        jsonObj.Add("currentplayer", myTurn.ToString());
        PhotonView pv = PhotonView.Get(photonView);
        pv.RPC("OnChat", PhotonTargets.Others, jsonObj.ToString());
    }
    public string succesive;
    [PunRPC]
    public void OnRecieveSuccess(string succ)
    {
        Debug.Log("succes " + succ);
        succesive = succ;
    }

    [PunRPC]
    public void OnPlayerMoveReceive(int sendTo, int sendFrom,string turn)
    {
       // rpcTurn = turn;
        if (turn == "player")
        {
            Debug.LogError("player");
            GameController.instance.turn = GameController.Turn.playerTurn;

            GameController.instance.board.TileClicked(GameController.instance.board.allTiles[sendFrom].GetComponent<TileHandler>());
            GameController.instance.board.TileClicked(GameController.instance.board.allTiles[sendTo].GetComponent<TileHandler>());
            PhotonView pv = PhotonView.Get(CheckersMultiplayer.Instance.photonView);

            pv.RPC("onReceivedTurn", PhotonTargets.All, "opponent");

            /*
            if (succesive =="no")
            {
                pv.RPC("onReceivedTurn", PhotonTargets.All, "opponent");
            }
            if (succesive=="player")
            {
               

                pv.RPC("onReceivedTurn", PhotonTargets.All, "player");
            }
           */
            
            
        }
        if (turn == "opponent")
        {
           GameController.instance.turn = GameController.Turn.enemyTurn;

            Debug.LogError("opponent");
            GameController.instance.board.TileClickedOther(GameController.instance.board.allTiles[sendFrom].GetComponent<TileHandler>());
            GameController.instance.board.TileClickedOther(GameController.instance.board.allTiles[sendTo].GetComponent<TileHandler>());

            PhotonView pv = PhotonView.Get(CheckersMultiplayer.Instance.photonView);
            // pv.RPC("onReceivedTurn", PhotonTargets.All, "player");
            pv.RPC("onReceivedTurn", PhotonTargets.All, "player");

            /*
            if (succesive =="no")
            {
                pv.RPC("onReceivedTurn", PhotonTargets.All, "player");

            }
            if (succesive=="opponent")
            {
                pv.RPC("onReceivedTurn", PhotonTargets.All, "opponent");
            }
            */

            
            
            
        }




    }

    [PunRPC]
    public void OnPlayerMoveReceiveOther(int sendTo, int sendFrom)
    {
        Debug.Log("send other" + sendTo + "sendfrom other" + sendFrom);
        GameController.instance.board.TileClicked(GameController.instance.board.allTiles[sendFrom].GetComponent<TileHandler>());
        GameController.instance.board.TileClicked(GameController.instance.board.allTiles[sendTo].GetComponent<TileHandler>());

    }
    [PunRPC]
    public void OnChat(string str)
    {
        Debug.Log("RPC Hit: " + str);
        JSONObject jsonObject = (JSONObject)JSONObject.Parse(str);
        Debug.Log("chat received : " + jsonObject.ToString());
        if (jsonObject["flag"].Value.ToString() == "1" && jsonObject["stage"].Value.ToString() == "1" && myTurn.ToString() != jsonObject["currentplayer"].Value.ToString())
        {
            OnMoveReceived(jsonObject["list"].AsArray);
        }
        else if (jsonObject["flag"].Value.ToString() == "1" && jsonObject["stage"].Value.ToString() == "2" && myTurn.ToString() != jsonObject["currentplayer"].Value.ToString())
        {
            OnMoveReceived2(jsonObject);
        }
        else if (jsonObject["flag"].Value.ToString() == "2" && myTurn.ToString() != jsonObject["currentplayer"].Value.ToString())
        {
           
            GameController.instance.NextTurn();
        }
        else if (jsonObject["flag"].Value.ToString() == "3" && myTurn.ToString() != jsonObject["currentplayer"].Value.ToString())
        {
            //WhotManager.instance.SetOppoPro(jsonObject["url"].Value.ToString(), jsonObject["name"].Value.ToString());
            //JSONObject jsonObj = new JSONObject();
            ///jsonObj.Add("flag", "4");
            //jsonObj.Add ("name", WebServices.instance.currentUser.firstname.ToString ());
            //jsonObj.Add ("url", WebServices.instance.GetURL ());
            //jsonObj.Add("currentplayer", myTurn.ToString());

            //			Debug.Log ("sending profile : " + jsonObj.ToString ());
            //	mySocket.Emit ("chat", jsonObj.ToString());
            //PhotonView pv = PhotonView.Get(photonView);
            //pv.RPC("OnChat", PhotonTargets.OthersBuffered, jsonObj.ToString());

        }
       
    }

    public void onPrivateChat()
    {
        if (string.IsNullOrEmpty(chatInputText.text))
            return;

        PhotonView pv = PhotonView.Get(photonView);
        StopCoroutine(deactivateMyChatElement());
        StartCoroutine(deactivateMyChatElement());
        mychatText.gameObject.SetActive(true);
        chatInputText.gameObject.SetActive(false);
        pv.RPC("sendprivateChat", PhotonTargets.OthersBuffered, chatInputText.text);
        chatInputText.text = "";
    }

    IEnumerator deactivateMyChatElement()
    {
        yield return new WaitForSeconds(8f);
        mychatText.text = "";

        mychatText.gameObject.SetActive(false);
    }
    [PunRPC]
    void sendprivateChat(string chat)
    {
        StopCoroutine(deactivateChatElement());

        chatText.SetActive(true);
        chatText.transform.GetChild(0).gameObject.GetComponent<Text>().text = chat;
        StartCoroutine(deactivateChatElement());
    }

    IEnumerator deactivateChatElement()
    {
        yield return new WaitForSeconds(8f);
        chatText.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
        chatText.SetActive(false);
    }
    public void GoMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginSplash");
    }

    public void SendChangeTurn()
    {
        JSONObject jsonObj = new JSONObject();
        jsonObj.Add("flag", "2");
        jsonObj.Add("currentplayer", GameController.instance.myTurn.ToString());
        PhotonView pv = PhotonView.Get(photonView);
        pv.RPC("OnChat", PhotonTargets.OthersBuffered, jsonObj.ToString());
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        Debug.Log("notificationAYA");
        if (sender.Equals(UserDetailsManager.userName))
        {
            if (message.ToString().Contains("Join_Room"))
            {
                string msg = message.ToString().Split(';')[1];

                Debug.Log(msg);

            }

        }
    }
    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log("level" + level.ToString() + "/" + message);
        connectToChat();
    }

    public void OnDisconnected()
    {
        Debug.Log("Chat Disconnect");
    }

    public void OnChatStateChange(ChatState state)
    {

    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {

    }

    // public void OnPrivateMessage(string sender, object message, string channelName)
    // {
    //    throw new System.NotImplementedException();
    // }

    public void OnSubscribed(string[] channels, bool[] results)
    {

    }

    public void OnUnsubscribed(string[] channels)
    {

    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

    }
   
    #region CreatePool
    public IEnumerator CreatePool()
    {

        if (IsTableTen)
        {
            table = "table10";
        }
        else
        {
            table = "table8";

        }
        //string otherUserId = WhotOpponent.instance.userId;
        //Debug.Log("other User Id: " + otherUserId);
        string url = UserDetailsManager.serverUrl + "createpool";
        string uId = SystemInfo.deviceUniqueIdentifier;
        betAmount = int.Parse(betAmountText.text);
#if UNITY_ANDROID && !UNITY_EDITOR
        uId = UserDetailsManager.androidUnique();
#elif UNITY_IOS && !UNITY_EDITOR
        uId = Device.advertisingIdentifier;
#endif
        Debug.Log("checkers pool url: " + url);
        Debug.Log("bet Amount: " + betAmount);
        Debug.Log("device_id: " + uId);
        Debug.Log("Receiver: " + receiverName);
        if (PhotonNetwork.connected && PhotonNetwork.room == null)
        {
            WWWForm form = new WWWForm();
            form.AddField("bet", betAmount);
            //form.AddField("otheruser", otherUserId);
            form.AddField("device_id", uId);
            Debug.Log("Time1: " + Time.timeScale);

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
            www.timeout = 15;
            Debug.Log("Time2: " + Time.timeScale);
            yield return www.SendWebRequest();
            Debug.Log("Time3: " + Time.timeScale);
            Debug.Log("Pool Creation Response: " + www.downloadHandler.text);

            if (www.isNetworkError)
            {
                Debug.Log("Network Error!! " + www.error);
            }
            else
            {
                var roomList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
                if (www.downloadHandler.text.Contains("error"))
                {
                    Debug.Log("Error Occured: " + www.downloadHandler.text);
                }
                else
                {
                    //WHOTMultiplayerManager.instance.gameStarted = false;
                    isOpponentReady = false;
                    isPlayerReady = false;
                    var poolDetails = (IDictionary)roomList["result"];
                    poolId = poolDetails["poolid"].ToString();
                    RoomOptions roomOptions = new RoomOptions();
                    roomOptions.PublishUserId = true;
                    winAmt = int.Parse(poolDetails["winning_amount"].ToString());
                    roomOptions.CustomRoomPropertiesForLobby = new string[] { "ownername", "ownerid", "bet", "isAvailable", "appVer", "poolId", "isChallenge", "winAmt","game","table"};
                    roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ownername", UserDetailsManager.userName }, { "ownerid", UserDetailsManager.userId }, { "bet", betAmount }, { "isAvailable", true }, { "appVer", Application.version }, { "poolId", poolId }, { "isChallenge", isChallenge }, { "winAmt", winAmt }, { "game", "checker" }, {"table",table} };
                    //ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "bet", MAtchMakeString }, { "isAvailable", true }, { "appVer", Application.version } };
                    roomOptions.MaxPlayers = 2;
                    roomOptions.IsVisible = true;
                    roomOptions.IsOpen = true;
                    PhotonNetwork.CreateRoom(poolId, roomOptions, TypedLobby.Default);

                    if (isChallenge)
                    {

                        ChatGui.instance.sendPhotonNotification(receiverName, UserDetailsManager.userName, "sendChallenge;" + poolId + ";" + betAmount + ";" + winAmt);
                    }
                }
            }
        }
        else if(PhotonNetwork.room != null)
        {
            DisconnectFromPhoton();
        }
        else
        {
            GetPhotonToken();
        }
    }
    
    public void SendGameDetails(bool isWin)
    {
        if(isWin)
            photonView.RPC("SendGameOverDetail", PhotonTargets.AllBuffered, UserDetailsManager.userId, WhotOpponent.instance.userId);
        else
            photonView.RPC("SendGameOverDetail", PhotonTargets.AllBuffered, WhotOpponent.instance.userId, UserDetailsManager.userId);
    }

    [PunRPC]
    void SendPoolDetail(string _poolId)
    {
        Debug.Log("Received Pool Id " + _poolId);
        poolId = _poolId.ToString();
        //UserDetailsManager.userCoins -= betAmount;
        StartCoroutine(GetOtherPlayerDetails("user_dp"));
    }

    [PunRPC]
    void SendGameOverDetail(string _winnerId, string _loserId)
    {
        Debug.Log("Winner Id: "+ _winnerId);
        Debug.Log("Loser Id: " + _loserId);
        if (_winnerId == UserDetailsManager.userId)
            WhotManager.instance.AnnounceWinner(true);
        else
            WhotManager.instance.AnnounceWinner(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       
    }
    #endregion

    public IEnumerator GetOtherPlayerDetails(string field)
    {
        Debug.Log("Required Fields: " + field);
        WWWForm form = new WWWForm();
        form.AddField("userid", playerUserId);
        form.AddField("field", field);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "getuserdetail", form);
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
        www.timeout = 120;

        yield return www.SendWebRequest();
        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Opponent Details Error: " + www.error);
        }
        else
        {
            Debug.Log("Opponent Details Response: " + www.downloadHandler.text);
            var opponentDetailsResponse = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var opponentDetails = (IDictionary)opponentDetailsResponse["result"];
            try
            {
                if (!string.IsNullOrEmpty(opponentDetails["user_dp"].ToString()))
                {
                    string pic = opponentDetails["user_dp"].ToString();
                    byte[] Avtbytes = Convert.FromBase64String(pic);
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(Avtbytes);
                    WhotOpponent.instance.UpdateOpponentImage(tex);
                }
            }
            catch
            {
                Debug.Log("No Image Found, Use Default Img!s");
            }
        }
    }
    public void checkForceQuit()
    {
        photonView.RPC("CheckForceQuit", PhotonTargets.OthersBuffered, true);
    }
    [PunRPC]
    void CheckForceQuit(bool forceQuit)
    {
      OtherPlayerForceQuit = forceQuit;
    }

   
}