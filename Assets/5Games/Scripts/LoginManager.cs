using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.IO;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;
    public GameObject loaderScreen;
    [SerializeField] GameObject loginCanvas;
    [SerializeField] GameObject splashCanvas;
    [SerializeField] GameObject homePage;
    [SerializeField] Text verifyWarningText;
    [SerializeField] GameObject warningPopup, warningVerifyNumberPopup;
    [SerializeField] GameObject idLoginDialog;
    [SerializeField] GameObject idRegisterDialog;
    [SerializeField] GameObject forgetPasswordDialog;
    [SerializeField] GameObject phoneVerificationDialog;
    
    [Header("Login Panel References")]
     public InputField loginCountryCode;
    [SerializeField] InputField loginPhone;
    [SerializeField] InputField loginPin;
    [SerializeField] GameObject loginInvalidEmailorPassword;

    [Header("SignUp Panel References")]
    [SerializeField] InputField registerFirstName;
    [SerializeField] InputField registerLastName;
    [SerializeField] InputField registerUserName;
    [SerializeField] InputField registerEmail;
    [SerializeField] InputField registerPin;
    [SerializeField] InputField registerRetypePin;
     public InputField registerCountryCode;
    [SerializeField] InputField registerNumber;
    [SerializeField] InputField registerDOB;
    [SerializeField] Text registerGender;
    [SerializeField] GameObject registerInvalidInput;

    [Header("Reset Password References")]
    [SerializeField] InputField resetPasswordEmail;
    [SerializeField] GameObject resetPasswordInformationText;

    [Header("Verify Code References")]
    [SerializeField] List<InputField> verificationCode;

    [SerializeField] string uId;
    [SerializeField] Texture defaultPic;

    void Awake()
    {
        Debug.Log("LoginManager Awake");
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    void Start()
    {        Debug.Log("LoginManager start");
        if (Instance == null)
            Instance = this;

        Debug.Log("UID: " + SystemInfo.deviceUniqueIdentifier);
        Debug.Log("Server Url:" + UserDetailsManager.serverUrl);
    }

    public void OnEnable()
    {
        Debug.Log("OnEnable()");

        if (PlayerPrefs.HasKey("LoggedType") && PlayerPrefs.GetString("LoggedType") == "EmailAccount")
        {
            if (PlayerPrefs.HasKey("phone"))
            {
                ShowLoadingCanvas();
                loginPhone.text = PlayerPrefs.GetString("phone");
                if (PlayerPrefs.GetInt("LoggedIn") == 1 && PlayerPrefs.HasKey("pin") && UserDetailsManager.inGame)
                {
                    loginPin.text = PlayerPrefs.GetString("pin");
                    DoLogin();
                }
                else
                {
                    loginPin.text = "";
                }
            }
            else
            {
                idLoginDialog.SetActive(true);
            }
        }
        else
        {
            idLoginDialog.SetActive(true);
        }
    }

    public string androidUnique()
    {
        AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityPlayerActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject unityPlayerResolver = unityPlayerActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass androidSettingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        return androidSettingsSecure.CallStatic<string>("getString", unityPlayerResolver, "android_id");
    }

    public void ShowMessageDialog(string title, string message)
    {
        warningPopup.SetActive(true);
        warningPopup.GetComponent<PopUP>().title.text = title;
        warningPopup.GetComponent<PopUP>().msg.text = message;
    }

#region Login
    public void DoLogin()
    {
        StartCoroutine(InternetChecker.Instance.checkInternetConnection((isConnected) =>
        {
            if (isConnected)
            {
                if (string.IsNullOrEmpty(loginPin.text) || string.IsNullOrWhiteSpace(loginPin.text))
                {
                    ShowMessageDialog("Error", "Pin cannot be empty");
                    return;
                }
                loaderScreen.SetActive(false);
                StartCoroutine(Login());
            }
            else
            {
                loaderScreen.SetActive(true);
                InternetChecker.Instance.DisplayInternetError();
            }
        }));
    }

    IEnumerator Login()
    {
        loaderScreen.SetActive(true);
        ShowLoadingCanvas();
        uId = SystemInfo.deviceUniqueIdentifier;
#if UNITY_ANDROID && !UNITY_EDITOR
            uId = androidUnique();
#elif UNITY_IOS && !UNITY_EDITOR
        uId = Device.advertisingIdentifier;
#endif
        //string phoneNum = "+" + PlayerPrefs.GetString("CountryCode") + loginPhone.text;
        string phoneNum = loginPhone.text;
        if (!phoneNum.StartsWith("+"))
            phoneNum = "+" + phoneNum;
        WWWForm form = new WWWForm();
        form.AddField("email", phoneNum);
        form.AddField("pin", loginPin.text);
        Debug.Log("Phone: "+ phoneNum);
        Debug.Log("Pin: " + loginPin.text);
        form.AddField("device_id", uId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "login", form);
        www.timeout = 120;
        yield return www.SendWebRequest();
        Debug.Log("Helloooo");
        loaderScreen.SetActive(false);
        if (www.isNetworkError || www.isHttpError)
        {
            ShowMessageDialog("WARNING!!!!", www.error);
            HideLoadingCanvas();
        }
        else
        {
            Debug.Log("Login Response: " + www.downloadHandler.text);
            var roomList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var playerLoginDetails = (IDictionary)roomList["result"];
            
            HideLoadingCanvas();

            if (playerLoginDetails.Contains("error"))
            {
                Debug.Log("error " + www.error);                
                if((playerLoginDetails["error"].ToString().Contains("You need to verify your number")) || (playerLoginDetails["error"].ToString().Contains("You are logged in somewhere else")))
                {
                    verifyWarningText.text = playerLoginDetails["error"].ToString();
                    warningVerifyNumberPopup.SetActive(true);
                    PlayerPrefs.SetString("phone", loginPhone.text);
                    PlayerPrefs.SetString("pin", loginPin.text);
                }
                else
                {
                    ShowMessageDialog("WARNING!!!!", playerLoginDetails["error"].ToString());
                }
            }
            else
            {
                PlayerPrefs.SetString("phone", loginPhone.text);
                PlayerPrefs.SetString("pin", loginPin.text);
                PlayerPrefs.SetString("LoggedType", "EmailAccount");
                PlayerPrefs.Save();
                UserDetailsManager.accessToken = playerLoginDetails["token"].ToString();
                UserDetailsManager.userId = playerLoginDetails["user_id"].ToString();
                UserDetailsManager.userName = playerLoginDetails["username"].ToString();
                UserDetailsManager.isAdminPlayer = (int.Parse(playerLoginDetails["isadmin"].ToString())== 1)?true:false;
                UserDetailsManager.userPhone = PlayerPrefs.GetString("phone");
                UserDetailsManager.userCountryCode = PlayerPrefs.GetString("CountryCode");
                PlayerPrefs.SetInt("SelectedCountryCodeCount", PlayerPrefs.GetInt("CountryCodeCount"));
                PlayerPrefs.SetString("SelectedCountryCode", PlayerPrefs.GetString("CountryCode"));
                Debug.Log(PlayerPrefs.GetInt("SelectedCountryCodeCount"));
                Debug.Log(PlayerPrefs.GetString("SelectedCountryCode"));
                UIManager.Instance.mobileNumText.text = UserDetailsManager.userPhone;
                //Debug.Log("phone:"+ PlayerPrefs.GetString("phone"));
                PlayerPrefs.SetInt("LoggedIn", 1);
                UserDetailsManager.userCoins = Mathf.RoundToInt(float.Parse(playerLoginDetails["coins"].ToString()));                
                UIManager.Instance.UpdateUserName();
                UIManager.Instance.UpdateUserCoins();
                UserDetailsManager.inGame = true;
                StartCoroutine(UIManager.Instance.GetUserStats());
                
                try
                {
                    UserDetailsManager.userImageString = playerLoginDetails["user_dp"].ToString();
                    if (!string.IsNullOrEmpty(UserDetailsManager.userImageString))
                    {
                        byte[] Avtbytes = Convert.FromBase64String(UserDetailsManager.userImageString);
                        Texture2D tex = new Texture2D(1, 1);
                        tex.LoadImage(Avtbytes);
                        UserDetailsManager.userImageTexture = tex;
                        Debug.Log("In Try");
                    }
                }
                catch {
                    UserDetailsManager.userImageTexture = defaultPic; //Needs to be loaded only incase of login
                   
                    //File.WriteAllBytes(playerImgPath, screenshot.EncodeToPNG());
                    Debug.Log("In Catch");
                }
              
                    
                UIManager.Instance.UpdateUserPic();
                /*GameManager.Instance.logged = true;                
                GameManager.Instance.friendids = playerDic["friends"] as IList;
                GameManager.Instance.usergender = playerDic["gender"].ToString();
                Debug.Log(GameManager.Instance.friendids);
                Debug.Log("error" + GameManager.Instance.friendids.Count);
                if (playerDic["avatar"].ToString() == "")
                {
                    GameManager.Instance.avatarMycode = -1;
                }
                else if (playerDic["avatar"].ToString().Length > 5)
                {
                    byte[] Avtbytes = Convert.FromBase64String(playerDic["avatar"].ToString());
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(Avtbytes);
                    GameManager.Instance.avatarMy = Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2());
                    GameManager.Instance.avatarMycode = 101;
                    dp = tex;
                }
                else
                {
                    Debug.Log("myavatarcode" + playerDic["avatar"].ToString());
                    GameManager.Instance.avatarMycode = Convert.ToInt32(playerDic["avatar"].ToString());
                }
                playFabManager.GetPhotonToken();
                StartCoroutine(playFabManager.loadSceneMenu());*/
                PushNotificatonCall();
                LoadHomePage();
            }
        }
    }  
#endregion

#region Reset Password
    public void ResetPassword()
    {
        StartCoroutine(InternetChecker.Instance.checkInternetConnection((isConnected) =>
        {
            if (isConnected)
            {
                StartCoroutine(resetPswd());
            }
            else
            {
                //Loader.SetActive(false);
                InternetChecker.Instance.DisplayInternetError();
            }
        }));
    }

    IEnumerator resetPswd()
    {
        WWWForm form = new WWWForm();
        Debug.Log("Sending Reset mail to: "+ resetPasswordEmail.text);
        form.AddField("email", resetPasswordEmail.text);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "forgotpassword", form);

        www.timeout = 15;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            resetPasswordInformationText.SetActive(true);
            resetPasswordEmail.text = "";  
            resetPasswordInformationText.GetComponent<Text>().text = www.error.ToString();
        }
        else
        {
            resetPasswordInformationText.SetActive(true);
            PlayerPrefs.SetString("email_account", resetPasswordEmail.text);
            resetPasswordEmail.text = "";
            resetPasswordInformationText.GetComponent<Text>().text = "Email sent to your address. Check your inbox";
        }
    }
    #endregion

    #region SignUp

    public void onTerm()
    {
        Application.OpenURL("https://www.ommzi.com/");
    }

    public void SetDOB()
    {
        int len = registerDOB.text.Length;
        //Debug.Log("DOB:"+registerDOB.text+ " " + registerDOB.text.Length);
        if (!registerDOB.text.EndsWith("/"))
        {
            switch (len)
            {
                case 3:
                    registerDOB.text = registerDOB.text.Substring(0,2) + "/"+registerDOB.text.Substring(2,1);
                    break;
                case 6:
                    registerDOB.text = registerDOB.text.Substring(0, 5) + "/" + registerDOB.text.Substring(5, 1);
                    break;
            }
        }
    }

    IEnumerator Signup(string email, string username, string pin, string gender, string number)
    {
        loaderScreen.SetActive(true);
        uId = SystemInfo.deviceUniqueIdentifier;
#if UNITY_ANDROID && !UNITY_EDITOR
            uId = androidUnique();
#elif UNITY_IOS && !UNITY_EDITOR
        uId = Device.advertisingIdentifier;
#endif
        WWWForm form = new WWWForm();
        //form.AddField("age", age);
        //string pnumber = "+" + PlayerPrefs.GetString("CountryCode") + number;
        string pnumber = number;
        if (!pnumber.StartsWith("+"))
            pnumber = "+" + pnumber;
        Debug.Log("email: "+ email);
        Debug.Log("username: " + username);
        Debug.Log("pin: "+ pin);
        Debug.Log("phone: " + pnumber);
        Debug.Log("gender: " + gender);
        Debug.Log("device_id: " + uId);
        form.AddField("email", email);
        form.AddField("username", username);
        form.AddField("pin", pin);
        form.AddField("phone", pnumber);
        form.AddField("gender", gender);
        form.AddField("device_id", uId);
        //Debug.Log(registerNickname.GetComponent<Text>().text + "   " + regiterEmail.GetComponent<Text>().text + "   " + registerPassword.GetComponent<Text>().text);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "signup", form);
        www.timeout = 15;
        yield return www.SendWebRequest();
        loaderScreen.SetActive(false);
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("Sign Up Error: "+www.error);
            //splashCanvas.SetActive(false);
            HideLoadingCanvas();
            ShowMessageDialog("WARNING!!!!", "Invalid Details Entered");
        }
        else
        {
            Debug.Log("Sign Up Response: " + www.downloadHandler.text);
            var userList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var signupDetails = (IDictionary)userList["result"];
            HideLoadingCanvas();

            if (signupDetails.Contains("error"))
            {
                Debug.Log("Sign Up Error: " + www.error);
                ShowMessageDialog("WARNING!!!!", signupDetails["error"].ToString());                
            }
            else
            {
                if (signupDetails["sms"].ToString() == "sent")
                {
                    PlayerPrefs.SetString("email_account", email);
                    PlayerPrefs.SetString("password", username);
                    PlayerPrefs.SetString("pin", pin);
                    PlayerPrefs.SetString("phone", number);
                    PlayerPrefs.SetString("LoggedType", "EmailAccount");
                    PlayerPrefs.Save();
                    //GameManager.Instance.nameMy = friendDic["username"].ToString();
                    //GameManager.Instance.myid = friendDic["id"].ToString();
                    //GameManager.Instance.facebookManager.playerToken = friendDic["token"].ToString();
                    //GameManager.Instance.avatarMycode = Convert.ToInt32(friendDic["avatar"].ToString());
                    //GameManager.Instance.friendids = friendDic["friends"] as List<int>;
                    ShowVerificationDialog();
                    //setInitNewAccountData();
                    //addCoinsRequest(PoolStaticStrings.initCoinsCount);
                    //GetPhotonToken();
                    //StartCoroutine(loadSceneMenu());
                }
                else
                {
                    ShowMessageDialog("Message", "Enter a valid Phone Number to receive verification code.");
                }
            }
        }
    }

    public void RegisterNewAccountWithID()
    {
        string email = registerEmail.text;
        string username = registerUserName.text;
        string pin = registerPin.text;
        string retypepin = registerRetypePin.text;
        //int age = 20;
        string gender = registerGender.text;
        string number = registerNumber.text;        
        registerInvalidInput.SetActive(false);

        if(pin != retypepin)
        {
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = "Pin Mismatch!!";
            return;
        }

       // if (Convert.ToInt32(age) >= 18 )
      //  {
            if (number.Length >= 8)
            {
                StartCoroutine(InternetChecker.Instance.checkInternetConnection((isConnected) =>
                {
                    if (isConnected)
                    {
                        ShowLoadingCanvas();
                        StartCoroutine(Signup(email, username, pin, gender, number));
                    }
                    else
                    {
                        //Loader.SetActive(false);
                        InternetChecker.Instance.DisplayInternetError();
                    }
                }));
            }
            else
            {
                registerInvalidInput.SetActive(true);
                registerInvalidInput.GetComponent<Text>().text = "Invalid mobile number";
            }
       /* }
        else
        {
            if (Convert.ToInt32(age) < 18)
            {
                ShowMessageDialog("Restricted Entry", "You must be 18 years of age or older to register with us.");
            }
            else
            {
                registerInvalidInput.SetActive(true);
                registerInvalidInput.GetComponent<Text>().text = "Invalid input specified";
            }
        }*/
    }
