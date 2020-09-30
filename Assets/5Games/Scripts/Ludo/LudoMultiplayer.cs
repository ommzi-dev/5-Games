using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Ludo;
using UnityEngine.SceneManagement;
using SimpleJSON;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
public class LudoMultiplayer : Photon.PunBehaviour, IChatClientListener, IPunObservable
{
    public static LudoMultiplayer Instance;

    [Header("Gameplay Values")]
    public int myTurn, opponentsReady;
    public int winAmt, betAmount, playerCount = 2;
    public string matchMakingString = "Ludo";
    public string poolId, receiverName;
    public bool gameStarted, isPlayerReady, isConnected, hasJoinedLobby, canLeavePool, isChallenge;
    public bool roomOwner = false;
    public bool imReady = false;
    public bool opponentReady = false;
    public bool otherPlayerForceQuit = false;
    public bool gameOver = false;
    public bool fetchRoomCalled = false;
    public bool isOnlineMode = false;
    public List<int> disconnectedPlayersIndexList = new List<int>();

    [Header("UI Values")]
    public Text betAmountText, errorText;
    public Toggle players2sides1, players2sides2, players4;
    public GameObject errorDialog;
    public GameObject SearchOpponentPanel, Loader;
    public GameObject OutOfLives;
    public Texture defaultPlayerAvatar;
    public Text mychatText, opponentName, loadingText;
    public Button startGameButton;
    public InputField chatInputText;

    [Header("Script References")]
    public ChatClient chatClient;

    [Header("Photon Rooms")]
    [SerializeField] List<GameObject> ludoRoomPrefabs = new List<GameObject>();
    public GameObject ludoRoomPrefab, ludoRoomParentObj;
    bool hideNote = false;
    public GameObject[] hideObj;

    void Awake()
    {
        Debug.Log("LudoMultiplayer Awake Called");
        fetchRoomCalled = false;
        if (Instance == null)
        {
            Debug.Log("LudoMultiplayer Instance null");
            Instance = this;
            PhotonNetwork.PhotonServerSettings.AppID = UserDetailsManager.PhotonAppID;
            PhotonNetwork.PhotonServerSettings.ChatAppID = UserDetailsManager.PhotonChatID;
            DontDestroyOnLoad(gameObject);
        }
        else if(Instance != this)
        {
            Debug.Log("Destroying Ludo Multiplayer");
            //Destroy(gameObject);
        }
    }

    void Start()
    {
        GetPhotonToken();
        PhotonNetwork.OnEventCall += this.OnEvent;
        //PhotonNetwork.playerName = UserDetailsManager.userId;     //Set in PlayFab Manager on Authentication Success.
    }

