using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public List<Text> userNameText;
    public List<Text> userCoinsText;
    public List<RawImage> userImage;
    public Text mobileNumText;
    //public Text AddCoinsField;
    public Text CoinsWon;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        if (PlayerPrefs.HasKey("Coins"))
            UserDetailsManager.userCoins = PlayerPrefs.GetInt("Coins");
        else
            PlayerPrefs.SetInt("Coins", 500);
    }

    public IEnumerator GetUserStats()
    {
        string url = "http://18.191.157.16:4000/apis/getuserstats";
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
            Debug.Log("User Stats Response: " + www.downloadHandler.text);
            var statsList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var userDetails = (IDictionary)statsList["result"];
            if (!www.downloadHandler.text.Contains("error"))
            {
                UserDetailsManager.userCoins = Mathf.RoundToInt(float.Parse(userDetails["coins"].ToString()));
                UserDetailsManager.userCoinsWon = int.Parse(userDetails["coins_won"].ToString());
                CoinsWon.text = "Total Winnings: " + UserDetailsManager.userCoinsWon.ToString();
                UpdateUserCoins();
            }
        }
    }

    public void UpdateUserName()
    {
        for(int i=0; i<userNameText.Count; i++)
        {
            userNameText[i].text = UserDetailsManager.userName;
        }
    }

    public void UpdateUserCoins()
    {
        PlayerPrefs.SetInt("Coins", UserDetailsManager.userCoins);
        for (int i = 0; i < userCoinsText.Count; i++)
        {
            userCoinsText[i].text = UserDetailsManager.userCoins.ToString();
        }
    }

    public void UpdateUserPic()
    {
        for (int i = 0; i < userImage.Count; i++)
        {
            userImage[i].texture = UserDetailsManager.userImageTexture;
        }
    }

    /*public void AddCoins()
    {
        int currentCoins = int.Parse(AddCoinsField.text);
        AddCoinsField.text = (currentCoins + 50).ToString();
    }

    public void SubtractCoins()
    {
        int currentCoins = int.Parse(AddCoinsField.text);
        if(currentCoins > 50)
            AddCoinsField.text = (currentCoins - 50).ToString();
    }

    public void BuyCoins()
    {
        UserDetailsManager.userCoins += int.Parse(AddCoinsField.text);
        UpdateUserCoins();
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