#endregion

#region Verify Mobile Number
    public void VerifyPhoneNumber()
    {
        StartCoroutine(InternetChecker.Instance.checkInternetConnection((isConnected) =>
        {
            if (isConnected)
            {
                StartCoroutine(VerifyCode());
            }
            else
            {
                //Loader.SetActive(false);
                InternetChecker.Instance.DisplayInternetError();
            }
        }));
    }

    IEnumerator VerifyCode()
    {
        string code = "";
        for (int x = 0; x < verificationCode.Count; x++)
            code = code + verificationCode[x].text;
       
        WWWForm form = new WWWForm();
        string num = PlayerPrefs.GetString("phone");
        form.AddField("phone", num);
        form.AddField("code", code);
        Debug.Log("code: " + code);
        Debug.Log("phone: " + num);                
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "phoneverifyCheck", form);
        www.timeout = 30;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            ShowMessageDialog("WARNING!!!!", www.error);
            HideLoadingCanvas();
        }
        else
        {
            Debug.Log("Verification Response: " + www.downloadHandler.text);
            var roomList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var verificationDic = (IDictionary)roomList["result"];
            HideLoadingCanvas();
            //string success = playerDic["success"].ToString() ;
            if (verificationDic.Contains("error"))
            {
                Debug.Log("error " + www.error);
                ShowMessageDialog( "WARNING!!!!", verificationDic["error"].ToString());                
            }
            else
            {
                CloseVerificationDialog();
                ShowLoginDialog();
                ShowMessageDialog("Message", "Phone Number Verified Successfully!");
                yield return new WaitForSecondsRealtime(2f);
                loginPhone.text = PlayerPrefs.GetString("phone");
                loginPin.text = PlayerPrefs.GetString("pin");
                StartCoroutine(Login());
            }
        }
    }
