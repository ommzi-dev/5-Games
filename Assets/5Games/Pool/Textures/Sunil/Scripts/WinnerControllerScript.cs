using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AssemblyCSharp;
using PlayFab.ClientModels;
using Facebook.Unity;
using System;

public class WinnerControllerScript : MonoBehaviour
{

    public GameObject myObject;
    public GameObject opponentObject;
    public GameObject shareButton;

    public bool isGameScene = false;

    public Image myImage;
    public Image oppoImage;

    public Text myName;
    public Text oppoText;

    public GameObject myMessageBubble;
    public GameObject oppoMessageBubble;

    public GameObject rematchButton;

    public bool rematchRequest = false;
    public bool sentRematch = false;

    public GameObject ChatMessagesList;
    public GameObject ChatMessageButtonPrefab;

    public GameObject ChatMessagesObject;

    public GameObject prizeText;
    private AudioSource[] audioSources;
    public GameObject reardShareText;

    public bool messageDialogVisible = false;
    // Use this for initialization
    void Start()
    {



        audioSources = GetComponents<AudioSource>();

        if (PoolGameManager.Instance.playerDisconnected)
        {
            PoolGameManager.Instance.playerDisconnected = false;
            if (!isGameScene)
            {
                rematchButton.SetActive(false);
            }
        }



        if (!isGameScene)
        {


            PhotonNetwork.BackgroundTimeout = 0;

            if (PoolGameManager.Instance.payoutCoins > PoolGameManager.Instance.coinsCount)
            {
                rematchButton.SetActive(false);
            }

            if (reardShareText != null)
                reardShareText.GetComponent<Text>().text = "+" + PoolStaticStrings.rewardCoinsForShareViaFacebook;

            //if (StaticStrings.showAdOnGameOverScene)
            //    PoolGameManager.Instance.adsScript.ShowAd();

            if (!PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
            {
                shareButton.SetActive(false);
            }

            rematchRequest = false;
            sentRematch = false;

            if (PoolGameManager.Instance.iWon)
            {
                myObject.GetComponent<Animator>().Play("WinnerOpponentAnimation");
                audioSources[0].Play();
                PoolGameManager.Instance.playfabManager.addCoinsRequest(PoolGameManager.Instance.payoutCoins * 2);
            }
            else
            {
                opponentObject.GetComponent<Animator>().Play("WinnerOpponentAnimation");
                audioSources[1].Play();
            }

            if (PoolGameManager.Instance.avatarMy != null)
                myImage.sprite = PoolGameManager.Instance.avatarMy;
            if (PoolGameManager.Instance.avatarOpponent != null)
                oppoImage.sprite = PoolGameManager.Instance.avatarOpponent;

            myName.text = PoolGameManager.Instance.nameMy;
            oppoText.text = PoolGameManager.Instance.nameOpponent;

            int prizeCoins = PoolGameManager.Instance.payoutCoins * 2;

            if (prizeCoins >= 1000)
            {
                if (prizeCoins >= 1000000)
                {
                    if (prizeCoins % 1000000.0f == 0)
                    {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000000.0f).ToString("0") + "M";

                    }
                    else
                    {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000000.0f).ToString("0.0") + "M";

                    }

                }
                else
                {
                    if (prizeCoins % 1000.0f == 0)
                    {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000.0f).ToString("0") + "k";
                    }
                    else
                    {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000.0f).ToString("0.0") + "k";
                    }

                }
            }
            else
            {
                prizeText.GetComponent<Text>().text = prizeCoins + "";
            }
        }

        for (int i = 0; i < PoolStaticStrings.chatMessages.Length; i++)
        {
            GameObject button = Instantiate(ChatMessageButtonPrefab);
            button.transform.GetChild(0).GetComponent<Text>().text = PoolStaticStrings.chatMessages[i];
            button.transform.parent = ChatMessagesList.transform;
            button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            string index = PoolStaticStrings.chatMessages[i];
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => SendMessageEvent(index));
        }

        for (int i = 0; i < PoolStaticStrings.chatMessagesExtended.Length; i++)
        {
            if (PoolGameManager.Instance.ownedChats.Contains("'" + i + "'"))
            {
                for (int j = 0; j < PoolStaticStrings.chatMessagesExtended[i].Length; j++)
                {
                    GameObject button = Instantiate(ChatMessageButtonPrefab);
                    button.transform.GetChild(0).GetComponent<Text>().text = PoolStaticStrings.chatMessagesExtended[i][j];
                    button.transform.parent = ChatMessagesList.transform;
                    button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    string index = PoolStaticStrings.chatMessagesExtended[i][j];
                    button.GetComponent<Button>().onClick.RemoveAllListeners();
                    button.GetComponent<Button>().onClick.AddListener(() => SendMessageEvent(index));
                }
            }

        }

    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    // void Update() {

    //     if (Input.anyKey && messageDialogVisible) {
    //         ChatMessagesObject.GetComponent<Animator>().Play("hideMessageDialog");
    //         messageDialogVisible = false;
    //     }
    // }

    public void share()
    {
        if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
        {

            Uri myUri = new Uri(PoolStaticStrings.facebookShareLinkAndroid);
#if UNITY_IPHONE
            myUri = new Uri(StaticStrings.facebookShareLinkAppStore);
#endif

            FB.ShareLink(
                myUri,
                PoolStaticStrings.facebookShareLinkTitle,
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
            PoolGameManager.Instance.playfabManager.addCoinsRequest(PoolStaticStrings.rewardCoinsForShareViaFacebook);
            Debug.Log("ShareLink success!");
        }
    }


    void OnDestroy()
    {
        removeOnEventCall();
    }

    public void SendMessageEvent(string index)
    {
        Debug.Log("Button Clicked " + index);
        if (!PoolGameManager.Instance.offlineMode)
            PhotonNetwork.RaiseEvent(193, index, true, null);
        ChatMessagesObject.GetComponent<Animator>().Play("hideMessageDialog");
        messageDialogVisible = false;

        myMessageBubble.SetActive(true);
        myMessageBubble.transform.GetChild(0).GetComponent<Text>().text = index;

        if (isGameScene)
        {
            myMessageBubble.SetActive(true);
            myMessageBubble.transform.GetChild(0).GetComponent<Text>().text = index;
            if (isGameScene)
            {
                CancelInvoke("hideMyMessageBubble");
                Invoke("hideMyMessageBubble", 6.0f);
            }
        }

    }

    public void loadMenuScene()
    {
        //if (PoolGameManager.Instance.offlineMode && StaticStrings.showAdWhenLeaveGame)
        //    PoolGameManager.Instance.adsScript.ShowAd();
        SceneManager.LoadScene("Menu");
        Debug.Log("Timeout 6");
        PhotonNetwork.BackgroundTimeout = 0;
        if (!PoolGameManager.Instance.offlineMode)
            PhotonNetwork.RaiseEvent(194, 1, true, null);
        removeOnEventCall();

        PoolGameManager.Instance.cueController.removeOnEventCall();
        if (PhotonNetwork.room != null)
            PhotonNetwork.LeaveRoom();

        PoolGameManager.Instance.playfabManager.roomOwner = false;
        PoolGameManager.Instance.roomOwner = false;
        PoolGameManager.Instance.resetAllData();

    }

    public void sendRematchRequest()
    {
        if (!rematchRequest)
        {
            sentRematch = true;
            Debug.Log("Send message");
            if (!PoolGameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(195, 1, true, null);
            myMessageBubble.SetActive(true);
            myMessageBubble.transform.GetChild(0).GetComponent<Text>().text = PoolStaticStrings.IWantPlayAgain;
            rematchButton.SetActive(false);
        }
        else
        {
            Debug.Log("Send message");
            if (!PoolGameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(195, 1, true, null);
            rematchButton.SetActive(false);
            PoolGameManager.Instance.resetAllData();
            SceneManager.LoadScene("GameScene");
            removeOnEventCall();
        }
    }

    public void sendMessageButton()
    {
        ChatMessagesObject.GetComponent<Animator>().Play("showMessagesDialog");
        messageDialogVisible = true;

    }

    void Awake()
    {
        PhotonNetwork.OnEventCall += this.OnEvent;

    }

    public void removeOnEventCall()
    {
        PhotonNetwork.OnEventCall -= this.OnEvent;
    }

    // Multiplayer data received
    private void OnEvent(byte eventcode, object content, int senderid)
    {
        Debug.Log("Received message");
        if (eventcode == 195)
        {
            if (sentRematch)
            {
                PoolGameManager.Instance.resetAllData();
                SceneManager.LoadScene("GameScene");
                removeOnEventCall();
            }
            else
            {
                rematchRequest = true;
                if (PoolGameManager.Instance.payoutCoins <= PoolGameManager.Instance.coinsCount)
                {
                    oppoMessageBubble.SetActive(true);
                    oppoMessageBubble.transform.GetChild(0).GetComponent<Text>().text = PoolStaticStrings.IWantPlayAgain;
                }
            }
        }
        else if (eventcode == 194)
        {
            rematchButton.SetActive(false);
            oppoMessageBubble.SetActive(true);
            oppoMessageBubble.transform.GetChild(0).GetComponent<Text>().text = PoolStaticStrings.cantPlayRightNow;

        }
        else if (eventcode == 193)
        {
            string index = (string)content;
            Debug.Log("INDEX: " + index);
            oppoMessageBubble.SetActive(true);
            oppoMessageBubble.transform.GetChild(0).GetComponent<Text>().text = index;
            if (isGameScene)
            {
                CancelInvoke("hideOppoMessageBubble");
                Invoke("hideOppoMessageBubble", 6.0f);
            }

        }
    }

    public void hideOppoMessageBubble()
    {
        oppoMessageBubble.SetActive(false);
    }

    public void hideMyMessageBubble()
    {
        myMessageBubble.SetActive(false);
    }
}
