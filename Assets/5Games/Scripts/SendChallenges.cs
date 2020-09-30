using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SendChallenges : MonoBehaviour
{
    public static SendChallenges instance;
    public InputField betAmount;
    public InputField receiverName;
    public Button sendChallengeToServerBtn;
    //public GameObject sendTab, receiveTab, challengeScreen, sendChallengeDialog;
    //public Button sendBtn, receiveBtn, sendChallengeToServerBtn;

    /* [Header("Sent Challenges")]
     [SerializeField] List<GameObject> sentChallengeInstances = new List<GameObject>();
     [SerializeField] GameObject sentChallengePrefab, sentChallengeParent;

     [Header("Receive Challenges")]
     [SerializeField] List<GameObject> receivedChallengeInstances = new List<GameObject>();
     [SerializeField] GameObject receivedChallengePrefab, receivedChallengeParent;
     */
    public void Start()
    {
        instance = this;
       // ShowSentChallenges();
       // ShowRecievedChallenges();
       // DisplayReceivedChallenges(true);
    }

    public void VerifyUserName()
    {
       // Debug.Log("Receiver Text: " + receiverName.text);
        if (!string.IsNullOrEmpty(receiverName.text) && !string.IsNullOrWhiteSpace(receiverName.text))
            sendChallengeToServerBtn.interactable = true;
        else
            sendChallengeToServerBtn.interactable = false;
    }

    public void ResetSendChallengeDialog()
    {
        receiverName.text = "";
        betAmount.text = "500";
        sendChallengeToServerBtn.interactable = false;       
    }

    /* public void DisplayReceivedChallenges(bool show)
     {
         receiveBtn.interactable = !show;
         sendBtn.interactable = show;
         receiveTab.SetActive(show);
         sendTab.SetActive(!show);
     }

     void ResetChallengeScreen()
     {
         challengeScreen.SetActive(false);
         sendChallengeDialog.SetActive(false);
     }*/

    public void SendChallenge()
    {
        int bet = 0;
        if (!string.IsNullOrEmpty(betAmount.text) && !string.IsNullOrWhiteSpace(betAmount.text))
            bet = int.Parse(betAmount.text);
        if (UserDetailsManager.userCoins < bet)
        {
            WHOTMultiplayerManager.Instance.betAmtError.text = "You don't have enough coins!!";
            WHOTMultiplayerManager.Instance.buyCoins.SetActive(true);
            return;
        }
        else if (bet < 500)
        {
            WHOTMultiplayerManager.Instance.betAmtError.text = "Bet Amount needs to be 500 or above.";
            WHOTMultiplayerManager.Instance.buyCoins.SetActive(true);
            return;
        }        
        StartCoroutine(ValidateUserName(receiverName.text, bet));
    }

    IEnumerator ValidateUserName(string name, int bet)
    {
        WhotManager.instance.Loader.SetActive(true);
        Debug.Log("Check Name: " + name);
        string msg = UserDetailsManager.userName + " has challenged you for a bet of " + bet + " coins in Whot!";
        Debug.Log("Message: " + msg);
        WWWForm form = new WWWForm();
        form.AddField("username", name);
        form.AddField("message", msg);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "checkusername", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();
        WhotManager.instance.Loader.SetActive(false);
        Debug.Log("Check UserName Response: " + www.downloadHandler.text);
        var joinPoolDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Error while trying to validate Username: " + www.error);
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = www.error;
            WhotUiManager.instance.errorPopup.SetActive(true);
        }
        else if (www.downloadHandler.text.Contains("Username not exists"))
        {
            var errorDetails = (IDictionary)joinPoolDetails["result"];
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "INVALID USER";
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = "Entered Username doesn't exist!!";
            receiverName.text = "";
            WhotUiManager.instance.errorPopup.SetActive(true);
        }
        else
        {
            Debug.Log("PlayerId: " + PhotonNetwork.player.UserId);
            PhotonNetwork.player.NickName = UserDetailsManager.userName;
            PhotonNetwork.player.UserId = UserDetailsManager.userId;
            WHOTMultiplayerManager.Instance.isChallenge = true;
            WHOTMultiplayerManager.Instance.receiverName = receiverName.text;
            WHOTMultiplayerManager.Instance.betAmountText.text = bet.ToString();
            WHOTMultiplayerManager.Instance.JoinRoomAndStartGame();
            this.gameObject.SetActive(false);
        }
    }


    /* 
     #region Send Challenge
     public IEnumerator SendChallengeToServer()
     {
         string msg = UserDetailsManager.userName + " has challenged you in Whot!";
         Debug.Log("Challenge Message: "+ msg);
         WWWForm form = new WWWForm();
         form.AddField("receiver", receiverName.text);
         form.AddField("action", "register");
         form.AddField("bet_amount", int.Parse(betAmount.text));
         form.AddField("game_id", "5"); /// 5 represents Whot game
         form.AddField("message", msg);
         UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "challenge", form);
         www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
         www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

         www.timeout = 120;
         yield return www.SendWebRequest();
         Debug.Log("Send Challenge To Server: " + www.downloadHandler.text);
         if (www.error != null || www.isNetworkError)
         {
             Debug.Log("Error while sending Challenge: " + www.error);
             WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
             WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = www.error;
             WhotUiManager.instance.errorPopup.SetActive(true);
         }
         else
         {
             var sendChallengeDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
             if (www.downloadHandler.text.Contains("error"))
             {
                 var errorDetails = (IDictionary)sendChallengeDetails["result"];
                 WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
                 WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = errorDetails["error"].ToString();
                 WhotUiManager.instance.errorPopup.SetActive(true);
             }
             else
             {
                 Debug.Log("Challenge Sent Successfully!");
                 //call sent challenges api
                 ChatGui.instance.sendPhotonNotification(receiverName.text, UserDetailsManager.userName, "refresh");
                 ShowSentChallenges();
             }
         }
     }

     public void ShowSentChallenges()
     {
         StartCoroutine(GetSentChallengesFromServer());
         DisplayReceivedChallenges(false);
     }

     IEnumerator GetSentChallengesFromServer()
     {
         //Debug.Log("Get Sent Challenges From Server Called!");
         string url = UserDetailsManager.serverUrl + "getchallenge/5";  //5- gameid
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
             Debug.Log("Get Sent Challenges Response: " + www.downloadHandler.text);
             var challengeStats = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
             foreach (GameObject temp in sentChallengeInstances)
             {
                 Destroy(temp);
             }
             if (!www.downloadHandler.text.Contains("No user found"))
             {
                 var challengeList = (IList)challengeStats["result"];

                 sentChallengeInstances.Clear();
                 for (int k = 0; k < challengeList.Count; k++)
                 {
                     IDictionary challengeDetails = (IDictionary)challengeList[k];
                     GameObject challengeInstance = Instantiate(sentChallengePrefab, sentChallengeParent.transform);
                     sentChallengeInstances.Add(challengeInstance);
                     challengeInstance.name = challengeDetails["sender"].ToString();
                     ChallengeDetails details = challengeInstance.GetComponent<ChallengeDetails>();
                     details.challengeId = challengeDetails["id"].ToString();
                     details.poolId = challengeDetails["poolid"].ToString();
                     details.senderName = challengeDetails["sender"].ToString();
                     details.receiverName = challengeDetails["receiver"].ToString();
                     details.betAmount = challengeDetails["bet_amount"].ToString();
                     details.winningAmount = challengeDetails["win_amount"].ToString();
                     if (challengeDetails["status"].ToString() == "sent")
                         details.joinBtn.interactable = false;
                     else
                         details.joinBtn.interactable = true;
                     details.SetSentChallengeDetails();
                 }
             }

             //yield return new WaitForSecondsRealtime(5f);
             //StartCoroutine(GetSentChallengesFromServer());
         }
     }
     #endregion

     #region Receive Challenges
     public void ShowRecievedChallenges()
     {
         StartCoroutine(GetReceiveChallengesFromServer());
         DisplayReceivedChallenges(true);
     }

     IEnumerator GetReceiveChallengesFromServer()
     {
         //Debug.Log("Get Received Challenges From Server Called!");
         string url = UserDetailsManager.serverUrl + "receivedchallenge/5";  //5- gameid
         Debug.Log("UserDetailsManager.accessToken" + UserDetailsManager.accessToken);
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
             Debug.Log("Get Received Challenges Response: " + www.downloadHandler.text);
             var challengeStats = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
             foreach (GameObject temp in receivedChallengeInstances)
             {
                 Destroy(temp);
             }
             if (!www.downloadHandler.text.Contains("No user found"))
             {
                 var challengeList = (IList)challengeStats["result"];

                 receivedChallengeInstances.Clear();
                 for (int k = 0; k < challengeList.Count; k++)
                 {
                     IDictionary challengeDetails = (IDictionary)challengeList[k];
                     GameObject challengeInstance = Instantiate(receivedChallengePrefab, receivedChallengeParent.transform);
                     receivedChallengeInstances.Add(challengeInstance);
                     challengeInstance.name = challengeDetails["sender"].ToString();
                     ChallengeDetails details = challengeInstance.GetComponent<ChallengeDetails>();
                     details.challengeId = challengeDetails["id"].ToString();
                     details.poolId = challengeDetails["poolid"].ToString();
                     details.senderName = challengeDetails["sender"].ToString();
                     details.receiverName = challengeDetails["receiver"].ToString();
                     details.betAmount = challengeDetails["bet_amount"].ToString();
                     details.winningAmount = challengeDetails["win_amount"].ToString();
                     details.SetReceiveChallengeDetails();
                 }
             }
             //yield return new WaitForSecondsRealtime(5f);
             //StartCoroutine(GetReceiveChallengesFromServer());
         }
     }
     #endregion
     */
}