#endregion

#region Resend Verification Code
    public void ResendVerificationCode()
    {
        StartCoroutine(InternetChecker.Instance.checkInternetConnection((isConnected) =>
        {
            if (isConnected)
            {
                StartCoroutine(ResendCode());
            }
            else
            {
                //Loader.SetActive(false);
                InternetChecker.Instance.DisplayInternetError();
            }
        }));
    }

    IEnumerator ResendCode()
    {
        WWWForm form = new WWWForm();
        string num = PlayerPrefs.GetString("phone");
        form.AddField("phone", num);
        Debug.Log("phone: " + num);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "phoneverifyReq", form);
        www.timeout = 30;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            ShowMessageDialog("WARNING!!!",www.error);
        }
        else
        {
            Debug.Log("Resend Code Response: " + www.downloadHandler.text);
            var roomList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

            var verificationDic = (IDictionary)roomList["result"];

            //string success = playerDic["success"].ToString() ;
            if (verificationDic.Contains("error"))
            {
                Debug.Log("error " + www.error);
                ShowMessageDialog("WARNING!!!!", verificationDic["error"].ToString());
            }
            else
            {
                ShowMessageDialog("Message", "Verification Code Sent.");
                idLoginDialog.SetActive(false);
                ShowVerificationDialog();
            }
        }
    }
