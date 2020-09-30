using Ludo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LudoChallengeManager : MonoBehaviour
{
    public string sender;
    public int betAmount;
    public int winAmount;
    public string poolId;
    [SerializeField] Text timerText;
    int timer;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void OnEnable()
    {
        StartCoroutine(startTimer());
    }
    public void acceptChallenge()
    {
        StartCoroutine(JoinPool());
        ChatGui.instance.sendPhotonNotification( sender, UserDetailsManager.userName, UserDetailsManager.userName + " has accepted your challenge in Ludo.");
        StartCoroutine(HideNotification());
    }

    public void declineChallenge()
    {
        ChatGui.instance.sendPhotonNotification(sender, UserDetailsManager.userName, UserDetailsManager.userName + " has declined your challenge in Ludo.");
        StartCoroutine(HideNotification());
    }

    IEnumerator HideNotification()
    {
        yield return new WaitForSecondsRealtime(1f);
        this.gameObject.SetActive(false);
    }

    IEnumerator startTimer()
    {

        yield return new WaitForSecondsRealtime(1f);
        timer += 1;
        Debug.Log("Timer: "+ timer);
        if (timer < 119)
        {
            StartCoroutine(startTimer());
        }
        else
        {
            declineChallenge();
            // StartCoroutine(SendChallengeResponseToServer("rejected"));
        }
    }
   /* IEnumerator SendChallengeResponseToServer(string myAction)
    {
        string message = "";
        if (myAction == "approved")
            message = UserDetailsManager.userName + " has accepted your challenge request!";
        else
            message = UserDetailsManager.userName + " has declined your challenge request!";
        Debug.Log("Challenge Response Message: " + message);
        WWWForm form = new WWWForm();
        form.AddField("action", myAction);
        form.AddField("poolid", poolId);
        //form.AddField("cid", challengeId);
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
    }*/

    IEnumerator JoinPool()
    {
        LudoMultiplayer.Instance.opponentsReady = 0;
        LudoMultiplayer.Instance.isPlayerReady = false;
        LudoMultiplayer.Instance.isChallenge = true;
        LudoMultiplayer.Instance.GetPhotonToken();
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
            Debug.Log("Error while trying to join pool: " + www.error);
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
            LudoMultiplayer.Instance.startGameButton.gameObject.SetActive(false);
            LudoMultiplayer.Instance.poolId = poolId;
            LudoMultiplayer.Instance.canLeavePool = false;
            LudoMultiplayer.Instance.winAmt = winAmount;
            LudoMultiplayer.Instance.betAmount = betAmount;

            /*if (playerName != UserDetailsManager.userName)
             {
                 Debug.Log("playerNameText.text: " + playerName+ " " + UserDetailsManager.userName);
                 ChatGui.instance.sendPhotonNotification(playerName, UserDetailsManager.userName, " has joined your room");
             }
             */
            GameManager.Instance.matchPlayerObject.GetComponent<SetMyData>().MatchPlayer();
            PhotonNetwork.JoinRoom(poolId);
        }
    }
}
