using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingScreen : MonoBehaviour
{
    public Toggle soundToggle, musicToggle, vibrationToggle;
    // Start is called before the first frame update
    void Start()
    {
        int i = PlayerPrefs.GetInt("IsSoundOn", 0);
        //soundOn.SetActive(i == 0 ? true : false);
        //soundOff.SetActive(!soundOn.activeInHierarchy);
        SetSound();
    }

    public void SetSound()
    {
        soundToggle.isOn = (PlayerPrefs.GetInt("IsSoundOn")== 1)? true : false ;
        musicToggle.isOn = (PlayerPrefs.GetInt("IsMusicOn")== 1)? true : false ;
        vibrationToggle.isOn = (PlayerPrefs.GetInt("IsVibrationOn") == 1)? true: false;
    }

    public void LogoutClick()
    {
        string phoneNum = PlayerPrefs.GetString("phone");
        string countryCode = PlayerPrefs.GetString("SelectedCountryCode");
        int countryCount = PlayerPrefs.GetInt("SelectedCountryCodeCount");
        string loggedType = PlayerPrefs.GetString("LoggedType");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetString("LoggedType", loggedType);
        PlayerPrefs.SetString("SelectedCountryCode", countryCode);
        PlayerPrefs.SetInt("SelectedCountryCodeCount", countryCount);
        Debug.Log(PlayerPrefs.GetInt("SelectedCountryCodeCount"));
        Debug.Log(PlayerPrefs.GetString("SelectedCountryCode"));
        PlayerPrefs.SetString("phone", phoneNum);
        StartCoroutine(Logout());        
    }
    IEnumerator Logout()
    {       
   
        UnityWebRequest www = UnityWebRequest.Get("http://18.191.157.16:4000/apis/logout");
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
        www.timeout = 120;
        yield return www.SendWebRequest();
        var logoutDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
        if (www.isNetworkError || www.error != null)
        {
            Debug.Log("Error Logging out: " + www.error.ToString());
        }
        else
        {
            if (www.downloadHandler.text.Contains("error"))
            {
                var errorDetails = (IDictionary)logoutDetails["result"];
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().title.text = "ERROR";
                WhotUiManager.instance.errorPopup.GetComponent<PopUP>().msg.text = errorDetails["error"].ToString();
                WhotUiManager.instance.errorPopup.SetActive(true);
            }
            else
            {
                //InitMenuScript.instance.switchUser();
                SceneManager.LoadScene("LoginSplash");
            }
        }
    }
    public void ResetSound()
    {
        int soundValue = (soundToggle.isOn) ? 1 : 0;
        PlayerPrefs.SetInt("IsSoundOn",soundValue);
        PlayerPrefs.Save();
        SoundManager.Instance.ResetSound();
    }

    public void ResetMusic()
    {
        int musicValue = (musicToggle.isOn) ? 1 : 0;
        PlayerPrefs.SetInt("IsMusicOn", musicValue);
        PlayerPrefs.Save();
        SoundManager.Instance.ResetMusic();
    }

    public void ResetVibration()
    {
        int vibrationValue = (vibrationToggle.isOn) ? 1 : 0;
        PlayerPrefs.SetInt("IsVibrationOn", vibrationValue);
        PlayerPrefs.Save();
    }

    public void AboutUs()
    {
        Application.OpenURL("http://192pool.com/about.html");
    }

    public void ContactUs()
    {
        Application.OpenURL("http://192pool.com/contact.html");
    }

    public void PrivacyPolicy()
    {
        Application.OpenURL("http://192pool.com/privacy.html");
    }

    public void TermsConditions()
    {
        Application.OpenURL("http://192pool.com/terms.html");
    }

    public void RefundPolicy()
    {
        Application.OpenURL("http://192pool.com/refund.html");
    }
}
