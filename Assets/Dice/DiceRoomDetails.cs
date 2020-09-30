using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DiceRoomDetails : MonoBehaviour
{
    public string playerName;
    public string playerId, poolId;
    public int betAmount, winningAmount;
    
    public Text playerNameText, betAmountText, winningAmountText;
    public Button StartGame;
    // Start is called before the first frame update
    public void SetDetails()
    {
        playerNameText.text = "USER NAME : " + playerName;
        betAmountText.text = "BET AMOUNT : " + betAmount;
        winningAmountText.text = "WINNING AMOUNT: " + winningAmount.ToString();
    }

    public void onJoin()
    {
        if (UserDetailsManager.userCoins < betAmount)
        {
            DiceMultiplayer.Instance.buyCoins.SetActive(true);
            return;
        }
        StartCoroutine(JoinPool());
    }

    IEnumerator JoinPool()
    {
        if(!PhotonNetwork.connected)
            DiceMultiplayer.Instance.GetPhotonToken();

        Debug.Log("Join Pool: " + poolId);
        Debug.Log("UserDetailsManager.accessToken: " + UserDetailsManager.accessToken);
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
            Debug.Log("Error while trying to join pool: " + www.error);
            /*
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = www.error;
            WhotUiManager.instance.errorPopup.SetActive(true);
            */
        }
        else if (www.downloadHandler.text.Contains("error"))
        {
            /*
            var errorDetails = (IDictionary)joinPoolDetails["result"];
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
            WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = errorDetails["error"].ToString();
            WhotUiManager.instance.errorPopup.SetActive(true);
            */
        }
        else
        {
            DiceMultiplayer.Instance.isOpponentReady = false;
            DiceMultiplayer.Instance.isPlayerReady = false;
            DiceMultiplayer.Instance.isChallenge = false;
            DiceMultiplayer.Instance.startGameButton.gameObject.SetActive(false);
            DiceMultiplayer.Instance.poolId = poolId;
            DiceMultiplayer.Instance.canLeavePool = true;
            DiceMultiplayer.Instance.winAmt = winningAmount;
            DiceMultiplayer.Instance.betAmount = betAmount;

            /* if (playerName != UserDetailsManager.userName)
             {
                 Debug.Log("playerNameText.text: " + playerName+ " " + UserDetailsManager.userName);
                 ChatGui.instance.sendPhotonNotification(playerName, UserDetailsManager.userName, " has joined your room");
             }
             */
            /*PhotonNetwork.player.NickName = playerName;
            PhotonNetwork.player.UserId = playerId;
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.PublishUserId = true;
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "ownername", "ownerid", "bet", "isAvailable", "appVer", "poolId", "isChallenge", "winAmt" };
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ownername", playerName }, { "ownerid", playerId }, { "bet", betAmount }, { "isAvailable", true }, { "appVer", Application.version }, { "poolId", poolId }, { "isChallenge", false }, { "winAmt", winningAmount } };
            roomOptions.MaxPlayers = 2;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            */
            PhotonNetwork.JoinRoom(poolId);
        }
    }
}
