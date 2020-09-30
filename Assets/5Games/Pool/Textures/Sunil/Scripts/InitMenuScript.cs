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

public class InitMenuScript : MonoBehaviour
{

    public GameObject playerName;
    public GameObject videoRewardText;
    public GameObject playerAvatar;
    public GameObject fbFriendsMenu;
    public GameObject matchPlayer;
    public GameObject backButtonMatchPlayers;
    public GameObject MatchPlayersCanvas;
    public GameObject menuCanvas;
    public GameObject tablesCanvas;
    public GameObject gameTitle;
    public GameObject changeDialog;
    public GameObject inputNewName;
    public GameObject tooShortText;
    public GameObject coinsText;
    public GameObject coinsTextShop;
    public GameObject coinsTab;

    public GameObject dialog;
    // Use this for initialization
    void Start()
    {

#if UNITY_ANDROID 
        Advertisement.Initialize(PoolStaticStrings.unityAdsAndroidID);
#else
        Advertisement.Initialize(StaticStrings.unityAdsIOSID);
#endif


        PoolGameManager.Instance.dialog = dialog;
        videoRewardText.GetComponent<Text>().text = "+" + PoolStaticStrings.rewardForVideoAd;
        PoolGameManager.Instance.tablesCanvas = tablesCanvas;
        PoolGameManager.Instance.facebookFriendsMenu = fbFriendsMenu.GetComponent<FacebookFriendsMenu>(); ;
        PoolGameManager.Instance.matchPlayerObject = matchPlayer;
        PoolGameManager.Instance.backButtonMatchPlayers = backButtonMatchPlayers;
        playerName.GetComponent<Text>().text = UserDetailsManager.userName;
        PoolGameManager.Instance.MatchPlayersCanvas = MatchPlayersCanvas;

        if (PoolGameManager.Instance.avatarMy != null)
            playerAvatar.GetComponent<Image>().sprite = PoolGameManager.Instance.avatarMy;


        PoolGameManager.Instance.coinsTextMenu = coinsText;
        PoolGameManager.Instance.coinsTextShop = coinsTextShop;
        PoolGameManager.Instance.playfabManager.updateCoinsTextMenu();
        PoolGameManager.Instance.playfabManager.updateCoinsTextShop();
        PoolGameManager.Instance.initMenuScript = this;

        if (PoolStaticStrings.hideCoinsTabInShop)
        {
            coinsTab.SetActive(false);
        }

#if UNITY_WEBGL
        coinsTab.SetActive(false);
#endif
        //coinsText.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void showAdStore()
    {
        //if (StaticStrings.showAdOnStoreScene)
        //    PoolGameManager.Instance.adsScript.ShowAd();
    }

    public void backToMenuFromTableSelect()
    {
        tablesCanvas.SetActive(false);
        menuCanvas.SetActive(true);
        gameTitle.SetActive(true);
    }

    public void showSelectTableScene(bool challengeFriend)
    {
        if (!challengeFriend)
            PoolGameManager.Instance.inviteFriendActivated = false;
        //if (StaticStrings.showAdOnSelectTableScene)
        //    PoolGameManager.Instance.adsScript.ShowAd();
        menuCanvas.SetActive(false);
        tablesCanvas.SetActive(true);
        gameTitle.SetActive(false);
    }

    public void playOffline()
    {
        PoolGameManager.Instance.tableNumber = 0;
        PoolGameManager.Instance.offlineMode = true;
        PoolGameManager.Instance.roomOwner = true;
        SceneManager.LoadScene("Pool_GameScene_Original");
    }

    public void switchUser()
    {
        PoolGameManager.Instance.playfabManager.destroy();
        PoolGameManager.Instance.facebookManager.destroy();
        PoolGameManager.Instance.connectionLost.destroy();
        //PoolGameManager.Instance.adsScript.destroy();
        PoolGameManager.Instance.avatarMy = null;
        PhotonNetwork.Disconnect();

        PlayerPrefs.DeleteAll();
        PoolGameManager.Instance.resetAllData();
        PoolGameManager.Instance.coinsCount = 0;
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
        if (newName.Equals(PoolStaticStrings.addCoinsHackString))
        {
            PoolGameManager.Instance.playfabManager.addCoinsRequest(1000000);
            changeDialog.SetActive(false);
        }
        else
        {
            if (newName.Length > 0)
            {
                UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
                {
                    //DisplayName = newName
                    DisplayName = PoolGameManager.Instance.playfabManager.PlayFabId
                };

                PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
                {
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("PlayerName", newName);
                    UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
                    {
                        Data = data,
                        Permission = UserDataPermission.Public
                    };

                    PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
                    {
                        Debug.Log("Data updated successfull ");
                        Debug.Log("Title Display name updated successfully");
                        PlayerPrefs.SetString("GuestPlayerName", newName);
                        PlayerPrefs.Save();
                        PoolGameManager.Instance.nameMy = newName;
                        playerName.GetComponent<Text>().text = newName;
                    }, (error1) =>
                    {
                        Debug.Log("Data updated error " + error1.ErrorMessage);
                    }, null);

                }, (error) =>
                {
                    Debug.Log("Title Display name updated error: " + error.Error);

                }, null);

                changeDialog.SetActive(false);
            }
            else
            {
                tooShortText.SetActive(true);
            }
        }



    }

    public void startQuickGame()
    {
        PoolGameManager.Instance.facebookManager.startRandomGame();
    }

    public void startQuickGameTableNumer(int tableNumer, int fee)
    {
        PoolGameManager.Instance.payoutCoins = fee;
        PoolGameManager.Instance.tableNumber = tableNumer;
        PoolGameManager.Instance.facebookManager.startRandomGame();
    }

    public void showFacebookFriends()
    {
        //if (StaticStrings.showAdOnFriendsScene)
        //    PoolGameManager.Instance.adsScript.ShowAd();
        PoolGameManager.Instance.playfabManager.GetPlayfabFriends();
    }

    public void setTableNumber()
    {
        PoolGameManager.Instance.tableNumber = Int32.Parse(GameObject.Find("TextTableNumber").GetComponent<Text>().text);
    }


    public void ShowRewardedAd()
    {
        Debug.Log("Rewarded Ad Button Click");
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        if (Advertisement.IsReady("rewardedVideo"))
        {
            Debug.Log("UnityAds Show");
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show("rewardedVideo", options);
        }
        else
        {
            Debug.Log("UnityAds not ready");
        }
#endif
    }


#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                PoolGameManager.Instance.playfabManager.addCoinsRequest(PoolStaticStrings.rewardForVideoAd);
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }
#endif

}
