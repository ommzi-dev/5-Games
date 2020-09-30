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
using UnityEngine.UI;
using System;
using Photon.Chat;
using UnityEngine.SceneManagement;
using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.Advertisements;
#endif
using AssemblyCSharp;
namespace Ludo
{
    public class InitMenuScript : MonoBehaviour
    {
        //public GameObject FacebookLinkReward;
        //public GameObject rewardDialogText;
        //public GameObject FacebookLinkButton;
        //public GameObject videoRewardText;
        //public GameObject playerName;
        //public GameObject playerAvatar;
        public GameObject fbFriendsMenu;
        public GameObject matchPlayer;
        public GameObject backButtonMatchPlayers;
        public GameObject MatchPlayersCanvas;
        public GameObject menuCanvas;
        public GameObject tablesCanvas;
        public GameObject changeDialog;
        public GameObject inputNewName;
        public GameObject tooShortText;
        public GameObject coinsText;
        public GameObject coinsTextShop;
        public GameObject coinsTab;
        //public GameObject TheMillButton;
        public GameObject dialog;
        // Use this for initialization
        public GameObject GameConfigurationScreen;
        
        void Start()
        {




            if (PlayerPrefs.GetInt(StaticStrings.SoundsKey, 0) == 0)
            {
                AudioListener.volume = 1;
            }
            else
            {
                AudioListener.volume = 0;
            }


            //FacebookLinkReward.GetComponent<Text>().text = "+ " + StaticStrings.CoinsForLinkToFacebook;

            
            //GameManager.Instance.FacebookLinkButton = FacebookLinkButton;

            GameManager.Instance.dialog = dialog;
            GameManager.Instance.tablesCanvas = tablesCanvas;
            //GameManager.Instance.facebookFriendsMenu = fbFriendsMenu.GetComponent<FacebookFriendsMenu>(); ;
            GameManager.Instance.matchPlayerObject = matchPlayer;
            GameManager.Instance.backButtonMatchPlayers = backButtonMatchPlayers;
            //playerName.GetComponent<Text>().text = GameManager.Instance.nameMy;
            GameManager.Instance.MatchPlayersCanvas = MatchPlayersCanvas;

            /*if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
            {
                FacebookLinkButton.SetActive(false);
            }*/

            //if (GameManager.Instance.avatarMy != null)
            //    playerAvatar.GetComponent<RawImage>().texture = GameManager.Instance.avatarMy.texture;

            //GameManager.Instance.myAvatarGameObject = playerAvatar;
            //GameManager.Instance.myNameGameObject = playerName;

            //GameManager.Instance.coinsTextMenu = coinsText;
            //GameManager.Instance.coinsTextShop = coinsTextShop;
            GameManager.Instance.initMenuScript = this;

            //if (StaticStrings.hideCoinsTabInShop)
            //{
            //    coinsTab.SetActive(false);
            //}
                       
            //rewardDialogText.GetComponent<Text>().text = "1 Video = " + StaticStrings.rewardForVideoAd + " Coins";
            //coinsText.GetComponent<Text>().text = GameManager.Instance.myPlayerData.GetCoins() + "";



            Debug.Log("Load ad menu");

            /*if (PlayerPrefs.GetInt("GamesPlayed", 1) % 8 == 0 && PlayerPrefs.GetInt("GameRated", 0) == 0)
            {
                rateWindow.SetActive(true);
                PlayerPrefs.SetInt("GamesPlayed", PlayerPrefs.GetInt("GamesPlayed", 1) + 1);
            }*/

        }


        /*public void QuitApp()
        {
            PlayerPrefs.SetInt("GameRated", 1);
#if UNITY_ANDROID
            Application.OpenURL("market://details?id=" + StaticStrings.AndroidPackageName);
#elif UNITY_IPHONE
        Application.OpenURL("itms-apps://itunes.apple.com/app/id" + StaticStrings.ITunesAppID);
#endif
            //Application.Quit();
        }*/


        /*public void LinkToFacebook()
        {
            GameManager.Instance.facebookManager.FBLinkAccount();
        }*/

