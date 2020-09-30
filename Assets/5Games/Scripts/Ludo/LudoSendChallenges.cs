using Ludo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LudoSendChallenges : MonoBehaviour
{
    public static LudoSendChallenges Instance;
    public InputField betAmount;
    public InputField receiverName;
    public Button sendChallengeToServerBtn;
    public Toggle sides1, sides2;
    [SerializeField] int playerCount = 2;
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
        Instance = this;
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

    /*public void DisplayReceivedChallenges(bool show)
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
            LudoMultiplayer.Instance.errorText.text = "You don't have enough coins!!";
            LudoMultiplayer.Instance.errorDialog.SetActive(true);
            return;
        }
        else if (bet < 500)
        {
            LudoMultiplayer.Instance.errorText.text = "Bet Amount needs to be 500 or above.";
            LudoMultiplayer.Instance.errorDialog.SetActive(true);
            return;
        }

        if (sides2.isOn)
            GameManager.Instance.sides = MySidesType.OneSide;
        else
            GameManager.Instance.sides = MySidesType.TwoSide;
        StartCoroutine(ValidateUserName(receiverName.text, bet));
    }

    IEnumerator ValidateUserName(string name, int bet)
    {
        //LudoMultiplayer.Instance.Loader.SetActive(true);
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
        //LudoMultiplayer.Instance.Loader.SetActive(false);
        Debug.Log("Check UserName Response: " + www.downloadHandler.text);
        var validateUserDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Error while trying to validate Username: " + www.error);
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().title.text = "ERROR";
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().msg.text = www.error;
            LudoMultiplayer.Instance.errorDialog.SetActive(true);
        }
        else if (www.downloadHandler.text.Contains("Username not exists"))
        {
            var errorDetails = (IDictionary)validateUserDetails["result"];
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().title.text = "INVALID USER";
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().msg.text = "Entered Username doesn't exist!!";
            receiverName.text = "";
            LudoMultiplayer.Instance.errorDialog.SetActive(true);
        }
        else
        {
            Debug.Log("PlayerId: " + PhotonNetwork.player.UserId);
            PhotonNetwork.player.NickName = UserDetailsManager.userName;
            PhotonNetwork.player.UserId = UserDetailsManager.userId;
            LudoMultiplayer.Instance.isChallenge = true;
            LudoMultiplayer.Instance.receiverName = receiverName.text;
            LudoMultiplayer.Instance.playerCount = playerCount;
            LudoMultiplayer.Instance.betAmountText.text = bet.ToString();
            LudoMultiplayer.Instance.JoinRoomAndStartGame();
            this.gameObject.SetActive(false);
        }
    }
}