#endregion

#region PushWoosh Notifications
    IEnumerator OnTokenUpdate(string _token)
    {
        WWWForm form = new WWWForm();
        uId = SystemInfo.deviceUniqueIdentifier;

#if UNITY_ANDROID && !UNITY_EDITOR
            uId = androidUnique();
#elif UNITY_IOS
            uId = Device.advertisingIdentifier;
#endif
        Debug.Log("push_id token: " + _token);
        Debug.Log("device_id token: " + uId);
        form.AddField("push_id", _token);
        form.AddField("device_id", uId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "updatepushtoken", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
        www.timeout = 15;
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            Debug.Log("Network Error");
        }
        else
        {
            Debug.Log("OnTokenUpdated " + www.downloadHandler.text);
            StartCoroutine(OnRegisterForPush());
        }
    }

    IEnumerator OnRegisterForPush()
    {
        WWWForm form = new WWWForm();
        uId = SystemInfo.deviceUniqueIdentifier;

#if UNITY_ANDROID && !UNITY_EDITOR
                    uId = androidUnique();
#elif UNITY_IOS
                    uId = Device.advertisingIdentifier;
#endif
        Debug.Log("Uid RegisterForPush: " + uId);
        form.AddField("device_id", uId);
        UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "registerdevice", form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
        www.timeout = 15;
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            Debug.Log("Network Error: " + www.error);
        }
        else
        {
            Debug.Log("user registered for push " + www.downloadHandler.text);
        }
    }

    public void PushNotificatonCall()
    {
        Pushwoosh.ApplicationCode = "E1F52-634BF";
        Pushwoosh.FcmProjectNumber = "1002878343355";
        Pushwoosh.Instance.OnRegisteredForPushNotifications += OnRegisteredForPushNotifications;
        Pushwoosh.Instance.OnFailedToRegisteredForPushNotifications += OnFailedToRegisteredForPushNotifications;
        Pushwoosh.Instance.OnPushNotificationsReceived += OnPushNotificationsReceived;
        Pushwoosh.Instance.RegisterForPushNotifications();
    }

    void OnRegisteredForPushNotifications(string token)
    {
        StartCoroutine(InternetChecker.Instance.checkInternetConnection((isConnected) =>
        {
            if (isConnected)
            {
                Debug.Log("Received token: \n" + token);
                if (!string.IsNullOrEmpty(token))
                {
                    StartCoroutine(OnTokenUpdate(token));
                }
            }
            else
            {
                //Loader.SetActive(false);
                InternetChecker.Instance.DisplayInternetError();
            }
        }));
    }

    void OnFailedToRegisteredForPushNotifications(string error)
    {
        // handle here
        Debug.Log("Error ocurred while registering to push notifications: \n" + error);
    }

    void OnPushNotificationsReceived(string payload)
    {
        // handle here
        Debug.Log("Received push notificaiton: \n" + payload);
    }
    #endregion

    #region UIFunctions
    
    public IEnumerator loadSceneMenu()
    {
        yield return new WaitForSeconds(3f);

        // if (isInMaster && isInLobby)
        // {
        SceneManager.LoadScene("LoginSplash");
        //  }
        //  else
        //  {
        //     StartCoroutine(loadSceneMenu());
        //   }
    }

    public void LoadGamePage(int game)
    {
        switch(game)
        {
            case 1:
                Debug.Log("Ludo Selected");
                SceneManager.LoadScene("LudoMenu");
                break;
            case 2:
                Debug.Log("Checkers Selected");
               // ShowMessageDialog("MESSAGE", "Checkers is Under Development!!");
                SceneManager.LoadScene("main");

                break;
            case 3:
                Debug.Log("Snooker Selected");
                //ShowMessageDialog("MESSAGE", "Snooker is Under Development!!");
                SceneManager.LoadScene("PoolLoginSplash");
                break;
            case 4:
                Debug.Log("Dice Selected");
                SceneManager.LoadScene("Dice");
              //  ShowMessageDialog("MESSAGE", "Dice Game is Under Development!!");
                break;
            case 5:
                Debug.Log("Whot Selected");
                SceneManager.LoadScene("WhotPlay");
                break;
        }
    }

    public void LoadHomePage()
    {
        splashCanvas.SetActive(false);
        idLoginDialog.SetActive(false);
        homePage.SetActive(true);
    }

    public void ShowVerificationDialog()
    {
        idRegisterDialog.SetActive(false);
        phoneVerificationDialog.SetActive(true);
        if(warningPopup.activeSelf)
            warningPopup.SetActive(false);
    }

    public void CloseVerificationDialog()
    {
        for(int x=0; x< verificationCode.Count; x++)
            verificationCode[x].text = "";
        phoneVerificationDialog.SetActive(false);
    }

    public void ShowLoginDialog()
    {
        Debug.Log("Show Login Dialog Called!!");
        idLoginDialog.SetActive(true);
        loaderScreen.SetActive(true);
    }

    public void ShowRegisterDialog()
    {
        idLoginDialog.SetActive(false);
        idRegisterDialog.SetActive(true);
    }

    public void CloseRegisterDialog()
    {
        registerEmail.text = "";
        registerRetypePin.text = "";
        registerPin.text = "";
        registerFirstName.text = "";
        registerLastName.text = "";
        registerNumber.text = "+";
        registerGender.text = "";
        registerDOB.text = "";
        registerUserName.text = "";
        registerInvalidInput.SetActive(false);
        loginCanvas.SetActive(true);
        idRegisterDialog.SetActive(false);
    }

    public void CloseForgetPasswordDialog()
    {
        Debug.Log("CloseForgetPasswordDialog!!");
        resetPasswordEmail.text = "";
        resetPasswordInformationText.SetActive(false);
        forgetPasswordDialog.SetActive(false);
        idLoginDialog.SetActive(true);
    }

    public void showForgetPasswordDialog()
    {
        forgetPasswordDialog.SetActive(true);
        idLoginDialog.SetActive(false);
    }

    public void ShowLoadingCanvas()
    {
        Debug.Log("Show Loading Canvas Called!!");
        loginCanvas.SetActive(false);
        splashCanvas.SetActive(true);
    }

    public void HideLoadingCanvas()
    {
        Debug.Log("Hide Loading Canvas Called!!");
        loginCanvas.SetActive(true);
        splashCanvas.SetActive(false);
    }
#endregion
}