    void Update()
    {
        if (isPlayerReady && opponentsReady == (PhotonNetwork.room.MaxPlayers -1) && !gameStarted)
        {
            loadingText.text = "Loading Game ...";
            isPlayerReady = false;
            opponentsReady = 0;

        }
    }

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
            Debug.Log("Photon Connected!");
            if(!fetchRoomCalled)
                StartCoroutine(FetchRoom());
        }
        //connectToChat();
        if (!ChatGui.instance.chatClient.CanChat)
        {
            ChatGui.instance.Connect();
        }
    }

    public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("Custom properties changed: " + propertiesThatChanged);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby Called!");
        if (PhotonNetwork.insideLobby)
        {
            base.OnJoinedLobby();

            //playButton.SetActive(true);
            PhotonNetwork.player.UserId = UserDetailsManager.userId;
            PhotonNetwork.player.NickName = UserDetailsManager.userName;
            Debug.Log("PhotonNetwork UserId: " + PhotonNetwork.player.UserId);
            Debug.Log("PhotonNetwork NickName: " + PhotonNetwork.player.NickName);

            PlayerData p = new PlayerData();
            p.playerName = UserDetailsManager.userName;
            p.userid = UserDetailsManager.userId;
            
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
            h.Add("data", JsonUtility.ToJson(p));
            PhotonNetwork.player.SetCustomProperties(h);

            //connectToChat();
            if (gameStarted)
            {
                Debug.Log("Game started!! : " + PhotonNetwork.player.UserId);
                RoomOptions roomOptions = new RoomOptions();
                
                roomOptions.PublishUserId = true;
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "ownername", "ownerid", "bet", "winAmt", "poolId", "isChallenge", "game", "players" };
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ownername", UserDetailsManager.userName }, { "ownerid", UserDetailsManager.userId }, { "bet", betAmount }, { "poolId", poolId }, { "isChallenge", isChallenge }, { "winAmt", winAmt }, { "game", "Ludo" }, {"players", playerCount } };
                roomOptions.MaxPlayers = 2;
                roomOptions.IsVisible = true;
                roomOptions.IsOpen = true;
                if(PhotonNetwork.room == null)
                    PhotonNetwork.JoinOrCreateRoom(poolId, roomOptions, TypedLobby.Default);
            }
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.player.UserId = UserDetailsManager.userId;
        Debug.Log("OnJoinedRoom: " + PhotonNetwork.room.Name + " " + PhotonNetwork.player.UserId);


        loadingText.text = "Searching...";
        if (!SearchOpponentPanel.activeSelf)
            SearchOpponentPanel.SetActive(true);

        if (PhotonNetwork.room.CustomProperties.ContainsKey("bt"))
        {
            Debug.Log("bt");
            extractBotMoves(PhotonNetwork.room.CustomProperties["bt"].ToString());
        }

        if (PhotonNetwork.room.CustomProperties.ContainsKey("fp"))
        {
            Debug.Log("fp");
            GameManager.Instance.firstPlayerInGame = int.Parse(PhotonNetwork.room.CustomProperties["fp"].ToString());
        }
        else
        {
            Debug.Log("fp =0");
            GameManager.Instance.firstPlayerInGame = 0;
        }

        GameManager.Instance.avatarOpponent = null;

        Debug.Log("Players in room " + PhotonNetwork.room.PlayerCount);

        GameManager.Instance.currentPlayersCount = 1;

        //GameManager.Instance.controlAvatars.setCancelButton();
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            GameManager.Instance.roomOwner = true;

        }
        else if (PhotonNetwork.room.PlayerCount >= PhotonNetwork.room.MaxPlayers )
        {
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
            if (!gameStarted && !PhotonNetwork.isMasterClient)
            {
                loadingText.gameObject.SetActive(false);
                startGameButton.gameObject.SetActive(true);
                SearchOpponentPanel.SetActive(true);
            }
        }

        if (!roomOwner)
        {
            GameManager.Instance.backButtonMatchPlayers.SetActive(false);

            for (int i = 0; i < PhotonNetwork.otherPlayers.Length; i++)                                                 
            {

                //int ii = i;
                int index = GetFirstFreeSlot();
                PlayerData pd = JsonUtility.FromJson<PlayerData>(PhotonNetwork.otherPlayers[i].CustomProperties["data"].ToString());
                GameManager.Instance.opponentsIDs[index] = pd.userid.ToString();
                GameManager.Instance.opponentsNames[index] = pd.playerName;
                StartCoroutine(GetOtherPlayerDetails(pd.userid.ToString(), index));
            }
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        roomOwner = true;

        Debug.Log("gameStarted" + gameStarted);
        if (gameStarted)
        {
            return;
        }
        GameManager.Instance.roomOwner = true;
        //GameManager.Instance.currentPlayersCount = 1;
        //GameManager.Instance.controlAvatars.updateRoomID(PhotonNetwork.room.Name);

        Debug.Log("OnCreatedRoom: \n Room Name:" + PhotonNetwork.room.Name + " RoomList Length:" + PhotonNetwork.GetRoomList().Length + " In Room:" + PhotonNetwork.room.Name);
        loadingText.text = "Searching...";
        startGameButton.gameObject.SetActive(false);
        //SearchOpponentPanel.SetActive(true);
        if (isChallenge)
        {
            hideNote = true;
            StartCoroutine(hidenotification());

        }
    }

    public void StopHideNotification()
    {
        hideNote = false;
        StopCoroutine(hidenotification());
    }
        

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom called");
        roomOwner = false;
        GameManager.Instance.roomOwner = false;
        GameManager.Instance.resetAllData();
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

    public void CreatePrivateRoom()
    {       
        GameManager.Instance.JoinedByID = false;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;


        string roomName = "";
        for (int i = 0; i < 8; i++)
        {
            roomName = roomName + UnityEngine.Random.Range(0, 10);
        }

        roomOptions.CustomRoomPropertiesForLobby = new String[] { "bet", "game" };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {
            { "bet", GameManager.Instance.payoutCoins},{ "game", "Ludo"}
         };
        Debug.Log("Private room name: " + roomName);
        PhotonNetwork.CreateRoom("", roomOptions, TypedLobby.Default);
    }

    public void WaitForNewPlayer()
    {
        if (PhotonNetwork.isMasterClient && GameManager.Instance.type != MyGameType.Private)
        {
            Debug.Log("START INVOKE");
            CancelInvoke("StartGameWithBots");
            Invoke("StartGameWithBots", StaticStrings.WaitTimeUntilStartWithBots);
        }
    }

    public void StartGame()
    {
        // while (!opponentReady || !imReady /*|| (!GameManager.Instance.roomOwner && !GameManager.Instance.receivedInitPositions)*/)
        // {
        //     yield return 0;
        // }
        Debug.Log("Start Game Called");
        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.room.IsVisible = false;

        CancelInvoke("StartGameWithBots");
        Invoke("StartGameScene", 0f);        
    }


    IEnumerator FetchRoom()
    {
        OnReceivedRoomListUpdate();
        fetchRoomCalled = true;
        yield return new WaitForSeconds(5f);
        StartCoroutine(FetchRoom());
    }

    public override void OnReceivedRoomListUpdate()
    {
        Debug.Log("Ludo Room List Count: " + PhotonNetwork.GetRoomList().Length);
        foreach (GameObject temp in ludoRoomPrefabs)
        {
            Destroy(temp);
        }
        ludoRoomPrefabs.Clear();

        foreach (RoomInfo temp in PhotonNetwork.GetRoomList())
        {
            Debug.Log("Room prop" + temp.CustomProperties);
            ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = temp.CustomProperties;
            //Debug.Log("Room Created By:" + expectedCustomRoomProperties["ownername"].ToString());
            //Debug.Log("Room Created By ID:" + expectedCustomRoomProperties["ownerid"].ToString());
            //Debug.Log("Bet Amount:" + expectedCustomRoomProperties["bet"].ToString());
            //Debug.Log("Pool Id:" + expectedCustomRoomProperties["poolId"].ToString());
            //Debug.Log("isChallenge:" + expectedCustomRoomProperties["isChallenge"].ToString());
            if (SceneManager.GetActiveScene().name != "LudoMenu")
                return;
            if (expectedCustomRoomProperties["game"].ToString()  == "Ludo" && !bool.Parse(expectedCustomRoomProperties["isChallenge"].ToString()) && temp.PlayerCount < temp.MaxPlayers)
            {
                GameObject roomInfo = Instantiate(ludoRoomPrefab, ludoRoomParentObj.transform);
                ludoRoomPrefabs.Add(roomInfo);
                roomInfo.name = temp.Name;
                roomInfo.GetComponent<LudoRoomDetails>().playerId = expectedCustomRoomProperties["ownerid"].ToString();
                roomInfo.GetComponent<LudoRoomDetails>().playerName = expectedCustomRoomProperties["ownername"].ToString();
                roomInfo.GetComponent<LudoRoomDetails>().poolId = expectedCustomRoomProperties["poolId"].ToString();
                roomInfo.GetComponent<LudoRoomDetails>().betAmount = int.Parse(expectedCustomRoomProperties["bet"].ToString());
                roomInfo.GetComponent<LudoRoomDetails>().winningAmount = int.Parse(expectedCustomRoomProperties["winAmt"].ToString());
                roomInfo.GetComponent<LudoRoomDetails>().playerCount = int.Parse(expectedCustomRoomProperties["players"].ToString());
                roomInfo.GetComponent<LudoRoomDetails>().SetDetails();
                //roomInfo.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "USER NAME : " + expectedCustomRoomProperties["ownername"].ToString();

                if (expectedCustomRoomProperties["ownername"].ToString() == UserDetailsManager.userName)
                {
                    roomInfo.transform.GetChild(1).gameObject.SetActive(false);
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

    private IEnumerator waitAndStartGame()
    {
        // while (!opponentReady || !imReady /*|| (!GameManager.Instance.roomOwner && !GameManager.Instance.receivedInitPositions)*/)
        // {
        //     yield return 0;
        // }
        while (GameManager.Instance.readyPlayers < GameManager.Instance.requiredPlayers - 1 || !imReady /*|| (!GameManager.Instance.roomOwner && !GameManager.Instance.receivedInitPositions)*/)
        {
            yield return 0;
        }
        StartGameScene();
        GameManager.Instance.readyPlayers = 0;
        opponentReady = false;
        imReady = false;
    }

    public void LoadGameScene()
    {
        GameManager.Instance.GameScene = "GameScene";

        if (!GameManager.Instance.gameSceneStarted)
        {
            SceneManager.LoadScene(GameManager.Instance.GameScene);
            GameManager.Instance.gameSceneStarted = true;
        }

    }   

    public void StartGameScene()
    {
        Debug.Log("StartGameScene");
        if (PhotonNetwork.room.PlayerCount >= PhotonNetwork.room.MaxPlayers || GameManager.Instance.currentPlayersCount >= GameManager.Instance.requiredPlayers || GameManager.Instance.type == MyGameType.Private)
        {
            Debug.Log("StartGameScene if()");
            LoadGameScene();

            if (GameManager.Instance.type == MyGameType.Private)
            {
                PhotonNetwork.RaiseEvent((int)EnumPhoton.BeginPrivateGame, null, true, null);
            }
            else
            {
                Debug.Log("Start Game Event raised!");
                PhotonNetwork.RaiseEvent((int)EnumPhoton.StartGame, null, true, null);
            }

        }
        else if(GameManager.Instance.offlineMode)
        {
            Debug.Log("StartGameScene else");
            if (PhotonNetwork.isMasterClient && GameManager.Instance.offlineMode)
            {
                CancelInvoke("StartGameWithBots");
                Invoke("StartGameWithBots", StaticStrings.WaitTimeUntilStartWithBots);
            }
        }
    }

    public void StartGameWithBots()
    {
        Debug.Log("Room: " + PhotonNetwork.room.Name);
        Debug.Log(PhotonNetwork.isMasterClient);
        Debug.Log(PhotonNetwork.player.UserId + " " + UserDetailsManager.userId);
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("Master Client");

            if (PhotonNetwork.room.PlayerCount < GameManager.Instance.requiredPlayers)
            {
                Debug.Log("Loading Bots");
                // PhotonNetwork.RaiseEvent((int)EnumPhoton.StartWithBots, null, true, null);
                LoadBots();
            }
        }
        else
        {
            Debug.Log("Not Master client");
        }
    }

    public void LoadBots()
    {
        Debug.Log("Close room - add bots");
        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.room.IsVisible = false;

        if (PhotonNetwork.isMasterClient)
        {
            Invoke("AddBots", 0f);
        }
        else
        {
            AddBots();
        }

    }

    public void AddBots()
    {
        // Add Bots here

        Debug.Log("Add Bots with delay");

        if (PhotonNetwork.room.PlayerCount < GameManager.Instance.requiredPlayers)
        {

            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.RaiseEvent((int)EnumPhoton.StartWithBots, null, true, null);
            }

            for (int i = 0; i < GameManager.Instance.requiredPlayers - 1; i++)
            {
                if (GameManager.Instance.opponentsIDs[i] == null)
                {
                    StartCoroutine(AddBot(i));
                }
            }
        }
    }


    public IEnumerator AddBot(int i)
    {
        yield return new WaitForSeconds(i + UnityEngine.Random.Range(0.0f, 0.9f));

        GameManager.Instance.opponentsAvatars[i] = defaultPlayerAvatar;
        GameManager.Instance.opponentsIDs[i] = "_BOT" + i;
        GameManager.Instance.opponentsNames[i] = "Guest" + UnityEngine.Random.Range(100000, 999999);
        Debug.Log("Name: " + GameManager.Instance.opponentsNames[i]);
        GameManager.Instance.controlAvatars.PlayerJoined(i, "_BOT" + i);
    }

    public string generateBotMoves()
    {
        // Generate BOT moves
        string BotMoves = "";
        int BotCount = 100;
        // Generate dice values
        for (int i = 0; i < BotCount; i++)
        {
            BotMoves += (UnityEngine.Random.Range(1, 7)).ToString();
            if (i < BotCount - 1)
            {
                BotMoves += ",";
            }
        }

        BotMoves += ";";

        // Generate delays
        float minValue = GameManager.Instance.playerTime / 10;
        if (minValue < 1.5f) minValue = 1.5f;
        for (int i = 0; i < BotCount; i++)
        {
            BotMoves += (UnityEngine.Random.Range(minValue, GameManager.Instance.playerTime / 8)).ToString();
            if (i < BotCount - 1)
            {
                BotMoves += ",";
            }
        }
        return BotMoves;
    }

    public void extractBotMoves(string data)
    {
        GameManager.Instance.botDiceValues = new List<int>();
        GameManager.Instance.botDelays = new List<float>();
        string[] d1 = data.Split(';');


        string[] diceValues = d1[0].Split(',');
        for (int i = 0; i < diceValues.Length; i++)
        {
            GameManager.Instance.botDiceValues.Add(int.Parse(diceValues[i]));
        }

        string[] delays = d1[1].Split(',');
        for (int i = 0; i < delays.Length; i++)
        {
            GameManager.Instance.botDelays.Add(float.Parse(delays[i]));
        }
        StartGameWithBots();
        Invoke("startGameScene", 1.0f);
    }

    public void CreateGameWithBots()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new String[] { "m", "v" };
        string BotMoves = generateBotMoves();

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {
            { "m", GameManager.Instance.mode.ToString() +  GameManager.Instance.type.ToString() + GameManager.Instance.payoutCoins.ToString()},
            {"bt", BotMoves},
            {"fp", UnityEngine.Random.Range(0, GameManager.Instance.requiredPlayers)}
         };

        Debug.Log("Create Room: " + GameManager.Instance.mode.ToString() + GameManager.Instance.type.ToString() + GameManager.Instance.payoutCoins.ToString());
        roomOptions.MaxPlayers = (byte)GameManager.Instance.requiredPlayers;
        roomOptions.IsVisible = false;
        //PhotonNetwork.JoinOrCreateRoom(UserDetailsManager.userName, roomOptions, TypedLobby.Default);
        StartCoroutine(TryToCreateGame(roomOptions));
    }    

    public IEnumerator TryToCreateGame(RoomOptions roomOptions)
    {
        while (true)
        {

            
            if (PhotonNetwork.insideLobby)
            {
                PhotonNetwork.JoinOrCreateRoom(UserDetailsManager.userName, roomOptions, TypedLobby.Default);       // USing Username as roomname for practice.
                
                break;
            }
            
            else
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    public void CreateRoomForLudo()
    {
        if (players4.isOn)
        {
            playerCount = 4;
            GameManager.Instance.sides = MySidesType.OneSide;
        }
        else
        {
            playerCount = 2;
            if(players2sides1.isOn)
                GameManager.Instance.sides = MySidesType.OneSide;
            else
                GameManager.Instance.sides = MySidesType.TwoSide;
        }
        JoinRoomAndStartGame();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
    }

    public void JoinRoomAndStartGame()
    {        
        Debug.Log("Bet Amt: " + betAmountText.text);
        
        if (string.IsNullOrEmpty(betAmountText.text) || string.IsNullOrWhiteSpace(betAmountText.text) || int.Parse(betAmountText.text) < 500)
        {
            errorText.text = "Bet Amount needs to be 500 or above.";
            errorDialog.SetActive(true);
            return;
        }
        else if (UserDetailsManager.userCoins < int.Parse(betAmountText.text))
        {
            errorText.text = "You don't have enough coins!!";
            errorDialog.SetActive(true);
            return;
        }
        else
        {
            if (PhotonNetwork.connected && PhotonNetwork.insideLobby)
            {
                betAmount = int.Parse(betAmountText.text);
            Debug.Log("UserDetailsManager.userCoins: " + UserDetailsManager.userCoins + " betAmount: " + betAmount);
            PhotonNetwork.player.NickName = UserDetailsManager.userName;
            PhotonNetwork.player.UserId = UserDetailsManager.userId;
            Debug.Log("PlayerId: " + PhotonNetwork.player.UserId);
            Debug.Log("PlayerCount: " + playerCount);

            if (playerCount == 4)
            {
                StaticStrings.isFourPlayerModeEnabled = true;
                GameManager.Instance.type = MyGameType.FourPlayer;
                GameManager.Instance.sides = MySidesType.OneSide;
            }
            else
            {
                StaticStrings.isFourPlayerModeEnabled = false;
                GameManager.Instance.type = MyGameType.TwoPlayer;
            }
            GameManager.Instance.offlineMode = false;
            GameManager.Instance.matchPlayerObject.GetComponent<SetMyData>().MatchPlayer();
            if (isChallenge)
                StartCoroutine(CreatePool());
            else
                CheckIfRoomAvailable();
            }
            else
            {
                GetPhotonToken();
                errorText.text = "Unable to Join room at the moment. Try again after sometime!!";
                errorDialog.SetActive(true);
            }
        }
    }

    void CheckIfRoomAvailable()
    {
        Debug.Log("PhotonNetwork.GetRoomList()" + PhotonNetwork.GetRoomList().Length);
        foreach (RoomInfo temp in PhotonNetwork.GetRoomList())
        {
            ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = temp.CustomProperties;
            if (expectedCustomRoomProperties["game"].ToString() == "Ludo" && !bool.Parse(expectedCustomRoomProperties["isChallenge"].ToString()) && expectedCustomRoomProperties["bet"].ToString() == betAmountText.text && int.Parse(expectedCustomRoomProperties["players"].ToString())  == playerCount)
            {
                Debug.Log("Kuch to mila" + temp.Name);
                LudoRoomDetails room = ludoRoomParentObj.transform.Find(temp.Name).GetComponent<LudoRoomDetails>();
                room.onJoin();
                return;
            }
        }
        StartCoroutine(CreatePool());
    }

    public IEnumerator CreatePool()
    {
          string url = UserDetailsManager.serverUrl + "createpool";
            string uId = SystemInfo.deviceUniqueIdentifier;
            betAmount = int.Parse(betAmountText.text);
#if UNITY_ANDROID && !UNITY_EDITOR
        uId = UserDetailsManager.androidUnique();
#elif UNITY_IOS && !UNITY_EDITOR
        uId = Device.advertisingIdentifier;
#endif
            Debug.Log("Ludo pool url: " + url);
            Debug.Log("bet Amount: " + betAmount);
            Debug.Log("device_id: " + uId);
            Debug.Log("Receiver: " + receiverName);
            if (PhotonNetwork.room == null)
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
                //Debug.Log("Time2: " + Time.timeScale);
                yield return www.SendWebRequest();
                //Debug.Log("Time3: " + Time.timeScale);
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
                        gameStarted = false;
                        opponentsReady = 0;
                        isPlayerReady = false;
                        var poolDetails = (IDictionary)roomList["result"];
                        poolId = poolDetails["poolid"].ToString();
                        RoomOptions roomOptions = new RoomOptions();
                        roomOptions.PublishUserId = true;
                        winAmt = int.Parse(poolDetails["winning_amount"].ToString());
                        Debug.Log("Photon Id:" + PhotonNetwork.player.UserId);
                        roomOptions.CustomRoomPropertiesForLobby = new string[] { "ownername", "ownerid", "bet", "poolId", "isChallenge", "winAmt", "game", "players" };
                        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ownername", UserDetailsManager.userName }, { "ownerid", UserDetailsManager.userId }, { "bet", betAmount }, { "poolId", poolId }, { "isChallenge", isChallenge }, { "winAmt", winAmt }, { "game", "Ludo" }, { "players", playerCount } };
                        //ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "bet", MAtchMakeString }, { "isAvailable", true }, { "appVer", Application.version } };
                        roomOptions.MaxPlayers = byte.Parse(playerCount.ToString());
                        roomOptions.IsVisible = true;
                        roomOptions.IsOpen = true;
                        PhotonNetwork.CreateRoom(poolId, roomOptions, TypedLobby.Default);

                        if (isChallenge)
                        {
                            ChatGui.instance.sendPhotonNotification(receiverName, UserDetailsManager.userName, "sendChallenge;" + poolId + ";" + betAmount + ";" + winAmt + ";" + "Ludo");
                            Debug.Log("Notification Sent!!");
                        }
                    }
                }
            }
    }

    
    [PunRPC]
    void SendPoolDetail(string _poolId)
    {
        Debug.Log("Received Pool Id " + _poolId);
        poolId = _poolId.ToString();
        //StartCoroutine(GetOtherPlayerDetails("user_dp"));
    }

    
    public IEnumerator GetOtherPlayerDetails(string userId, int index)
    {
        Debug.Log("Details of user: " + userId);
        string requiredFields = "user_dp";
        WWWForm form = new WWWForm();
        form.AddField("userid", userId);
        form.AddField("field", requiredFields);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "getuserdetail", form);
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
        www.timeout = 120;

        yield return www.SendWebRequest();
        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Opponent Details Error: " + www.error);
            GameManager.Instance.opponentsAvatars[index] = defaultPlayerAvatar;
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
                    GameManager.Instance.opponentsAvatars[index] = tex;
                    GameManager.Instance.controlAvatars.OppoAvatar[index].GetComponent<RawImage>().texture = tex;
                }
            }
            catch
            {
                GameManager.Instance.opponentsAvatars[index] = defaultPlayerAvatar;
                Debug.Log("No Image Found, Use Default Imgs");
            }
        }
    }

    public void checkForceQuit()
    {
        photonView.RPC("CheckForceQuit", PhotonTargets.OthersBuffered, true);
        PhotonNetwork.RaiseEvent(153, UserDetailsManager.userId, true, null);
        PhotonNetwork.SendOutgoingCommands();
    }

    [PunRPC]
    void CheckForceQuit(bool forceQuit)
    {
        otherPlayerForceQuit = forceQuit;
    }

    //Need to change the API url for this.
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
                errorDialog.GetComponent<PopUP>().title.text = "ERROR";
                errorDialog.GetComponent<PopUP>().msg.text = www.error;
                errorDialog.SetActive(true);
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

    //Need to change the API url for this.
    public IEnumerator LeavePool()
    {
        Debug.Log("Leave Pool: " + canLeavePool + poolId);
        if (!canLeavePool)
            StartCoroutine(DeletePool());
        if (!string.IsNullOrEmpty(poolId) && canLeavePool)
        {
            Debug.Log("Leave POol Api Hit!!");
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
                errorDialog.GetComponent<PopUP>().title.text = "ERROR";
                errorDialog.GetComponent<PopUP>().msg.text = www.error;
                errorDialog.SetActive(true);
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


    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from photon");
        //PhotonNetwork.ReconnectAndRejoin();
        //switchUser();
        if(SceneManager.GetActiveScene().name == "GameScene" && isOnlineMode)
        {
            errorText.text = "You are disconnected from game!!";
            errorDialog.SetActive(true);
            if (string.IsNullOrEmpty(GameGUIController.Instance.winnerId))
                GameGUIController.Instance.winnerId = "winner";
            GameGUIController.Instance.ShowGameFinishWindow();
        }
    }

    public void DisconnectFromPhoton()
    {
        CancelInvoke("TimeOutSearch");
        Debug.Log("Disconnect from Photon Called!!");
        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(DeletePool());
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;
                PhotonNetwork.LeaveRoom();
            }
        }
        else
        {
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.room.IsOpen = true;
                PhotonNetwork.room.IsVisible = true;
                PhotonNetwork.LeaveRoom();
            }
            StartCoroutine(LeavePool());
        }
        if (gameStarted)
            LudoGameController.Instance.StopDisconnectTimer();
    }

    public int GetFirstFreeSlot()
    {
        int index = 0;
        for (int i = 0; i < GameManager.Instance.opponentsIDs.Count; i++)
        {
            if (GameManager.Instance.opponentsIDs[i] == null)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log(PhotonNetwork.otherPlayers[0].CustomProperties["data"].ToString());
        Debug.Log("OnPhotonPlayerConnected() " + newPlayer.NickName); // not seen if you're the player connecting
        CancelInvoke("StartGameWithBots");
        PlayerData pd = JsonUtility.FromJson<PlayerData>(newPlayer.CustomProperties["data"].ToString());
        Debug.Log("New player joined " + pd.userid);
        Debug.Log("Players Count: " + PhotonNetwork.room.PlayerCount + " Max Players: " + PhotonNetwork.room.MaxPlayers );

        int index = GetFirstFreeSlot();

        GameManager.Instance.opponentsIDs[index] = pd.userid;
        GameManager.Instance.opponentsNames[index] = pd.playerName;
        StartCoroutine(GetOtherPlayerDetails(pd.userid.ToString(), index));

        GameManager.Instance.controlAvatars.PlayerJoined(index, pd.userid);
        if (PhotonNetwork.room.PlayerCount >= PhotonNetwork.room.MaxPlayers && !PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.room.IsVisible = false;
            if (!gameStarted)
            {
                loadingText.gameObject.SetActive(false);
                startGameButton.gameObject.SetActive(true);
                SearchOpponentPanel.SetActive(true);
            }
            
            
            PhotonView pv = PhotonView.Get(photonView);
            pv.RPC("SendPoolDetail", PhotonTargets.Others, poolId);
        }

        /*if (PhotonNetwork.room.PlayerCount > 1)
        {
            GameManager.Instance.controlAvatars.startButtonPrivate.GetComponent<Button>().interactable = true;
        }
        else
        {
            GameManager.Instance.controlAvatars.startButtonPrivate.GetComponent<Button>().interactable = false;
        }   */

        /*if(PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount >= PhotonNetwork.room.MaxPlayers)
        {
            Debug.Log("Starting Game!!");
            StartGameScene();
        }*/
    }

    

    public override void OnPhotonPlayerDisconnected(PhotonPlayer disconnectingplayer)
    {
        Debug.Log("Other player Disconnected: " + disconnectingplayer.UserId);
        if (!gameStarted)
        {
            Debug.Log("Resetting Room");
            if (startGameButton.gameObject.activeSelf)
                startGameButton.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(true);
            loadingText.text = "Opponent Not Ready!";
            DisconnectFromPhoton();
            Invoke("HideWaitForOpponent", 2f);
        }
        else
        {
            PlayerData pd = JsonUtility.FromJson<PlayerData>(disconnectingplayer.CustomProperties["data"].ToString());
            GameGUIController.Instance.PlayerDisconnected(disconnectingplayer);
            disconnectedPlayersIndexList.Add(GameGUIController.Instance.GetPlayerPosition(pd.userid));
        }
    }

    public void HideWaitForOpponent()
    {
        SearchOpponentPanel.SetActive(false);
        //opponentName.text = "OPPONENT";        
    }

    public void StartLudoGame()
    {
        loadingText.gameObject.SetActive(true);
        disconnectedPlayersIndexList.Clear();
        isOnlineMode = true;
        if (opponentsReady == PhotonNetwork.room.MaxPlayers-1)
        {
            StartCoroutine(StartLudoGameOnServer());
        }
        else
        {
            loadingText.text = "Waiting For Opponent Response...";
            Debug.Log("WAITING FOR OPPONENT RESPONSE ...");
        }
        isPlayerReady = true;
        startGameButton.gameObject.SetActive(false);
        PhotonView pv = PhotonView.Get(photonView);
        pv.RPC("LudoGameStartResponse", PhotonTargets.OthersBuffered, "1");
    }

    public IEnumerator StartLudoGameOnServer()
    {
        Debug.Log("StartLudoGame()");
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
            Debug.Log("Start Game API Successful: " + www.downloadHandler.text);
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("currentplayer", UserDetailsManager.userId);
            photonView.RPC("OnLudoGameStart", PhotonTargets.AllBuffered, jsonObj.ToString());
        }
    }

    #region Rpcs
    [PunRPC]
    public void OnLudoGameStart(string str)
    {
        gameStarted = true;
        GameManager.Instance.offlineMode = false;
                
        StartGameScene();
    }

    [PunRPC]
    public void LudoGameStartResponse(string isReady)
    {
        if (isReady == "1")
        {
            opponentsReady += 1;
            loadingText.text = "Opponent Ready!";
            loadingText.gameObject.SetActive(true);
            if (PhotonNetwork.isMasterClient && opponentsReady == (PhotonNetwork.room.MaxPlayers-1))
                startGameButton.gameObject.SetActive(true);
        }
    }
    //[PunRPC]
    //void sendprivateChat(string chat)
    //{
     //   StopCoroutine(deactivateChatElement());

     //   chatText.SetActive(true);
     //   chatText.transform.GetChild(0).gameObject.GetComponent<Text>().text = chat;
     //   StartCoroutine(deactivateChatElement());
    //}

    #endregion

    /*public void onPrivateChat()
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

    IEnumerator deactivateChatElement()
    {
        yield return new WaitForSeconds(8f);
        chatText.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
        chatText.SetActive(false);
    }

    IEnumerator deactivateMyChatElement()
    {
        yield return new WaitForSeconds(8f);
        mychatText.text = "";

        mychatText.gameObject.SetActive(false);
    }*/

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        if (GameManager.Instance.controlAvatars != null && GameManager.Instance.type == MyGameType.Private)
        {
            PhotonNetwork.LeaveRoom();
            GameManager.Instance.controlAvatars.ShowJoinFailed("Room closed");
        }
        else
        {
            if (newMasterClient.UserId == PhotonNetwork.player.UserId)
            {
                Debug.Log("Im new master client");
                WaitForNewPlayer();
            }
        }
    }

    // handle events:
    private void OnEvent(byte eventcode, object content, int senderid)
    {

        Debug.Log("Received event: " + (int)eventcode + " Sender ID: " + senderid);

        if (eventcode == (int)EnumPhoton.BeginPrivateGame)
        {
            //StartGame();
            LudoMultiplayer.Instance.LoadGameScene();
        }
        else if (eventcode == (int)EnumPhoton.StartWithBots && senderid != PhotonNetwork.player.ID)
        {
            LudoMultiplayer.Instance.LoadBots();
        }
        else if (eventcode == (int)EnumPhoton.StartGame)
        {
            //Invoke("LoadGameWithDelay", UnityEngine.Random.Range(1.0f, 5.0f));
            //PhotonNetwork.LeaveRoom();
            LudoMultiplayer.Instance.LoadGameScene();
        }
        else if (eventcode == (int)EnumPhoton.ReadyToPlay)
        {
            GameManager.Instance.readyPlayersCount++;            
        }
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        GameManager.Instance.controlAvatars.ShowJoinFailed("Room closed");
        base.OnPhotonJoinRoomFailed(codeAndMsg);
    }

    void OnDestroy()
    {
        PhotonNetwork.OnEventCall -= this.OnEvent;
    }

    public void destroy()
    {
        if (this.gameObject != null)
            DestroyImmediate(this.gameObject);
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnected()
    {
        throw new NotImplementedException();
    }

    public void OnConnected()
    {
        throw new NotImplementedException();
    }

    public void OnChatStateChange(ChatState state)
    {
        throw new NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        throw new NotImplementedException();
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        throw new NotImplementedException();
    }

    public void OnUnsubscribed(string[] channels)
    {
        throw new NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new NotImplementedException();
    }
}