        public void ShowGameConfiguration(int index)
        {
            switch (index)
            {
                case 0:
                    GameManager.Instance.type = MyGameType.TwoPlayer;
                    break;
                case 1:
                    GameManager.Instance.type = MyGameType.FourPlayer;
                    break;
                case 2:
                    GameManager.Instance.type = MyGameType.Private;
                    break;
            }
            GameConfigurationScreen.SetActive(true);
        }

       /* public void TakeScreenshot()
        {
            ScreenCapture.CaptureScreenshot("TestScreenshot.png");
        }*/


        // Update is called once per frame
        void Update()
        {
        }


        public void backToMenuFromTableSelect()
        {
            GameManager.Instance.offlineMode = false;
            tablesCanvas.SetActive(false);
            menuCanvas.SetActive(true);
            
        }

        public void showSelectTableScene(bool challengeFriend)
        {
            if (!challengeFriend)
                GameManager.Instance.inviteFriendActivated = false;

            if (GameManager.Instance.offlineMode)
            {
                //TheMillButton.SetActive(false);
            }
            else
            {
                //TheMillButton.SetActive(true);
            }
            menuCanvas.SetActive(false);
            tablesCanvas.SetActive(true);
            
        }

        public void playOffline()
        {
            //GameManager.Instance.tableNumber = 0;
            GameManager.Instance.offlineMode = true;
            GameManager.Instance.roomOwner = true;
            showSelectTableScene(false);
            //SceneManager.LoadScene(GameManager.Instance.GameScene);
        }

        public void switchUser()
        {
            //GameManager.Instance.playfabManager.destroy();
            //GameManager.Instance.facebookManager.destroy();
            //GameManager.Instance.connectionLost.destroy();

            GameManager.Instance.avatarMy = null;
            PhotonNetwork.Disconnect();

            PlayerPrefs.DeleteAll();
            GameManager.Instance.resetAllData();
            LocalNotification.ClearNotifications();
            //GameManager.Instance.myPlayerData.GetCoins() = 0;
            SceneManager.LoadScene("LoginSplash");
        }

        public void showChangeDialog()
        {
            changeDialog.SetActive(true);
        }

        public void changeUserName()
        {
            Debug.Log("Change Nickname");

            string newName = inputNewName.GetComponent<Text>().text;
            /*if (newName.Equals(StaticStrings.addCoinsHackString))
            {
                GameManager.Instance.playfabManager.addCoinsRequest(1000000);
                changeDialog.SetActive(false);
            }
            else
            {
                if (newName.Length > 0)
                {
                    UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
                    {
                        //DisplayName = newName
                        //DisplayName = GameManager.Instance.playfabManager.PlayFabId
                        DisplayName = UserDetailsManager.userName
                    };

                    //PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
                    //{
                    //    Dictionary<string, string> data = new Dictionary<string, string>();
                    //    data.Add("PlayerName", newName);
                    //    UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
                    //    {
                    //        Data = data,
                    //        Permission = UserDataPermission.Public
                    //    };

                    //    PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
                    //    {
                    //        Debug.Log("Data updated successfull ");
                    //        Debug.Log("Title Display name updated successfully");
                    //        PlayerPrefs.SetString("GuestPlayerName", newName);
                    //        PlayerPrefs.Save();
                    //        GameManager.Instance.nameMy = newName;
                    //        playerName.GetComponent<Text>().text = newName;
                    //    }, (error1) =>
                    //    {
                    //        Debug.Log("Data updated error " + error1.ErrorMessage);
                    //    }, null);

                    //}, (error) =>
                    //{
                    //    Debug.Log("Title Display name updated error: " + error.Error);

                    //}, null);

                    changeDialog.SetActive(false);
                }
                else
                {
                    tooShortText.SetActive(true);
                }
            }*/
        }

        public void startQuickGame()
        {
            //GameManager.Instance.facebookManager.startRandomGame();
        }

        public void startQuickGameTableNumer(int tableNumer, int fee)
        {
            GameManager.Instance.payoutCoins = fee;
            GameManager.Instance.tableNumber = tableNumer;
           // GameManager.Instance.facebookManager.startRandomGame();
        }

        public void showFacebookFriends()
        {
           // GameManager.Instance.playfabManager.GetPlayfabFriends();
        }

        public void setTableNumber()
        {
            GameManager.Instance.tableNumber = int.Parse(GameObject.Find("TextTableNumber").GetComponent<Text>().text);
        }
    }
}
