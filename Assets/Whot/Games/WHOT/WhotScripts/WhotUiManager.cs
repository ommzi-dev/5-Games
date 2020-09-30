using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class WhotUiManager : MonoBehaviour {

	public static WhotUiManager instance;
   
	public GameObject exitPopUp,winLosePopUp,userprofile, optionsPanel, errorPopup;
	public GameObject devicebackbtn;
    public Texture lose, win;
    //public Texture raysyello, redrays;
	public GameObject winloseimg, raysimg;
	public List<Text> userNameText, userCoinsText;
    public List<RawImage> userImage;
    public Text AddCoinsField, exitgameMessage;
    public Text CoinsWon, mobileNumText;
    public bool resultDisplayed,OtherPlayerForceQuit;
    public GameObject connecting;
    [Header("Room Creation")]
    [SerializeField] List<GameObject> roomPrefabs = new List<GameObject>();
    [SerializeField] InputField betAmount;
    public GameObject roomPrefab, roomParentObj;
    //public GameObject sharebtn;
    void Awake()
	{
		instance = this;
        OtherPlayerForceQuit = false;
	}
	void Start()
	{
        //		#if UNITY_WEBGL
        //		//webgl
        //		WhotCommonConstants.AssignTexture(userprofile.GetComponent<RawImage>(),AlertController.instance.profileimg);
        //
        //		username.text = WebServices.instance.currentUser.firstname;
        //
        //
        //		#else
        //		username.text = PlayerPrefs.GetString ("username","Mohit123");
        //
        //		WhotCommonConstants.AssignTexture(userprofile.GetComponent<RawImage>(),AlertController.instance.profileimg);
        //		#endif
        StartCoroutine(GetUserStats());
        //StartCoroutine(GetRoomStats());
        SetUIDefaultValues();
	}
	void OnDestroy()
	{
		instance = null;
	}

    void SetUIDefaultValues()
    {
        for (int i = 0; i < userNameText.Count; i++)
            userNameText[i].text = UserDetailsManager.userName;

        UpdateUserImage();
        UpdateCoins();
    }

    public void UpdateUserImage()
    {
        for (int i = 0; i < userImage.Count; i++)
            userImage[i].texture = UserDetailsManager.userImageTexture;
    }

    public void UpdateCoins()
    {
        PlayerPrefs.SetInt("Coins", UserDetailsManager.userCoins);
        for (int i = 0; i < userCoinsText.Count; i++)
            userCoinsText[i].text = UserDetailsManager.userCoins.ToString();
    }

	public void ShowExitPopUp()
	{
        if (WhotManager.instance.isOnlineMode)
            exitgameMessage.text = "You will lose game if you quit.\n Do you want to continue?";
        else
            exitgameMessage.text = "Do you want to quit the game?";
        exitPopUp.SetActive (true);
	}

    public void ShowOptionsPanel()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

	public void YesOnExitPopClicked()
	{

		if (!WhotConstants.isAI) {
			if (!WhotManager.instance.gameOver && WHOTMultiplayerManager.Instance.gameStarted) {
                WhotManager.instance.AnnounceWinner(false);
                WHOTMultiplayerManager.Instance.checkForceQuit();
                PhotonNetwork.LeaveRoom();
			}
			WhotManager.instance.gameOver = true;
			//WHOTLultiplayerManager.instance.Disconnect ();
		}

		SceneManager.LoadScene(SceneManager.GetActiveScene ().name);
	}    
  
    public void AddCoins()
    {
        int currentCoins = int.Parse(AddCoinsField.text);
        AddCoinsField.text = (currentCoins + 50).ToString();
    }

    public void SubtractCoins()
    {
        int currentCoins = int.Parse(AddCoinsField.text);
        if (currentCoins > 50)
            AddCoinsField.text = (currentCoins - 50).ToString();
    }

    public void BuyCoins()
    {
        UserDetailsManager.userCoins += int.Parse(AddCoinsField.text);
        SetUIDefaultValues();
    }

    public void CheckBetAmount()
    {
        
    }

    public IEnumerator GetUserStats()
    {
        string url = UserDetailsManager.serverUrl +"getuserstats";
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
            Debug.Log("User Stats Response: "+ www.downloadHandler.text);
            var statsList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var userDetails = (IDictionary)statsList["result"];
            UserDetailsManager.userCoins = Mathf.RoundToInt(float.Parse(userDetails["coins"].ToString()));
            UserDetailsManager.userCoinsWon = int.Parse(userDetails["coins_won"].ToString());
            mobileNumText.text = UserDetailsManager.userPhone;
            CoinsWon.text = "Total Winnings: " + UserDetailsManager.userCoinsWon.ToString();
            UpdateCoins();
        }
    }

    public IEnumerator TransactionPool(string winner, string looser)
    {
        Debug.Log("TransactionPool: "+ WHOTMultiplayerManager.Instance.poolId);
        WWWForm form = new WWWForm();
        form.AddField("looser_id", looser);
        form.AddField("winner_id", winner);
        form.AddField("game_id", "5"); /// 5 represents Whot game
        form.AddField("poolid", WHOTMultiplayerManager.Instance.poolId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "transaction", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

        www.timeout = 15;
        yield return www.SendWebRequest();
        WHOTMultiplayerManager.Instance.poolId = "";
        StartCoroutine(GetUserStats());
        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("Transation Completed with error: " + www.error);
        }
        else
        {
            Debug.Log("Transation Completed Successfully" + www.downloadHandler.text);            
        }
    }
    
    public void ShowWinLosePopUp(bool isWin)
	{
        if (!resultDisplayed)
        {
            resultDisplayed = true;
            WhotManager.instance.opponentDisconnectPopup.SetActive(false);
            if (WhotManager.instance.isOnlineMode)
                WHOTMultiplayerManager.Instance.SendGameDetails(isWin);
            WHOTMultiplayerManager.Instance.OutOfLives.SetActive(false);
            if (isWin)
            {
                //sharebtn.SetActive (true);
                winloseimg.GetComponent<RawImage>().texture = win;
                //raysimg.GetComponent<RawImage> ().texture = raysyello;
                if (WhotManager.instance.isOnlineMode)
                {
                    //StopAllCoroutines();
                    WhotManager.instance.WageredAmountText1.text = "COINS WON: ";
                    WhotManager.instance.WageredAmountText2.text = (WHOTMultiplayerManager.Instance.winAmt).ToString();
                    //UserDetailsManager.userCoins += WHOTMultiplayerManager.instance.winAmt;
                    WhotManager.instance.WageredAmountText1.gameObject.SetActive(true);
                    WhotManager.instance.WageredAmountText2.gameObject.SetActive(true);
                    StartCoroutine(TransactionPool(UserDetailsManager.userId, WhotOpponent.instance.userId));

                }
                else
                {

                    WhotManager.instance.WageredAmountText1.gameObject.SetActive(false);
                    WhotManager.instance.WageredAmountText2.gameObject.SetActive(false);
                    //SendStats (false, 1, 0);

                }
                SoundManager.Instance.PlaySound(3);
            }
            else
            {
                //sharebtn.SetActive (false);
                if (WhotManager.instance.isOnlineMode)
                {
                   // StopAllCoroutines();
                    WhotManager.instance.WageredAmountText1.text = "COINS LOST: ";
                    WhotManager.instance.WageredAmountText2.text = WHOTMultiplayerManager.Instance.betAmount.ToString();
                    //UserDetailsManager.userCoins -= WHOTMultiplayerManager.instance.winAmt;
                    WhotManager.instance.WageredAmountText1.gameObject.SetActive(true);
                    WhotManager.instance.WageredAmountText2.gameObject.SetActive(true);
                    StartCoroutine(TransactionPool(WhotOpponent.instance.userId, UserDetailsManager.userId));
                }
                else
                {

                    WhotManager.instance.WageredAmountText1.gameObject.SetActive(false);
                    WhotManager.instance.WageredAmountText2.gameObject.SetActive(false);
                    //	SendStats (false, -1, 0);

                }
                winloseimg.GetComponent<RawImage>().texture = lose;
                //raysimg.GetComponent<RawImage> ().texture = redrays;
                SoundManager.Instance.PlaySound(4);

            }
            WhotManager.instance.StopTimer();
            winLosePopUp.SetActive(true);
            devicebackbtn.SetActive(false);
            
        }
    }
		

	public void OnMenuButtonClicked()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene ().name);
	}

	public void OnRestartButtonClicked()
	{
		Debug.Log ("restart clicked");
		WhotConstants.loadGameplay = false;

		/*if (WhotManager.instance.isOnlineMode) {
			PlayerPrefs.SetInt ("WHOTRestartOnline", 1);
		
		} else {
			PlayerPrefs.SetInt ("WHOTRestart", 1);

		}*/
        if(WhotManager.instance.isOnlineMode)
        {
            PhotonNetwork.LeaveRoom();
        }
		PlayerPrefs.SetInt ("Exit",1);
        SceneManager.LoadScene(SceneManager.GetActiveScene ().buildIndex);
		Debug.Log ("Loading scene");

	}

    

    /*IEnumerator GetRoomStats()
    {
        string url = UserDetailsManager.serverUrl + "roomlist";
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
            Debug.Log("Room Stats Response: " + www.downloadHandler.text);
            var roomStats = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            if (!www.downloadHandler.text.Contains("error"))
            {
                var roomList = (IList)roomStats["result"];
                foreach (GameObject temp in roomPrefabs)
                {
                    Destroy(temp);
                }
                roomPrefabs.Clear();
                for (int k = 0; k < roomList.Count; k++)
                {
                    IDictionary roomDetails = (IDictionary)roomList[k];
                    GameObject roomInstance = Instantiate(roomPrefab, roomParentObj.transform);
                    roomPrefabs.Add(roomInstance);
                    roomInstance.name = roomDetails["user1"].ToString();
                    RoomDetails details = roomInstance.GetComponent<RoomDetails>();
                    details.playerId = roomDetails["userid1"].ToString();
                    details.opponentId = roomDetails["userid2"].ToString();
                    details.playerName = roomDetails["user1"].ToString();
                    details.opponentName = roomDetails["user2"].ToString();
                    details.poolId = roomDetails["poolid"].ToString();
                    details.betAmount = int.Parse(roomDetails["bet_amount"].ToString());
                    details.winningAmount = int.Parse(roomDetails["winning_amount"].ToString());
                    if (details.playerId == UserDetailsManager.userId)
                        details.canLeavePool = false;
                    else
                        details.canLeavePool = true;
                    details.SetDetails();
                }
            }
        }
        yield return new WaitForSeconds(10f);
        StartCoroutine(GetRoomStats());
    }*/

    #region Audio
    public void PlayAppClickSound()
    {
        SoundManager.Instance.PlaySound(0);
    }

    public void PlayButtonClickSound()
    {
        SoundManager.Instance.PlaySound(1);
    }

    public void PlayMusic(int clipNum)
    {
        SoundManager.Instance.PlayMusic(clipNum);
    }
    #endregion
}

