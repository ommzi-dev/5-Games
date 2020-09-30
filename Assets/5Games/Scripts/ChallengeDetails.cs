using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChallengeDetails : MonoBehaviour
{
    public string challengeId, poolId;
    public string senderName, receiverName;
    public string betAmount, winningAmount;
    public Button joinBtn;

    public Text userNameText, betAmountText, winAmountText;

    public void SetSentChallengeDetails()
    {
        userNameText.text = "RECEIVER : " + receiverName;
        betAmountText.text = "BET AMOUNT : " + betAmount;
        winAmountText.text = "WINNING AMOUNT: " + winningAmount.ToString();
    }

    public void SetReceiveChallengeDetails()
    {
        userNameText.text = "SENDER : " + senderName;
        betAmountText.text = "BET AMOUNT : " + betAmount;
        winAmountText.text = "WINNING AMOUNT: " + winningAmount.ToString();
    }

    public void AcceptChallenge()
    {
        Debug.Log("Challenge Accepted");
        StartCoroutine(SendChallengeResponseToServer("approved"));
        ChatGui.instance.sendPhotonNotification(senderName, UserDetailsManager.userName, "refresh");
    }

    public void DeclineChallenge()
    {

        Debug.Log("Challenge Declined");
        StartCoroutine(SendChallengeResponseToServer("rejected"));
        ChatGui.instance.sendPhotonNotification(senderName, UserDetailsManager.userName, "refresh");
    }

    IEnumerator SendChallengeResponseToServer(string myAction)
    {
       string message = "";
        if (myAction == "approved")
            message= UserDetailsManager.userName + " has accepted your challenge request!";
        else
            message = UserDetailsManager.userName + " has declined your challenge request!";
        Debug.Log("Challenge Response Message: "+ message);
        WWWForm form = new WWWForm();
        form.AddField("action", myAction);
        form.AddField("poolid", poolId);
        form.AddField("cid", challengeId); 
        form.AddField("game_id", "5"); /// 5 represents Whot game
        form.AddField("message", message);
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
                Debug.Log("Challenge Response Sent Successfully!");
                if (myAction == "approved")
                    StartCoroutine(JoinPool());
                else
                    Destroy(gameObject);
            }

        }
    }

    public void BeginGame()
    {
        StartCoroutine(JoinPool());
    }

    IEnumerator JoinPool()
    {
        WHOTMultiplayerManager.Instance.isOpponentReady = false;
        WHOTMultiplayerManager.Instance.isPlayerReady = false;

        WHOTMultiplayerManager.Instance.GetPhotonToken();
        Debug.Log("Join Pool");
        WWWForm form = new WWWForm();
        form.AddField("poolid", poolId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "joinpool", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();
        Debug.Log("Join Pool Response: " + www.downloadHandler.text);
        var joinPoolDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Error while trying o join pool: " + www.error);
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = www.error;
            WhotUiManager.instance.errorPopup.SetActive(true);
        }
        else
        {
            if (www.downloadHandler.text.Contains("error"))
            {
                var errorDetails = (IDictionary)joinPoolDetails["result"];
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = errorDetails["error"].ToString();
                WhotUiManager.instance.errorPopup.SetActive(true);
            }
            else
            {
                WHOTMultiplayerManager.Instance.startGameButton.gameObject.SetActive(false);
                WHOTMultiplayerManager.Instance.poolId = poolId;
                WHOTMultiplayerManager.Instance.canLeavePool = false;
                WHOTMultiplayerManager.Instance.winAmt = int.Parse(winningAmount);
                WHOTMultiplayerManager.Instance.betAmount = int.Parse(betAmount);


                if (senderName != UserDetailsManager.userName)
                {
                    Debug.Log("playerNameText.text: " + senderName + " " + UserDetailsManager.userName);
                    ChatGui.instance.sendPhotonNotification(senderName, UserDetailsManager.userName, " has accept your challenge");
                }

                RoomOptions roomOptions = new RoomOptions();
                roomOptions.PublishUserId = true;
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "ownername", "ownerid", "bet", "isAvailable", "appVer", "poolId", "isChallenge", "game"};
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ownername", UserDetailsManager.userName }, { "ownerid", UserDetailsManager.userId }, { "bet", betAmount }, { "isAvailable", true }, { "appVer", Application.version }, { "poolId", poolId }, { "isChallenge", true }, { "game", "Whot" } };
                //ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "bet", MAtchMakeString }, { "isAvailable", true }, { "appVer", Application.version } };
                roomOptions.MaxPlayers = 2;
                roomOptions.IsVisible = true;
                roomOptions.IsOpen = true;
                PhotonNetwork.JoinOrCreateRoom(poolId, roomOptions, TypedLobby.Default);
            }
        }
    }
}
