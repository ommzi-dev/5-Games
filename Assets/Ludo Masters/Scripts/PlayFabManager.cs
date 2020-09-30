/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using System.Collections.Generic;
using Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using AssemblyCSharp;
using System.Globalization;

namespace Ludo
{
    public class PlayFabManager : Photon.PunBehaviour
    {

        private Texture[] avatarSprites;

        //public string PlayFabId = UserDetailsManager.userId;
        //public string authToken;
        //public bool multiGame = true;
        //public bool roomOwner = false;
        //public GameObject fbButton;
        //private FacebookFriendsMenu facebookFriendsMenu;
        public ChatClient chatClient;
        private bool alreadyGotFriends = false;
        public GameObject menuCanvas;
        public GameObject MatchPlayersCanvas;
        public bool opponentReady = false;
        public bool imReady = false;
        public GameObject backButtonMatchPlayers;


       

        public bool isInLobby = false;
        public bool isInMaster = false;

        void Awake()
        {
            //Debug.Log("Playfab awake");
            //PlayerPrefs.DeleteAll();
            //PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.PhotonCloud;
           // PhotonNetwork.PhotonServerSettings.PreferredRegion = CloudRegionCode.eu;
            // PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.BestRegion;
            // PhotonNetwork.PhotonServerSettings.AppID = StaticStrings.PhotonAppID;
//#if UNITY_IOS
//        PhotonNetwork.PhotonServerSettings.Protocol = ConnectionProtocol.Tcp;
//#else
//            PhotonNetwork.PhotonServerSettings.Protocol = ConnectionProtocol.Udp;
//#endif
//            Debug.Log("PORT: " + PhotonNetwork.PhotonServerSettings.ServerPort);

            //PlayFabSettings.TitleId = StaticStrings.PlayFabTitleID;

            //PhotonNetwork.OnEventCall += this.OnEvent;
            //DontDestroyOnLoad(transform.gameObject);
        }

        /*void OnDestroy()
        {
            PhotonNetwork.OnEventCall -= this.OnEvent;
        }

        public void destroy()
        {
            if (this.gameObject != null)
                DestroyImmediate(this.gameObject);
        }*/

        // Use this for initialization
        void Start()
        {
            Debug.Log("Playfab start");
            //PhotonNetwork.BackgroundTimeout = StaticStrings.photonDisconnectTimeoutLong; ;
            //fbManager = GameObject.Find("FacebookManager").GetComponent<FacebookManager>();
            //facebookFriendsMenu = GameManager.Instance.facebookFriendsMenu;

            avatarSprites = new Texture[] { UserDetailsManager.userImageTexture };
        }

        //void Update()
        //{
        //    if (chatClient != null) { chatClient.Service(); }
        //}



        // handle events:
        /*private void OnEvent(byte eventcode, object content, int senderid)
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
                //LoadGameScene();
            }

        }*/

        //public void LoadGameWithDelay()
        //{
        ///    LoadGameScene();
        //}
        
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
            LudoMultiplayer.Instance.StartGameScene();
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

        
        public void setInitNewAccountData(bool fb)
        {
            Dictionary<string, string> data = LudoPlayerData.InitialUserData(fb);
            GameManager.Instance.myPlayerData.UpdateUserData(data);
        }


        public void updateBoughtChats(int index)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add(LudoPlayerData.ChatsKey, GameManager.Instance.myPlayerData.GetChats() + ";'" + index + "'");


            GameManager.Instance.myPlayerData.UpdateUserData(data);


        }

       /* public void UpdateBoughtEmojis(int index)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add(LudoPlayerData.EmojiKey, GameManager.Instance.myPlayerData.GetEmoji() + ";'" + index + "'");


            GameManager.Instance.myPlayerData.UpdateUserData(data);
        }*/

