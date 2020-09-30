/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using Facebook.Unity;
using Photon;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Ludo
{
    public class GameGUIController : PunBehaviour
    {
        public static GameGUIController Instance;
        //public GameObject TIPButtonObject;
        public GameObject TIPObject;
        //public GameObject firstPrizeObject;
        //public GameObject SecondPrizeObject;
        //public GameObject firstPrizeText;
        //public GameObject secondPrizeText;
        public GameObject warningDialog;
        
        public AudioSource WinSound;
        public AudioSource myTurnSource;
        public AudioSource oppoTurnSource;
        private bool AllPlayersReady = false;
        // LUDO
        public MultiDimensionalGameObject[] PlayersPawns;
        //public GameObject[] PlayersDices;
        public GameObject[] HomeLockObjects;

        public Text userNameText, coinsText;

        [System.Serializable]
        public class MultiDimensionalGameObject
        {
            public GameObject[] objectsArray;
        }

        public GameObject ludoBoard;
        public GameObject[] diceBackgrounds;
        public MultiDimensionalGameObject[] playersPawnsColors;
        public MultiDimensionalGameObject[] playersPawnsMultiple;
        private Color colorRed = new Color(250.0f / 255.0f, 12.0f / 255, 12.0f / 255);
        private Color colorBlue = new Color(0, 86.0f / 255, 255.0f / 255);
        private Color colorYellow = new Color(255.0f / 255.0f, 163.0f / 255, 0);
        private Color colorGreen = new Color(8.0f / 255, 174.0f / 255, 30.0f / 255);


        // END LUDO

        public GameObject GameFinishWindow;
        public GameObject ScreenShotController;
        public GameObject invitiationDialog;
        public GameObject addedFriendWindow;
        public GameObject PlayerInfoWindow;
        public GameObject ChatWindow;
        public GameObject ChatButton;
        private bool SecondPlayerOnDiagonal = true;

        private List<string> PlayersIDs;
        public GameObject[] Players;
        public Text[] PlayerNames;
        //public GameObject[] PlayersTimers;
        public GameObject[] PlayersChatBubbles;
        public GameObject[] PlayersChatBubblesText;
        public GameObject[] PlayersChatBubblesImage;
        private GameObject[] ActivePlayers;
        //public GameObject[] PlayersAvatarsButton;

        private List<Texture> avatars;
        private List<string> names;

        public List<PlayerObject> playerObjects;
        private int myIndex;
        private string myId;
        public string winnerId;


        private Color[] borderColors = new Color[4] { Color.yellow, Color.green, Color.red, Color.blue };

        private int currentPlayerIndex;

        private int ActivePlayersInRoom;

        private Texture[] emojiSprites;

        private string CurrentPlayerID;

        private List<PlayerObject> playersFinished = new List<PlayerObject>();


        public bool iFinished = false;
        private bool FinishWindowActive = false;

        private int firstPlacePrize;
        private int secondPlacePrize;

        private int requiredToStart = 0;

        // Use this for initialization
        void Start()
        {
            if (Instance == null)
                Instance = this;

            userNameText.text = UserDetailsManager.userName;
            coinsText.text = UserDetailsManager.userCoins.ToString();
            StartCoroutine(GetUserStats());
            //requiredToStart = GameManager.Instance.requiredPlayers;
            requiredToStart = PhotonNetwork.room.MaxPlayers;

            if (GameManager.Instance.sides == MySidesType.TwoSide)
            {
                SecondPlayerOnDiagonal = false;
            }

            PhotonNetwork.RaiseEvent((int)EnumPhoton.ReadyToPlay, 0, true, null);

            // LUDO
            // Rotate board and set colors
            int rotation = UnityEngine.Random.Range(0, 1); // Range was btw (0,4) earlier.. changed to (0,1) on 13th March 2020

            Color[] colors = null;

            if (rotation == 0)
            {
                colors = new Color[] { colorYellow, colorGreen, colorRed, colorBlue };
            }
            else if (rotation == 1)
            {
                colors = new Color[] { colorBlue, colorYellow, colorGreen, colorRed };
                ludoBoard.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, -90.0f);
            }
            else if (rotation == 2)
            {
                colors = new Color[] { colorRed, colorBlue, colorYellow, colorGreen };
                ludoBoard.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, -180.0f);
            }
            else
            {
                colors = new Color[] { colorGreen, colorRed, colorBlue, colorYellow };
                ludoBoard.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, -270.0f);
            }

            for (int i = 0; i < diceBackgrounds.Length; i++)
            {
                diceBackgrounds[i].GetComponent<Image>().color = colors[i];
            }

            for (int i = 0; i < playersPawnsColors.Length; i++)
            {
                for (int j = 0; j < playersPawnsColors[i].objectsArray.Length; j++)
                {
                    playersPawnsColors[i].objectsArray[j].GetComponent<Image>().color = colors[i];
                    playersPawnsMultiple[i].objectsArray[j].GetComponent<Image>().color = colors[i];
                }
            }
            // END LUDO

            // Update player data in playfab
            //Dictionary<string, string> data = new Dictionary<string, string>();
            //data.Add(LudoPlayerData.CoinsKey, (GameManager.Instance.myPlayerData.GetCoins() - GameManager.Instance.payoutCoins).ToString());
            //data.Add(LudoPlayerData.GamesPlayedKey, (GameManager.Instance.myPlayerData.GetPlayedGamesCount() + 1).ToString());
            //GameManager.Instance.myPlayerData.UpdateUserData(data);

            currentPlayerIndex = 0;
            emojiSprites = new Texture[] { UserDetailsManager.userImageTexture };
            myId = UserDetailsManager.userId;
            playerObjects = new List<PlayerObject>();
            avatars = GameManager.Instance.opponentsAvatars;
            avatars.Insert(0, UserDetailsManager.userImageTexture);

            names = GameManager.Instance.opponentsNames;
            names.Insert(0, UserDetailsManager.userName);

            PlayersIDs = new List<string>();
            PlayersIDs.Insert(0, UserDetailsManager.userId);
            for (int i = 0; i < GameManager.Instance.opponentsIDs.Count; i++)
            {
                if (GameManager.Instance.opponentsIDs[i] != null)
                {
                    PlayersIDs.Add(GameManager.Instance.opponentsIDs[i]);
                }
            }
            //PlayersIDs.Insert(0, UserDetailsManager.userId);

            for (int i = 0; i < PlayersIDs.Count; i++)
            {
                Debug.Log(i + " PlayerID: " + PlayersIDs[i]);
                playerObjects.Add(new PlayerObject(names[i], PlayersIDs[i], avatars[i]));
            }                        
            
            ActivePlayersInRoom = PlayersIDs.Count;

            if (PlayersIDs.Count == 2)
            {
                if (GameManager.Instance.sides == MySidesType.OneSide)
                {
                    ActivePlayers = new GameObject[2];
                    ActivePlayers[0] = Players[0];
                    ActivePlayers[1] = Players[2];
                    Players[1].SetActive(false);
                    Players[3].SetActive(false);
                    PlayerNames[1].gameObject.SetActive(false);
                    PlayerNames[3].gameObject.SetActive(false);
                    playerObjects[0].index = 0;
                    playerObjects[1].index = 2;
                    Debug.Log("One Sided GamePlay");
                    //Disabling Pawns for 2 sides:
                    for (int k = 0; k < PlayersPawns[1].objectsArray.Length; k++)
                    {
                        PlayersPawns[1].objectsArray[k].SetActive(false);
                    }

                    for (int k = 0; k < PlayersPawns[3].objectsArray.Length; k++)
                    {
                        PlayersPawns[3].objectsArray[k].SetActive(false);
                    }
                }
                else
                {                    
                    // END LUDO
                    Players[2].SetActive(false);
                    Players[3].SetActive(false);
                    ActivePlayers = new GameObject[2];
                    ActivePlayers[0] = Players[0];
                    ActivePlayers[1] = Players[1];
                    playerObjects[0].index = 0;
                    playerObjects[1].index = 1;
                }
            }
            else
            {
                ActivePlayers = Players;
            }

            int startPos = 0;
            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i].id == UserDetailsManager.userId)
                {
                    startPos = i;
                    break;
                }
            }
            int index = 0;
            bool addedMe = false;
            myIndex = startPos;
            GameManager.Instance.myPlayerIndex = myIndex;
            for (int i = startPos; ;)
            {
                if (i == startPos && addedMe) break;

                if (PlayersIDs.Count == 2)
                {
                    int r = playerObjects[i].index;
                    if (addedMe)
                    {
                        Debug.Log("other player Index: " + r);
                        playerObjects[i].timer = Players[r].GetComponent<PlayerAvatarController>().Timer;//2 Replaced 2 with i on RHS 
                        playerObjects[i].ChatBubble = PlayersChatBubbles[r];//2 Replaced 2 with i on RHS 
                        playerObjects[i].ChatBubbleText = PlayersChatBubblesText[r];//2 Replaced 2 with i on RHS 
                        playerObjects[i].ChatbubbleImage = PlayersChatBubblesImage[r];//2 Replaced 2 with i on RHS 
                        playerObjects[i].AvatarObject = ActivePlayers[i];
                        string id = playerObjects[i].id;
                        Players[r].GetComponent<PlayerAvatarController>().name = playerObjects[i].name;
                        Players[r].GetComponent<PlayerAvatarController>().Name.transform.GetComponent<Text>().text = playerObjects[i].name;
                        Players[r].GetComponent<PlayerAvatarController>().Avatar.GetComponent<RawImage>().texture = LudoMultiplayer.Instance.defaultPlayerAvatar;
                        //PlayersAvatarsButton[i].GetComponent<Button>().onClick.RemoveAllListeners();//2 Replaced 2 with i on LHS 
                        //PlayersAvatarsButton[i].GetComponent<Button>().onClick.AddListener(() => ButtonClick(id));//2 Replaced 2 with i on LHS 

                        // LUDO
                        playerObjects[i].dice = Players[r].GetComponent<PlayerAvatarController>().Dice;//2 Replaced 2 with i on RHS 
                        playerObjects[i].dice.GetComponent<GameDiceController>().isMyDice = false;
                        playerObjects[i].dice.SetActive(true);
                        if (GameManager.Instance.sides == MySidesType.TwoSide)
                        {
                            int j = 0;
                            if (i == 0)
                                j = 3;
                            else
                                j = 1;

                            var p = PlayersPawns[i].objectsArray.Concat(PlayersPawns[i+j].objectsArray);
                            playerObjects[i].pawns = p.ToArray();//2 Replaced 2 with i on RHS
                            PlayerNames[i].text = playerObjects[i].name;
                            PlayerNames[i + j].text = playerObjects[i].name;
                        }
                        else
                        {
                            if (i == 0)
                            {
                                playerObjects[i].pawns = PlayersPawns[i].objectsArray;
                                PlayerNames[0].text = playerObjects[i].name;
                            }
                            else
                            {
                                playerObjects[i].pawns = PlayersPawns[2].objectsArray;
                                PlayerNames[2].text = playerObjects[i].name;
                            }
                        }
                        Debug.Log("Opponent Pawns:" + playerObjects[i].pawns.Length);
                        for (int k = 0; k < playerObjects[i].pawns.Length; k++)
                        {
                            playerObjects[i].pawns[k].GetComponent<LudoPawnController>().setPlayerIndex(i);
                        }
                        playerObjects[i].homeLockObjects = HomeLockObjects[i];//2 Replaced 2 with i on RHS 
                        // END LUDO
                    }
                    else
                    {
                        Debug.Log("my Index: " + i);
                        GameManager.Instance.myPlayerIndex = i;
                        playerObjects[i].timer = Players[r].GetComponent<PlayerAvatarController>().Timer;//replaced index by i RHS
                        playerObjects[i].ChatBubble = PlayersChatBubbles[r];//replaced index by i RHS
                        playerObjects[i].ChatBubbleText = PlayersChatBubblesText[r];//replaced index by i RHS
                        playerObjects[i].ChatbubbleImage = PlayersChatBubblesImage[r];//replaced index by i RHS
                        string id = playerObjects[i].id;
                        //playerObjects[i].index = i;
                        playerObjects[i].AvatarObject = ActivePlayers[r];
                        Players[r].GetComponent<PlayerAvatarController>().name = UserDetailsManager.userName;
                        Players[r].GetComponent<PlayerAvatarController>().Name.transform.GetComponent<Text>().text = UserDetailsManager.userName;
                        Debug.Log("Player Name: " + Players[r].GetComponent<PlayerAvatarController>().name);
                        Players[r].GetComponent<PlayerAvatarController>().Avatar.GetComponent<RawImage>().texture = UserDetailsManager.userImageTexture;
                        // LUDO
                        playerObjects[i].dice = Players[r].GetComponent<PlayerAvatarController>().Dice;//replaced index by i RHS
                        playerObjects[i].dice.GetComponent<GameDiceController>().isMyDice = true;
                        playerObjects[i].dice.SetActive(true);
                        if (GameManager.Instance.sides == MySidesType.TwoSide)
                        {
                            int j = 0;
                            if (i == 0)
                                j = 3;
                            else
                                j = 1;

                            var p = PlayersPawns[i].objectsArray.Concat(PlayersPawns[i+j].objectsArray);
                            playerObjects[i].pawns = p.ToArray();//2 Replaced 2 with i on RHS
                            PlayerNames[i].text = "Me";
                            PlayerNames[i + j].text = "Me";
                        }
                        else
                        {
                            if (i == 0)
                            {
                                playerObjects[i].pawns = PlayersPawns[i].objectsArray;
                                PlayerNames[0].text = playerObjects[i].name;
                            }
                            else
                            {
                                playerObjects[i].pawns = PlayersPawns[2].objectsArray;
                                PlayerNames[2].text = playerObjects[i].name;
                            }

                        }
                        Debug.Log("My Pawns:" + playerObjects[i].pawns.Length);
                        for (int k = 0; k < playerObjects[i].pawns.Length; k++)
                        {
                            playerObjects[i].pawns[k].GetComponent<LudoPawnController>().setPlayerIndex(i);
                        }
                        playerObjects[i].homeLockObjects = HomeLockObjects[i];//replaced index by i RHS
                        // END LUDO
                    }
                }
                else
                {
                    playerObjects[i].timer = Players[i].GetComponent<PlayerAvatarController>().Timer;//2 Replaced 2 with i on RHS 
                    playerObjects[i].ChatBubble = PlayersChatBubbles[i];//2 Replaced 2 with i on RHS 
                    playerObjects[i].ChatBubbleText = PlayersChatBubblesText[i];//2 Replaced 2 with i on RHS 
                    playerObjects[i].ChatbubbleImage = PlayersChatBubblesImage[i];//2 Replaced 2 with i on RHS 
                    //playerObjects[i].index = i;
                    playerObjects[i].AvatarObject = ActivePlayers[i];
                    string id = playerObjects[i].id;
                    Players[i].GetComponent<PlayerAvatarController>().name = playerObjects[i].name;
                    Players[i].GetComponent<PlayerAvatarController>().Name.transform.GetComponent<Text>().text = playerObjects[i].name;
                    //PlayersAvatarsButton[i].GetComponent<Button>().onClick.RemoveAllListeners();//2 Replaced 2 with i on LHS 
                    //PlayersAvatarsButton[i].GetComponent<Button>().onClick.AddListener(() => ButtonClick(id));//2 Replaced 2 with i on LHS 

                    // LUDO
                    playerObjects[i].dice = Players[i].GetComponent<PlayerAvatarController>().Dice;//2 Replaced 2 with i on RHS 
                    if (playerObjects[i].id == UserDetailsManager.userId)
                    {
                        playerObjects[i].dice.GetComponent<GameDiceController>().isMyDice = true;
                        Players[i].GetComponent<PlayerAvatarController>().Avatar.GetComponent<RawImage>().texture = UserDetailsManager.userImageTexture;
                    }
                    else
                    {
                        playerObjects[i].dice.GetComponent<GameDiceController>().isMyDice = false;
                        //if(addedMe)
                        //    Players[i].GetComponent<PlayerAvatarController>().Avatar.GetComponent<RawImage>().texture = GameManager.Instance.opponentsAvatars[i-1];
                        //else if(GameManager.Instance.opponentsAvatars.Count > i)
                            Players[i].GetComponent<PlayerAvatarController>().Avatar.GetComponent<RawImage>().texture = LudoMultiplayer.Instance.defaultPlayerAvatar;
                    }
                    playerObjects[i].dice.SetActive(true);
                    
                    playerObjects[i].pawns = PlayersPawns[i].objectsArray;//2 Replaced 2 with i on RHS 
                    Debug.Log("Opponent Pawns:" + playerObjects[i].pawns.Length);
                    for (int k = 0; k < playerObjects[i].pawns.Length; k++)
                    {
                        playerObjects[i].pawns[k].GetComponent<LudoPawnController>().setPlayerIndex(i);
                    }
                    playerObjects[i].homeLockObjects = HomeLockObjects[i];//2 Replaced 2 with i on RHS 

                    playerObjects[i].timer = Players[i].GetComponent<PlayerAvatarController>().Timer;
                    playerObjects[i].ChatBubble = PlayersChatBubbles[i];
                    playerObjects[i].ChatBubbleText = PlayersChatBubblesText[i];
                    playerObjects[i].ChatbubbleImage = PlayersChatBubblesImage[i];

                    // LUDO
                    /*playerObjects[i].dice = PlayersDices[index];
                    PlayersDices[i].SetActive(true);
                    playerObjects[i].pawns = PlayersPawns[index].objectsArray;

                    for (int k = 0; k < playerObjects[i].pawns.Length; k++)
                    {
                        playerObjects[i].pawns[k].GetComponent<LudoPawnController>().setPlayerIndex(i);
                    }
                    playerObjects[i].homeLockObjects = HomeLockObjects[index];
                    // END LUDO

                    string id = playerObjects[i].id;
                    if (index != 0)
                    {
                        PlayersAvatarsButton[index].GetComponent<Button>().onClick.RemoveAllListeners();
                        PlayersAvatarsButton[index].GetComponent<Button>().onClick.AddListener(() => ButtonClick(id));
                    }*/
                }

                //playerObjects[i].AvatarObject = ActivePlayers[index];
                //ActivePlayers[index].GetComponent<PlayerAvatarController>().Name.GetComponent<Text>().text = playerObjects[i].name;
                if (playerObjects[i].avatar != null)
                {
                   // ActivePlayers[index].GetComponent<PlayerAvatarController>().Avatar.GetComponent<RawImage>().texture = playerObjects[i].avatar;
                }

                index++;

                if (i < PlayersIDs.Count - 1)
                {
                    i++;
                }
                else
                {
                    i = 0;
                }

                addedMe = true;
            }

            currentPlayerIndex = GameManager.Instance.firstPlayerInGame;
            GameManager.Instance.currentPlayer = playerObjects[currentPlayerIndex];

            // SetTurn();

            // // if (myIndex == 0)
            // // {
            // //     SetMyTurn();
            // //     playerObjects[0].dice.GetComponent<GameDiceController>().DisableDiceShadow();
            // // }
            // // else
            // // {
            // //     SetOpponentTurn();
            // //     playerObjects[currentPlayerIndex].dice.GetComponent<GameDiceController>().DisableDiceShadow();
            // // }
            GameManager.Instance.playerObjects = playerObjects;

            // // Check if all players are still in room - if not deactivate
            // for (int i = 0; i < playerObjects.Count; i++)
            // {
            //     bool contains = false;
            //     if (!playerObjects[i].id.Contains("_BOT"))
            //     {
            //         for (int j = 0; j < PhotonNetwork.playerList.Length; j++)
            //         {
            //             if (PhotonNetwork.playerList[j].NickName.Equals(playerObjects[i].id))
            //             {
            //                 contains = true;
            //                 break;
            //             }
            //         }

            //         if (!contains)
            //         {
            //             setPlayerDisconnected(i);
            //         }
            //     }
            // }

            // CheckPlayersIfShouldFinishGame();
                        
           
            // LUDO
            // Enable home locks
            if (GameManager.Instance.mode == MyGameMode.Quick || GameManager.Instance.mode == MyGameMode.Master)
            {
                for (int i = 0; i < GameManager.Instance.playerObjects.Count; i++)
                {
                    GameManager.Instance.playerObjects[i].homeLockObjects.SetActive(true);
                }
                GameManager.Instance.needToKillOpponentToEnterHome = true;
            }
            else
            {
                GameManager.Instance.needToKillOpponentToEnterHome = false;
            }
            //GameManager.Instance.needToKillOpponentToEnterHome = false;

            // END LUDO

            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i].id.Contains("_BOT"))
                {
                    GameManager.Instance.readyPlayersCount++;
                }
            }

            GameManager.Instance.playerObjects = playerObjects;

            // Check if all players are still in room - if not deactivate
            for (int i = 0; i < playerObjects.Count; i++)
            {
                bool contains = false;
                if (!playerObjects[i].id.Contains("_BOT"))
                {
                    for (int j = 0; j < PhotonNetwork.playerList.Length; j++)
                    {
                        PlayerData pd = JsonUtility.FromJson<PlayerData>(PhotonNetwork.playerList[j].CustomProperties["data"].ToString());
                        Debug.Log("j: " + pd.userid + " Name: " + pd.playerName);
                        if (pd.userid.Equals(playerObjects[i].id))
                        {
                            GameManager.Instance.readyPlayersCount++;
                            contains = true;
                            break;
                        }
                    }

                    if (!contains)
                    {
                        GameManager.Instance.readyPlayersCount--;
                        Debug.Log("Ready players: " + i + " " + GameManager.Instance.readyPlayersCount);
                        setPlayerDisconnected(i);
                    }
                }
            }

            CheckPlayersIfShouldFinishGame();     
            StartCoroutine(waitForPlayersToStart());
        }

        public void ShowMessageDialog(string title, string message)
        {
            warningDialog.SetActive(true);
            warningDialog.GetComponent<PopUP>().title.text = title;
            warningDialog.GetComponent<PopUP>().msg.text = message;
        }

        private IEnumerator waitForPlayersToStart()
        {
            Debug.Log("Waiting for players " + GameManager.Instance.readyPlayersCount + " - " + requiredToStart);

            yield return new WaitForEndOfFrame();

            if (GameManager.Instance.readyPlayersCount < requiredToStart)
            {
                StartCoroutine(waitForPlayersToStart());
            }
            else
            {
                AllPlayersReady = true;
                SetTurn();
                // if (myIndex == 0)
                // {
                //     SetMyTurn();
                //     playerObjects[0].dice.GetComponent<GameDiceController>().DisableDiceShadow();
                // }
                // else
                // {
                //     SetOpponentTurn();
                //     playerObjects[currentPlayerIndex].dice.GetComponent<GameDiceController>().DisableDiceShadow();
                // }
            }
        }

        public int GetCurrentPlayerIndex()
        {
            return currentPlayerIndex;
        }

        public void TIPButton()
        {
            if (TIPObject.activeSelf)
            {
                TIPObject.SetActive(false);
            }
            else
            {
                TIPObject.SetActive(true);
            }
        }

        public void FacebookShare()
        {
            if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
            {
                Uri myUri = new Uri("https://play.google.com/store/apps/details?id=" + StaticStrings.AndroidPackageName);
            #if UNITY_IPHONE
                myUri = new Uri("https://itunes.apple.com/us/app/apple-store/id" + StaticStrings.ITunesAppID);
            #endif

                FB.ShareLink(
                    myUri,
                    StaticStrings.facebookShareLinkTitle,
                    callback: ShareCallback
                );
            }
        }

        private void ShareCallback(IShareResult result)
        {
            if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
            {
                Debug.Log("ShareLink Error: " + result.Error);
            }
            else if (!String.IsNullOrEmpty(result.PostId))
            {
                // Print post identifier of the shared content
                Debug.Log(result.PostId);
            }
            else
            {
                // Share succeeded without postID
                //GameManager.Instance.playfabManager.addCoinsRequest(StaticStrings.rewardCoinsForShareViaFacebook);
                Debug.Log("ShareLink success!");
            }
        }

        public void StopAndFinishGame()
        {
            StopTimers();
            SetFinishGame(PhotonNetwork.player.UserId, true);
            ShowGameFinishWindow();
        }

        public void ShareScreenShot()
        {
        #if UNITY_ANDROID
            string text = StaticStrings.ShareScreenShotText;
            text = text + " " + "https://play.google.com/store/apps/details?id=" + StaticStrings.AndroidPackageName;
            ScreenShotController.GetComponent<NativeShare>().ShareScreenshotWithText(text);
        #elif UNITY_IOS
            string text = StaticStrings.ShareScreenShotText;
            text = text + " " + "https://itunes.apple.com/us/app/apple-store/id" + StaticStrings.ITunesAppID;
            ScreenShotController.GetComponent<NativeShare>().ShareScreenshotWithText(text);
        #endif
        }

        public void ShowGameFinishWindow()
        {
            if (!FinishWindowActive)
            {
                FinishWindowActive = true;

                /*List<PlayerObject> players = new List<PlayerObject>();

                for (int i = 0; i < playerObjects.Count; i++)
                {
                    PlayerAvatarController controller = playerObjects[i].AvatarObject.GetComponent<PlayerAvatarController>();
                    if (controller.Active && !controller.finished)
                    {
                        players.Add(playerObjects[i]);
                    }
                }*/
                GameFinishWindow.GetComponent<GameFinishWindowController>().showWinLoseWindow();
            }
        }

        private void ButtonClick(string id)
        {

            int index = 0;

            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i].id == id)
                {
                    index = i;
                    break;
                }
            }

            CurrentPlayerID = id;
            if (playerObjects[index].AvatarObject.GetComponent<PlayerAvatarController>().Active)
            {
                PlayerInfoWindow.GetComponent<PlayerInfoController>().ShowPlayerInfo(playerObjects[index].avatar, playerObjects[index].name, playerObjects[index].data);
            }
        }

        public void AddFriendButtonClick()
        {
            if (!CurrentPlayerID.Contains("_BOT"))
            {
                AddFriendRequest request = new AddFriendRequest()
                {
                    FriendPlayFabId = CurrentPlayerID,
                };

                //PlayFabClientAPI.AddFriend(request, (result) =>
                //{
                //    PhotonNetwork.RaiseEvent((int)EnumPhoton.AddFriend, PhotonNetwork.playerName + ";" + GameManager.Instance.nameMy + ";" + CurrentPlayerID, true, null);
                //    addedFriendWindow.SetActive(true);
                //    Debug.Log("Added friend successfully");
                //}, (error) =>
                //{
                //    addedFriendWindow.SetActive(true);
                //    Debug.Log("Error adding friend: " + error.Error);
                //}, null);
            }
            else
            {
                Debug.Log("Add Friend - It's bot!");
                addedFriendWindow.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void FinishedGame(string userId)
        {
            if (userId == UserDetailsManager.userId)     ///earlier: GameManager.Instance.currentPlayer.id
            {
                SetFinishGame(UserDetailsManager.userId, true);
            }
            else
            {
                SetFinishGame(userId, false);
            }
            // SetFinishGame(PhotonNetwork.player.NickName, true);
        }

        private void SetFinishGame(string id, bool me)
        {
            if (!me || !iFinished)
            {
                Debug.Log("SET FINISH");
                ActivePlayersInRoom--;

                int index = GetPlayerPosition(id);
                Debug.Log("Index: " + index);
                Debug.Log("playerObjects: " + playerObjects.Count);
                playersFinished.Add(playerObjects[index]);

                PlayerAvatarController controller = playerObjects[index].AvatarObject.GetComponent<PlayerAvatarController>();
                controller.Name.GetComponent<Text>().text = "";
                controller.Active = false;
                controller.finished = true;

                playerObjects[index].dice.SetActive(false);

                int position = playersFinished.Count;
                if (position == 1)
                {
                    controller.Crown.SetActive(true);
                    winnerId = id;
                }

                if (me)
                {
                    PhotonNetwork.BackgroundTimeout = StaticStrings.photonDisconnectTimeoutLong;
                    iFinished = true;
                    if (ActivePlayersInRoom > 0 && (PhotonNetwork.room.PlayerCount > 1 || GameManager.Instance.offlineMode))
                    {
                        PhotonNetwork.RaiseEvent((int)EnumPhoton.FinishedGame, UserDetailsManager.userId, true, null);
                        LudoMultiplayer.Instance.gameOver = true;
                        Debug.Log("set finish call finish turn");
                        SendFinishTurn();
                    }

                    if (position == 1)
                    {
                        WinSound.Play();
                        //Dictionary<string, string> data = new Dictionary<string, string>();
                        //data.Add(LudoPlayerData.CoinsKey, (GameManager.Instance.myPlayerData.GetCoins() + firstPlacePrize).ToString());
                        //data.Add(LudoPlayerData.TotalEarningsKey, (GameManager.Instance.myPlayerData.GetTotalEarnings() + firstPlacePrize).ToString());
                        //if (GameManager.Instance.type == MyGameType.TwoPlayer)
                        //{
                        //  data.Add(LudoPlayerData.TwoPlayerWinsKey, (GameManager.Instance.myPlayerData.GetTwoPlayerWins() + 1).ToString());
                        //}
                        //else if (GameManager.Instance.type == MyGameType.FourPlayer)
                        //{
                        //    data.Add(LudoPlayerData.FourPlayerWinsKey, (GameManager.Instance.myPlayerData.GetFourPlayerWins() + 1).ToString());
                        //}
                        //GameManager.Instance.myPlayerData.UpdateUserData(data);
                    }
                    /*else if (position == 2)
                    {
                        Dictionary<string, string> data = new Dictionary<string, string>();
                        data.Add(LudoPlayerData.CoinsKey, (GameManager.Instance.myPlayerData.GetCoins() + secondPlacePrize).ToString());
                        data.Add(LudoPlayerData.TotalEarningsKey, (GameManager.Instance.myPlayerData.GetTotalEarnings() + secondPlacePrize).ToString());
                        GameManager.Instance.myPlayerData.UpdateUserData(data);
                    }*/
                }
                else if (GameManager.Instance.currentPlayer.isBot)
                {
                    SendFinishTurn();
                }

                controller.setPositionSprite(position);                
                CheckPlayersIfShouldFinishGame();
            }
        }

        public int GetPlayerPosition(string id)
        {
            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i].id.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }

        public void SendFinishTurn()
        {
            Debug.Log("SendFinishTurn called!");
            if (!FinishWindowActive && ActivePlayersInRoom > 1)
            {
                if (GameManager.Instance.currentPlayer.isBot)
                {
                    BotDelay();
                }
                else
                {
                    if (currentPlayerIndex == myIndex)
                    {
                        if (!GameManager.Instance.offlineMode)
                        {
                            Debug.Log("Sending Event 172 for turn change");
                            PhotonNetwork.RaiseEvent((int)EnumPhoton.NextPlayerTurn, myIndex, true, null);
                        }
                        //currentPlayerIndex = (myIndex + 1) % playerObjects.Count;

                        Debug.Log("PLAYER BEFORE: " + currentPlayerIndex);
                        setCurrentPlayerIndex(myIndex);
                        Debug.Log("PLAYER AFTER: " + currentPlayerIndex + " isbot: " + GameManager.Instance.currentPlayer.isBot);                        
                    }
                    else
                        Debug.Log("Not my turn");

                    SetTurn();
                    //SetOpponentTurn();

                    GameManager.Instance.miniGame.setOpponentTurn();
                }
            }            
        }

        void Awake()
        {
            PhotonNetwork.OnEventCall += this.OnEvent;
        }

        void OnDestroy()
        {
            PhotonNetwork.OnEventCall -= this.OnEvent;
        }

        private void OnEvent(byte eventcode, object content, int senderid)
        {
            Debug.Log("received event: " + eventcode);
            if (eventcode == (int)EnumPhoton.NextPlayerTurn)
            {
                if (playerObjects[(int)content].AvatarObject.GetComponent<PlayerAvatarController>().Active &&
                    currentPlayerIndex == (int)content)
                {
                    if (!FinishWindowActive)
                    {
                        setCurrentPlayerIndex((int)content);

                        SetTurn();
                    }
                }
            }
            else if (eventcode == (int)EnumPhoton.SendChatMessage)
            {
                string[] message = ((string)content).Split(';');
                Debug.Log("Received message " + message[0] + " from " + message[1]);
                for (int i = 0; i < playerObjects.Count; i++)
                {
                    if (playerObjects[i].id.Equals(message[1]))
                    {
                        playerObjects[i].ChatBubbleText.SetActive(true);
                        playerObjects[i].ChatbubbleImage.SetActive(false);
                        playerObjects[i].ChatBubbleText.GetComponent<Text>().text = message[0];
                        playerObjects[i].ChatBubble.GetComponent<Animator>().Play("MessageBubbleAnimation");
                    }
                }
            }
            else if (eventcode == (int)EnumPhoton.SendChatEmojiMessage)
            {
                string[] message = ((string)content).Split(';');
                Debug.Log("Received message " + message[0] + " from " + message[1]);
                for (int i = 0; i < playerObjects.Count; i++)
                {
                    if (playerObjects[i].id.Equals(message[1]))
                    {
                        playerObjects[i].ChatBubbleText.SetActive(false);
                        playerObjects[i].ChatbubbleImage.SetActive(true);
                        int index = int.Parse(message[0]);

                        if (index > emojiSprites.Length - 1)
                        {
                            index = emojiSprites.Length;
                        }
                        playerObjects[i].ChatbubbleImage.GetComponent<RawImage>().texture = emojiSprites[index];
                        playerObjects[i].ChatBubble.GetComponent<Animator>().Play("MessageBubbleAnimation");
                    }
                }
            }
            else if (eventcode == (int)EnumPhoton.AddFriend)
            {
                if (PlayerPrefs.GetInt(StaticStrings.FriendsRequestesKey, 0) == 0)
                {
                    string[] data = ((string)content).Split(';');
                    if (PhotonNetwork.playerName.Equals(data[2]))
                        invitiationDialog.GetComponent<PhotonChatListener2>().showInvitationDialog(data[0], data[1], null);
                }
                else
                {
                    Debug.Log("Invitations OFF");
                }

            }
            else if (eventcode == (int)EnumPhoton.FinishedGame)
            {
                string message = (string)content;
                Debug.Log("FinishedGame Details:" + message);
                winnerId = message;                
                ShowGameFinishWindow();
            }
        }

        private void SetMyTurn()
        {
            GameManager.Instance.isMyTurn = true;

            if (GameManager.Instance.miniGame != null)
                GameManager.Instance.miniGame.setMyTurn();

            StartTimer();
        }

        private void BotTurn()
        {
            Debug.Log("Bot : " + currentPlayerIndex);
            //GameManager.Instance.currentPlayer = playerObjects[currentPlayerIndex];
            GameManager.Instance.isMyTurn = false;
            oppoTurnSource.Play();
            Debug.Log("Bot Turn");
            StartTimer();
            GameManager.Instance.miniGame.BotTurn(true);
            //Invoke("BotDelay", 2.0f);
        }

        private void SetTurn()
        {
            Debug.Log("SET TURN CALLED");
            for (int i = 0; i < playerObjects.Count; i++)
            {
                playerObjects[i].dice.GetComponent<GameDiceController>().EnableDiceShadow();
                playerObjects[i].dice.SetActive(false);
            }
            playerObjects[currentPlayerIndex].dice.SetActive(true);
            playerObjects[currentPlayerIndex].dice.GetComponent<GameDiceController>().DisableDiceShadow();
            //playerObjects[currentPlayerIndex].dice.GetComponent<GameDiceController>().EnableShot();
            GameManager.Instance.currentPlayer = playerObjects[currentPlayerIndex];          

            

            if (Players.Length == 4 && playerObjects[currentPlayerIndex].finishedPawns == playerObjects[currentPlayerIndex].pawns.Length)
            {
                BotDelay();
            }
            else
            {
                if (playerObjects[currentPlayerIndex].id == myId)
                {
                    Debug.Log("Player Turn");
                    SetMyTurn();
                }
                else if (playerObjects[currentPlayerIndex].isBot)
                {
                    Debug.Log("bot Turn");
                    BotTurn();
                }
                else
                {
                    SetOpponentTurn();
                }
            }
        }

        private void BotDelay()
        {
            if (!FinishWindowActive)
            {
                setCurrentPlayerIndex(currentPlayerIndex);
                SetTurn();
            }
        }

        private void setCurrentPlayerIndex(int current)
        {
            while (true)
            {
                current = current + 1;
                currentPlayerIndex = (current) % playerObjects.Count;
                if(LudoMultiplayer.Instance.disconnectedPlayersIndexList.Contains(currentPlayerIndex))
                {
                    setCurrentPlayerIndex(currentPlayerIndex);
                    break;
                }
                GameManager.Instance.currentPlayer = playerObjects[currentPlayerIndex];
                if (playerObjects[currentPlayerIndex].AvatarObject.GetComponent<PlayerAvatarController>().Active) break;
            }
        }

        private void SetOpponentTurn()
        {
            Debug.Log("Opponent turn");
            oppoTurnSource.Play();
            GameManager.Instance.isMyTurn = false;
            if (playerObjects[currentPlayerIndex].id.Contains("_BOT"))
            {
                Debug.Log("Bot Turn!!");
                BotTurn();
            }

            StartTimer();
        }

        private void StartTimer()
        {
            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (i == currentPlayerIndex)
                {
                    playerObjects[currentPlayerIndex].timer.SetActive(true);
                }
                else
                {
                    playerObjects[i].timer.SetActive(false);
                }
            }
        }

        public void StopTimers()
        {
            for (int i = 0; i < playerObjects.Count; i++)
            {
                playerObjects[i].timer.SetActive(false);
            }
        }

        public void PauseTimers()
        {
            playerObjects[currentPlayerIndex].timer.GetComponent<UpdatePlayerTimer>().Pause();
        }

        public void restartTimer()
        {
            playerObjects[currentPlayerIndex].timer.GetComponent<UpdatePlayerTimer>().restartTimer();
        }

        public void PlayerDisconnected(PhotonPlayer otherPlayer)
        {
            PlayerData pd = JsonUtility.FromJson<PlayerData>(otherPlayer.CustomProperties["data"].ToString());

            Debug.Log("Player disconnected: " + pd.userid);

            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i].id.Equals(pd.userid))
                {
                    setPlayerDisconnected(i);
                    break;
                }
            }            
        }

        // public void CheckPlayersIfShouldFinishGame()
        // {
        //     if (!FinishWindowActive)
        //     {
        //         if ((ActivePlayersInRoom == 1 && !iFinished) || ActivePlayersInRoom == 1)
        //         {
        //             StopAndFinishGame();
        //         }

        //         if (iFinished && ActivePlayersInRoom == 1 && CheckIfOtherPlayerIsBot())
        //         {
        //             AddBotToListOfWinners();
        //             StopAndFinishGame();
        //         }
        //     }
        // }

        public void CheckPlayersIfShouldFinishGame()
        {
            Debug.Log("CheckPlayersIfShouldFinishGame()  FinishWindowActive:" + FinishWindowActive);
            if (!FinishWindowActive)
            {
                Debug.Log("Active Players In Room: " + ActivePlayersInRoom + "iFinished: "+ iFinished);
                if ((ActivePlayersInRoom <= 1) && !iFinished)
                {
                    Debug.Log("Check Finish 1");
                    StopAndFinishGame();
                    return;
                }                

                if (iFinished && ActivePlayersInRoom == 1 && CheckIfOtherPlayerIsBot())
                {
                    Debug.Log("Check Finish 3");
                    AddBotToListOfWinners();
                    StopAndFinishGame();
                    return;
                }

                if (ActivePlayersInRoom > 1 && iFinished)
                {
                    Debug.Log("Check Finish 4");
                    //TIPButtonObject.SetActive(true);
                    StopAndFinishGame();
                }
            }
        }
        public void AddBotToListOfWinners()
        {
            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i].id.Contains("_BOT") && playerObjects[i].AvatarObject.GetComponent<PlayerAvatarController>().Active)
                {
                    playersFinished.Add(playerObjects[i]);
                }
            }
        }

        public bool CheckIfOtherPlayerIsBot()
        {
            for (int i = 0; i < playerObjects.Count; i++)
            {
                if (playerObjects[i].id.Contains("_BOT") && playerObjects[i].AvatarObject.GetComponent<PlayerAvatarController>().Active)
                {
                    playerObjects[i].AvatarObject.GetComponent<PlayerAvatarController>().finished = true;
                    return true;
                }
            }
            return false;
        }

        public void setPlayerDisconnected(int i)
        {
            requiredToStart--;
            if (!FinishWindowActive)
            {
                if (!playerObjects[i].AvatarObject.GetComponent<PlayerAvatarController>().finished)
                    ActivePlayersInRoom--;

                Debug.Log("Active players: " + ActivePlayersInRoom);
                if (currentPlayerIndex == i && ActivePlayersInRoom > 1)
                {
                    setCurrentPlayerIndex(currentPlayerIndex);
                    if (AllPlayersReady)
                        SetTurn();
                }

                Debug.Log("za petla");
                playerObjects[i].AvatarObject.GetComponent<PlayerAvatarController>().PlayerLeftRoom();
                CheckPlayersIfShouldFinishGame();
                // LUDO
                playerObjects[i].dice.SetActive(false);
                if (!playerObjects[i].AvatarObject.GetComponent<PlayerAvatarController>().finished)
                {
                    for (int j = 0; j < playerObjects[i].pawns.Length; j++)
                    {
                        // playerObjects[i].pawns[j].SetActive(false);
                        playerObjects[i].pawns[j].GetComponent<LudoPawnController>().GoToInitPosition(false);
                    }
                }
                // END LUDO
                
            }
        }

        public void LeaveGame(bool finishWindow)
        {
            if (!iFinished || finishWindow)
            {
                PlayerPrefs.SetInt("GamesPlayed", PlayerPrefs.GetInt("GamesPlayed", 1) + 1);
                Destroy(LudoMultiplayer.Instance.gameObject);
                SceneManager.LoadScene("LudoMenu");
                PhotonNetwork.RaiseEvent(153, UserDetailsManager.userId, true, null);
                //GameManager.Instance.cueController.removeOnEventCall();
                PhotonNetwork.BackgroundTimeout = StaticStrings.photonDisconnectTimeoutLong;
                PhotonNetwork.LeaveRoom();
                LudoMultiplayer.Instance.roomOwner = false;
                GameManager.Instance.roomOwner = false;
                GameManager.Instance.resetAllData();
            }
            else
            {

                if (SceneManager.GetActiveScene().name == "GameScene" && LudoMultiplayer.Instance.isOnlineMode)
                {
                    if (string.IsNullOrEmpty(winnerId))
                        winnerId = "winner";
                    ShowGameFinishWindow();
                }
            }
        }

        public void ShowHideChatWindow()
        {
            if (!ChatWindow.activeSelf)
            {
                ChatWindow.SetActive(true);
                ChatButton.GetComponent<Text>().text = "X";
            }
            else
            {
                ChatWindow.SetActive(false);
                ChatButton.GetComponent<Text>().text = "CHAT";
            }
        }

        public IEnumerator GetUserStats()
        {
            string url = "http://18.191.157.16:4000/apis/getuserstats";
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
                if (!www.downloadHandler.text.Contains("error"))
                {
                    UserDetailsManager.userCoins = Mathf.RoundToInt(float.Parse(userDetails["coins"].ToString()));
                    UserDetailsManager.userCoinsWon = int.Parse(userDetails["coins_won"].ToString());
                    coinsText.text = UserDetailsManager.userCoins.ToString();
                }
            }
        }
    }    
}