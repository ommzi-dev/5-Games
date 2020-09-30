using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Ludo;

public class LudoRoomDetails : MonoBehaviour
{
    public string playerName;
    public string playerId, poolId;
    public int betAmount, winningAmount, playerCount;
    
    public Text playerNameText, betAmountText, winningAmountText, playerCountText;
    
    // Start is called before the first frame update
    public void SetDetails()
    {
        playerNameText.text = "USER NAME : " + playerName;
        betAmountText.text = "BET AMOUNT : " + betAmount;
        winningAmountText.text = "WINNING AMOUNT: " + winningAmount.ToString();
        playerCountText.text = "PLAYERS: " + playerCount.ToString();
    }

    public void onJoin()
    {
        if (UserDetailsManager.userCoins < betAmount)
        {
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().title.text = "ERROR";
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().msg.text = "Bet Amount needs to be 500 or above.";
            LudoMultiplayer.Instance.errorDialog.SetActive(true);            
            return;
        }
        StartCoroutine(JoinPool());
    }

    IEnumerator JoinPool()
    {
        if(!PhotonNetwork.connected)
            LudoMultiplayer.Instance.GetPhotonToken();

        Debug.Log("Join Pool: " + poolId);
        Debug.Log("UserDetailsManager.accessToken: " + UserDetailsManager.accessToken);
        WWWForm form = new WWWForm();
        form.AddField("poolid", poolId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "joinpool", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();
        Debug.Log("Ludo Join Pool Response: " + www.downloadHandler.text);
        var joinPoolDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Ludo Error while trying to join pool: " + www.error);
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().title.text = "ERROR";
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().msg.text = www.error;
            LudoMultiplayer.Instance.errorDialog.SetActive(true);
        }
        else if (www.downloadHandler.text.Contains("error"))
        {
            var errorDetails = (IDictionary)joinPoolDetails["result"];
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().title.text = "ERROR";
            LudoMultiplayer.Instance.errorDialog.GetComponent<PopUP>().msg.text = errorDetails["error"].ToString();
            LudoMultiplayer.Instance.errorDialog.SetActive(true);
        }
        else
        {
            LudoMultiplayer.Instance.opponentsReady = 0;
            LudoMultiplayer.Instance.isPlayerReady = false;
            LudoMultiplayer.Instance.isChallenge = false;
            LudoMultiplayer.Instance.poolId = poolId;
            LudoMultiplayer.Instance.playerCount = playerCount;
            LudoMultiplayer.Instance.canLeavePool = true;
            LudoMultiplayer.Instance.winAmt = winningAmount;
            LudoMultiplayer.Instance.betAmount = betAmount;
            GameManager.Instance.requiredPlayers = playerCount;
            if (playerCount == 4)
            {
                GameManager.Instance.type = MyGameType.FourPlayer;
                StaticStrings.isFourPlayerModeEnabled = true;
                GameManager.Instance.sides = MySidesType.OneSide;
            }
            else
            {
                GameManager.Instance.type = MyGameType.TwoPlayer;
                StaticStrings.isFourPlayerModeEnabled = false;
            }

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
            //LudoMultiplayer.Instance.SearchOpponentPanel.SetActive(true);
            GameManager.Instance.offlineMode = false;
            GameManager.Instance.matchPlayerObject.GetComponent<SetMyData>().MatchPlayer();
            PhotonNetwork.JoinRoom(poolId);
        }
    }
}