        public void addCoinsRequest(int count)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add(LudoPlayerData.CoinsKey, "" + (GameManager.Instance.myPlayerData.GetCoins() + count));
            GameManager.Instance.myPlayerData.UpdateUserData(data);
        }

        /*public void getPlayerDataRequest()
        {
            Debug.Log("Get player data request!!");
            GetUserDataRequest getdatarequest = new GetUserDataRequest()
            {
                PlayFabId = GameManager.Instance.playfabManager.PlayFabId,
            };

            PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
            {

                Dictionary<string, UserDataRecord> data = result.Data;

                GameManager.Instance.myPlayerData = new MyPlayerData(data, true);


                Debug.Log("Get player data request finish!!");
                StartCoroutine(loadSceneMenu());
            }, (error) =>
            {
                Debug.Log("Data updated error " + error.ErrorMessage);
            }, null);
        }/


        private IEnumerator loadSceneMenu()
        {
            yield return new WaitForSeconds(0.1f);

            if (isInMaster && isInLobby)
            {
                SceneManager.LoadScene("LudoMenu");
            }
            else
            {
                StartCoroutine(loadSceneMenu());
            }

        }

        
        public void LinkFacebookAccount()
        {
            LinkFacebookAccountRequest request = new LinkFacebookAccountRequest()
            {
                AccessToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString,
                ForceLink = true
            };

            PlayFabClientAPI.LinkFacebookAccount(request, (result) =>
            {
                Dictionary<string, string> data = new Dictionary<string, string>();

                data.Add("LoggedType", "Facebook");
                data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
                data.Add("PlayerAvatarUrl", GameManager.Instance.avatarMyUrl);
                data.Add(MyPlayerData.PlayerName, GameManager.Instance.nameMy);
                data.Add(MyPlayerData.AvatarIndexKey, "fb");
                data.Add(MyPlayerData.CoinsKey, (GameManager.Instance.myPlayerData.GetCoins() + StaticStrings.CoinsForLinkToFacebook).ToString());
                GameManager.Instance.myAvatarGameObject.GetComponent<Image>().sprite = GameManager.Instance.facebookAvatar;
                GameManager.Instance.myNameGameObject.GetComponent<Text>().text = GameManager.Instance.nameMy;
                GameManager.Instance.myPlayerData.UpdateUserData(data);

                GameManager.Instance.FacebookLinkButton.SetActive(false);
            },
            (error) =>
            {
                Debug.Log("Error linking facebook account: " + error.ErrorMessage + "\n" + error.ErrorDetails);
                GameManager.Instance.connectionLost.showDialog();
            });



        }       
        
        private string androidUnique()
        {
            AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityPlayerActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject unityPlayerResolver = unityPlayerActivity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaClass androidSettingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
            return androidSettingsSecure.CallStatic<string>("getString", unityPlayerResolver, "android_id");
        }

        
       /* public void GetPlayfabFriends()
        {
            if (alreadyGotFriends)
            {
                Debug.Log("show firneds FFFF");
                if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
                {
                    fbManager.getFacebookInvitableFriends();
                }
                else
                {

                    facebookFriendsMenu.showFriends(null, null, null);
                }
            }
            else
            {
                Debug.Log("IND");
                GetFriendsListRequest request = new GetFriendsListRequest();
                request.IncludeFacebookFriends = true;
                PlayFabClientAPI.GetFriendsList(request, (result) =>
                {

                    Debug.Log("Friends list Playfab: " + result.Friends.Count);
                    var friends = result.Friends;

                    List<string> playfabFriends = new List<string>();
                    List<string> playfabFriendsName = new List<string>();
                    List<string> playfabFriendsFacebookId = new List<string>();


                    chatClient.RemoveFriends(GameManager.Instance.friendsIDForStatus.ToArray());

                    List<string> friendsToStatus = new List<string>();


                    int index = 0;
                    foreach (var friend in friends)
                    {

                        playfabFriends.Add(friend.FriendPlayFabId);

                        Debug.Log("Title: " + friend.TitleDisplayName);
                        GetUserDataRequest getdatarequest = new GetUserDataRequest()
                        {
                            PlayFabId = friend.TitleDisplayName,
                        };


                        int ind2 = index;

                        PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                        {

                            Dictionary<string, UserDataRecord> data2 = result2.Data;
                            playfabFriendsName[ind2] = data2["PlayerName"].Value;
                            Debug.Log("Added " + data2["PlayerName"].Value);
                            GameManager.Instance.facebookFriendsMenu.updateName(ind2, data2["PlayerName"].Value, friend.TitleDisplayName);

                        }, (error) =>
                        {

                            Debug.Log("Data updated error " + error.ErrorMessage);
                        }, null);

                        playfabFriendsName.Add("");

                        friendsToStatus.Add(friend.FriendPlayFabId);

                        index++;
                    }

                    GameManager.Instance.friendsIDForStatus = friendsToStatus;

                    chatClient.AddFriends(friendsToStatus.ToArray());

                    GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

                    if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
                    {
                        fbManager.getFacebookInvitableFriends();
                    }
                    else
                    {
                        GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
                    }
                }, OnPlayFabError);
            }


        }*/       

        

        /*public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
        {
            GameManager.Instance.opponentDisconnected = true;

            GameManager.Instance.invitationID = "";

            if (GameManager.Instance.controlAvatars != null)
            {
                Debug.Log("PLAYER DISCONNECTED " + player.UserId);
                if (PhotonNetwork.room.PlayerCount > 1)
                {
                    GameManager.Instance.controlAvatars.startButtonPrivate.GetComponent<Button>().interactable = true;
                }
                else
                {
                    GameManager.Instance.controlAvatars.startButtonPrivate.GetComponent<Button>().interactable = false;
                }


                int index = GameManager.Instance.opponentsIDs.IndexOf(player.UserId);
                //PhotonNetwork.room.IsOpen = true;
                GameManager.Instance.controlAvatars.PlayerDisconnected(index);
            }
        }*/
              

        
        //public void challengeFriend(string id, string message)
        ///{
            //if (GameManager.Instance.invitationID.Length == 0 || !GameManager.Instance.invitationID.Equals(id))
            //{
       //     chatClient.SendPrivateMessage(id, "INVITE_TO_PLAY_PRIVATE;" + /*id + this.PlayFabId + ";" +*/ GameManager.Instance.nameMy + ";" + message);
       //     GameManager.Instance.invitationID = id;
       //     Debug.Log("Send invitation to: " + id);
            // }
        //}

        //string roomname;
        /*public void OnPrivateMessage(string sender, object message, string channelName)
        {
            if (!sender.Equals(UserDetailsManager.userName.ToLower()))
            {
                if (message.ToString().Contains("INVITE_TO_PLAY_PRIVATE"))
                {
                    GameManager.Instance.invitationID = sender;

                    string[] messageSplit = message.ToString().Split(';');
                    string whoInvite = messageSplit[1];
                    string payout = messageSplit[2];
                    string roomID = messageSplit[3];
                    GameManager.Instance.payoutCoins = int.Parse(payout);
                    GameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(0, whoInvite, payout, roomID, 0);
                }
            }

            if ((GameManager.Instance.invitationID.Length == 0 || !GameManager.Instance.invitationID.Equals(sender)))
            {

            }
            else
            {
                GameManager.Instance.invitationID = "";
            }
        }*/


        public override void OnDisconnectedFromPhoton()
        {
            Debug.Log("Disconnected from photon");
            //switchUser();
        }

        public void DisconnecteFromPhoton()
        {
            PhotonNetwork.Disconnect();
        }
             

       /* public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {
            Debug.Log("STATUS UPDATE CHAT!");
            Debug.Log("Status change for: " + user + " to: " + status);

            bool foundFriend = false;
            for (int i = 0; i < GameManager.Instance.friendsStatuses.Count; i++)
            {
                string[] friend = GameManager.Instance.friendsStatuses[i];
                if (friend[0].Equals(user))
                {
                    GameManager.Instance.friendsStatuses[i][1] = "" + status;
                    foundFriend = true;
                    break;
                }
            }

            if (!foundFriend)
            {
                GameManager.Instance.friendsStatuses.Add(new string[] { user, "" + status });
            }

            if (GameManager.Instance.facebookFriendsMenu != null)
                GameManager.Instance.facebookFriendsMenu.updateFriendStatus(status, user);
        }*/        

        
        /*public override void OnJoinedRoom()
        {

            Debug.Log("OnJoinedRoom");


            if (PhotonNetwork.room.CustomProperties.ContainsKey("bt"))
            {
                extractBotMoves(PhotonNetwork.room.CustomProperties["bt"].ToString());
            }

            if (PhotonNetwork.room.CustomProperties.ContainsKey("fp"))
            {
                GameManager.Instance.firstPlayerInGame = int.Parse(PhotonNetwork.room.CustomProperties["fp"].ToString());
            }
            else
            {
                GameManager.Instance.firstPlayerInGame = 0;
            }

            GameManager.Instance.avatarOpponent = null;

            Debug.Log("Players in room " + PhotonNetwork.room.PlayerCount);

            GameManager.Instance.currentPlayersCount = 1;

            GameManager.Instance.controlAvatars.setCancelButton();
            if (PhotonNetwork.room.PlayerCount == 1)
            {
                GameManager.Instance.roomOwner = true;
                WaitForNewPlayer();
            }
            else if (PhotonNetwork.room.PlayerCount >= GameManager.Instance.requiredPlayers)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;
            }

            if (!roomOwner)
            {
                GameManager.Instance.backButtonMatchPlayers.SetActive(false);

                for (int i = 0; i < PhotonNetwork.otherPlayers.Length; i++)
                {

                    int ii = i;
                    int index = GetFirstFreeSlot();
                    GameManager.Instance.opponentsIDs[index] = PhotonNetwork.otherPlayers[ii].UserId;

                    GetUserDataRequest getdatarequest = new GetUserDataRequest()
                    {
                        PlayFabId = PhotonNetwork.otherPlayers[ii].UserId,

                    };

                    string otherID = PhotonNetwork.otherPlayers[ii].UserId;


                    //PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
                    //{
                    //    Dictionary<string, UserDataRecord> data = result.Data;

                    //    if (data.ContainsKey("LoggedType"))
                    //    {
                    //        if (data["LoggedType"].Value.Equals("Facebook"))
                    //        {
                    //            bool fbAvatar = true;
                    //            int avatarIndex = 0;
                    //            if (!data[MyPlayerData.AvatarIndexKey].Value.Equals("fb"))
                    //            {
                    //                fbAvatar = false;
                    //                avatarIndex = int.Parse(data[MyPlayerData.AvatarIndexKey].Value.ToString());
                    //            }
                    //            getOpponentData(data, index, fbAvatar, avatarIndex, otherID);
                    //        }
                    //        else
                    //        {
                    //            if (data.ContainsKey("PlayerName"))
                    //            {
                    //                GameManager.Instance.opponentsNames[index] = data["PlayerName"].Value;
                    //            //GameManager.Instance.controlAvatars.PlayerJoined(index);
                    //            bool fbAvatar = true;
                    //                int avatarIndex = 0;
                    //                if (!data[MyPlayerData.AvatarIndexKey].Value.Equals("fb"))
                    //                {
                    //                    fbAvatar = false;
                    //                    avatarIndex = int.Parse(data[MyPlayerData.AvatarIndexKey].Value.ToString());
                    //                }
                    //                getOpponentData(data, index, fbAvatar, avatarIndex, otherID);
                    //            }
                    //            else
                    //            {
                    //                Debug.Log("ERROR");
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Debug.Log("ERROR");
                    //    }

                    //}, (error) =>
                    //{
                    //    Debug.Log("Get user data error: " + error.ErrorMessage);
                    //}, null);
                }
            }
        }*/


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

        /*public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("Failed to create room");
            CreatePrivateRoom();
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("Failed to join room");

            if (GameManager.Instance.type == MyGameType.Private)
            {
                if (GameManager.Instance.controlAvatars != null)
                {
                    GameManager.Instance.controlAvatars.ShowJoinFailed(codeAndMsg[1].ToString());
                }
            }
            //else
           // {
                //GameManager.Instance.facebookManager.startRandomGame();
           // }
        }
        */
        //private void GetPlayerDataRequest(string playerID)
        //{

        //}

        /*public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            CancelInvoke("StartGameWithBots");

            Debug.Log("New player joined " + newPlayer.UserId);
            Debug.Log("Players Count: " + GameManager.Instance.currentPlayersCount);



            if (PhotonNetwork.room.PlayerCount >= GameManager.Instance.requiredPlayers)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;
            }

            if (PhotonNetwork.room.PlayerCount > 1)
            {
                GameManager.Instance.controlAvatars.startButtonPrivate.GetComponent<Button>().interactable = true;
            }
            else
            {
                GameManager.Instance.controlAvatars.startButtonPrivate.GetComponent<Button>().interactable = true;
            }

            int index = GetFirstFreeSlot();

            GameManager.Instance.opponentsIDs[index] = newPlayer.UserId;
            GetUserDataRequest getdatarequest = new GetUserDataRequest()
            {
                PlayFabId = newPlayer.UserId,
            };

            //PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
            //{
            //    Dictionary<string, UserDataRecord> data = result.Data;

            //    if (data.ContainsKey("LoggedType"))
            //    {
            //        if (data["LoggedType"].Value.Equals("Facebook"))
            //        {
            //            bool fbAvatar = true;
            //            int avatarIndex = 0;
            //            if (!data[MyPlayerData.AvatarIndexKey].Value.Equals("fb"))
            //            {
            //                fbAvatar = false;
            //                avatarIndex = int.Parse(data[MyPlayerData.AvatarIndexKey].Value.ToString());
            //            }
            //            getOpponentData(data, index, fbAvatar, avatarIndex, newPlayer.UserId);
            //        }
            //        else
            //        {
            //            if (data.ContainsKey("PlayerName"))
            //            {
            //                GameManager.Instance.opponentsNames[index] = data["PlayerName"].Value;
            //            //GameManager.Instance.controlAvatars.PlayerJoined(index);
            //            bool fbAvatar = true;
            //                int avatarIndex = 0;
            //                if (!data[MyPlayerData.AvatarIndexKey].Value.Equals("fb"))
            //                {
            //                    fbAvatar = false;
            //                    avatarIndex = int.Parse(data[MyPlayerData.AvatarIndexKey].Value.ToString());
            //                }
            //                getOpponentData(data, index, fbAvatar, avatarIndex, newPlayer.UserId);
            //            }
            //            else
            //            {
            //                Debug.Log("ERROR");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Debug.Log("ERROR");
            //    }

            //}, (error) =>
            //{
            //    Debug.Log("Get user data error: " + error.ErrorMessage);
            //}, null);
        }*/

        private void getOpponentData(Dictionary<string, UserDataRecord> data, int index, bool fbAvatar, int avatarIndex, string id)
        {
            if (data.ContainsKey("PlayerName"))
            {
                GameManager.Instance.opponentsNames[index] = data["PlayerName"].Value;
            }
            else
            {
                GameManager.Instance.opponentsNames[index] = "Guest857643";
            }

            if (data.ContainsKey("PlayerAvatarUrl") && fbAvatar)
            {
                StartCoroutine(loadImageOpponent(data["PlayerAvatarUrl"].Value, index, id));
            }
            else
            {
                Debug.Log("GET OPPONENT DATA: " + avatarIndex);
                GameManager.Instance.opponentsAvatars[index] = UserDetailsManager.userImageTexture;
                //GameManager.Instance.opponentsAvatars[index] = null;
                GameManager.Instance.controlAvatars.PlayerJoined(index, id);
            }

        }

        public IEnumerator loadImageOpponent(string url, int index, string id)
        {
            WWW www = new WWW(url);

            yield return www;

            GameManager.Instance.opponentsAvatars[index] = www.texture;
            GameManager.Instance.controlAvatars.PlayerJoined(index, id);
        }

        
    }
}