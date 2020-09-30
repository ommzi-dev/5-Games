using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using System.Collections.Generic;
using Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using AssemblyCSharp;
using System.Globalization;

public class PoolPlayFabManager : Photon.PunBehaviour, IChatClientListener
{

    public bool initCoinsAdded = false;
    public string PlayFabId;
    public string PlayFabTitleID;
    public string PhotonAppID;
    public string PhotonChatID;
    public bool multiGame = true;
    public bool roomOwner = false;
    private FacebookManager fbManager;
    public GameObject fbButton;
    private FacebookFriendsMenu facebookFriendsMenu;
    public ChatClient chatClient;
    private bool alreadyGotFriends = false;
    public GameObject menuCanvas;
    public GameObject MatchPlayersCanvas;
    public GameObject splashCanvas;
    public bool opponentReady = false;
    public bool imReady = false;
    public GameObject playerAvatar;
    public GameObject playerName;
    public GameObject backButtonMatchPlayers;


    public GameObject loginEmail;
    public GameObject loginPassword;
    public GameObject loginInvalidEmailorPassword;
    public GameObject loginCanvas;


    public GameObject regiterEmail;
    public GameObject registerPassword;
    public GameObject registerNickname;
    public GameObject registerInvalidInput;
    public GameObject registerCanvas;

    public GameObject resetPasswordEmail;
    public GameObject resetPasswordInformationText;

    public bool isInLobby = false;
    public bool isInMaster = false;

    void Awake()
    {
        //PlayerPrefs.DeleteAll();
        PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.PhotonCloud;
        PhotonNetwork.PhotonServerSettings.PreferredRegion = CloudRegionCode.eu;
        PhotonNetwork.PhotonServerSettings.AppID = PoolStaticStrings.PhotonAppID;
        PhotonNetwork.PhotonServerSettings.ChatAppID = PoolStaticStrings.PhotonChatID;
        PhotonNetwork.PhotonServerSettings.Protocol = ConnectionProtocol.Udp;
#if UNITY_IOS
PhotonNetwork.PhotonServerSettings.Protocol = ConnectionProtocol.Tcp;
#endif


        PlayFabTitleID = PoolStaticStrings.PlayFabTitleID;
        PhotonAppID = PoolStaticStrings.PhotonAppID;
        PhotonChatID = PoolStaticStrings.PhotonChatID;
        PlayFabSettings.TitleId = PlayFabTitleID;
        PhotonNetwork.OnEventCall += this.OnEvent;
        DontDestroyOnLoad(transform.gameObject);
        //GameManager.Instance.playfabManager = this;
        //        if (GameManager.Instance.logged) {
        //            showMenu();
        //        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        PhotonNetwork.OnEventCall -= this.OnEvent;
    }

    public void destroy()
    {
        if (this.gameObject != null)
            DestroyImmediate(this.gameObject);
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("Timeout 4");
        PhotonNetwork.BackgroundTimeout = 0.0f;
        //PhotonNetwork.SwitchToProtocol(ConnectionProtocol.Tcp);
        PlayFabTitleID = PoolStaticStrings.PlayFabTitleID;
        PhotonAppID = PoolStaticStrings.PhotonAppID;
        PhotonChatID = PoolStaticStrings.PhotonChatID;

        PoolGameManager.Instance.playfabManager = this;

        fbManager = GameObject.Find("FacebookManager").GetComponent<FacebookManager>();
        facebookFriendsMenu = PoolGameManager.Instance.facebookFriendsMenu;//fbButton.GetComponent <FacebookFriendsMenu> ();


        //		if (multiGame)
        //			Login ();
        //		else
        //			SceneManager.LoadScene ("GameScene");
    }

    // Update is called once per frame
    void Update()
    {

        if (chatClient != null) { chatClient.Service(); }


    }





    // handle events:
    private void OnEvent(byte eventcode, object content, int senderid)
    {
        if (eventcode == 199)
        {
            string dd = (string)content;
            string[] dd1 = dd.Split('-');
            PoolGameManager.Instance.opponentCueIndex = Int32.Parse(dd1[0]);
            PoolGameManager.Instance.opponentCueTime = Int32.Parse(dd1[1]);
            opponentReady = true;
            StartCoroutine(waitAndStartGame());
        }
        else if (eventcode == 198)
        {
            Debug.Log("Received 198");
            PoolGameManager.Instance.initPositions = (Vector3[])content;

            if (PoolGameManager.Instance.initPositions == null) Debug.Log("null pos");
            else Debug.Log("not null pos");
            PoolGameManager.Instance.receivedInitPositions = true;
        }
        else if (eventcode == 141 && PoolGameManager.Instance.MatchPlayersCanvas.activeSelf)
        {
            PoolGameManager.Instance.controlAvatars.waitingOpponentTime = PoolStaticStrings.photonDisconnectTimeout;
            PoolGameManager.Instance.controlAvatars.opponentActive = false;
            PoolGameManager.Instance.controlAvatars.messageBubbleText.GetComponent<Text>().text = PoolStaticStrings.waitingForOpponent + " " + PoolStaticStrings.photonDisconnectTimeout;
            PoolGameManager.Instance.controlAvatars.messageBubble.GetComponent<Animator>().Play("ShowBubble");

            StartCoroutine(PoolGameManager.Instance.controlAvatars.updateMessageBubbleText());
        }
        else if (eventcode == 142 && PoolGameManager.Instance.MatchPlayersCanvas.activeSelf)
        {
            PoolGameManager.Instance.controlAvatars.CancelInvoke("showLongTimeMessage");
            PoolGameManager.Instance.controlAvatars.opponentActive = true;
            PoolGameManager.Instance.controlAvatars.messageBubble.GetComponent<Animator>().Play("HideBubble");
        }


    }

    private IEnumerator waitAndStartGame()
    {
        while (!opponentReady || !imReady || (!PoolGameManager.Instance.roomOwner && !PoolGameManager.Instance.receivedInitPositions))
        {
            yield return 0;
        }

        startGameScene();
        //Invoke ("startGameScene", 2);

        opponentReady = false;
        imReady = false;
    }

    public void startGameScene()
    {

        SceneManager.LoadScene("GameScene");
    }

    public void resetPassword()
    {
        resetPasswordInformationText.SetActive(false);

        SendAccountRecoveryEmailRequest request = new SendAccountRecoveryEmailRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = resetPasswordEmail.GetComponent<Text>().text
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, (result) =>
        {
            resetPasswordInformationText.SetActive(true);
            resetPasswordInformationText.GetComponent<Text>().text = "Email sent to your address. Check your inbox";


        }, (error) =>
        {
            resetPasswordInformationText.SetActive(true);
            resetPasswordInformationText.GetComponent<Text>().text = "Account with specified email doesn't exist";
        });
    }

    public void setInitNewAccountData()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Cues", "'0'");
        data.Add("Chats", "");
        data.Add("UsedCue", "'0';'0';'0';'0'");
        data.Add("FirstTitleLogin", "1");

        PoolGameManager.Instance.ownedCues = "'0'";
        PoolGameManager.Instance.ownedChats = "";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
        {
            Debug.Log("Initial data added");
            addCoinsRequest(PoolStaticStrings.initCoinsCount);
            initCoinsAdded = true;
        }, (error1) =>
        {
            Debug.Log("Initial data add error " + error1.ErrorMessage + error1.ToString());
        }, null);
    }

    public void setInitNewAccountData(Dictionary<string, string> data1)
    {
        Dictionary<string, string> data = data1;


        data.Add("Cues", "'0'");
        data.Add("Chats", "");
        data.Add("UsedCue", "'0';'0';'0';'0'");
        data.Add("FirstTitleLogin", "1");

        PoolGameManager.Instance.ownedCues = "'0'";
        PoolGameManager.Instance.ownedChats = "";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
        {
            Debug.Log("Initial data added");
            addCoinsRequest(PoolStaticStrings.initCoinsCount);
        }, (error1) =>
        {
            Debug.Log("Initial data add error " + error1.ErrorMessage);
        }, null);
    }

    public void setUsedCue(int index, int power, int aim, int time)
    {
        PoolGameManager.Instance.cueIndex = index;
        PoolGameManager.Instance.cuePower = power;
        PoolGameManager.Instance.cueAim = aim;
        PoolGameManager.Instance.cueTime = time;

        if (PoolGameManager.Instance.cueController != null)
        {
            PoolGameManager.Instance.cueController.changeCueImage(index);
        }

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("UsedCue", "'" + index + "';" + "'" + power + "';" + "'" + aim + "';" + "'" + time + "'");
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
        {
            Debug.Log("Cue changed playfab");
        }, (error1) =>
        {
            Debug.Log("Cue changed playfab error " + error1.ErrorMessage);
        }, null);
    }

    public void updateBoughtCues(int index)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Cues", PoolGameManager.Instance.ownedCues + ";'" + index + "'");
        PoolGameManager.Instance.ownedCues = PoolGameManager.Instance.ownedCues + ";'" + index + "'";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
        {
            Debug.Log("Bought cue added");
        }, (error1) =>
        {
            Debug.Log("Bought cue error " + error1.ErrorMessage);
        }, null);
    }

    public void updateBoughtChats(int index)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Chats", PoolGameManager.Instance.ownedChats + ";'" + index + "'");

        PoolGameManager.Instance.ownedChats = PoolGameManager.Instance.ownedChats + ";'" + index + "'";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
        {
            Debug.Log("Bought chat added");
        }, (error1) =>
        {
            Debug.Log("Bought chat error " + error1.ErrorMessage);
        }, null);
    }

    public void addCoinsRequest(int count)
    {
        if (!PoolGameManager.Instance.offlineMode)
        {


            PoolGameManager.Instance.coinsCount += count;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Coins", "" + PoolGameManager.Instance.coinsCount);
            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                if (PoolGameManager.Instance.coinsTextMenu != null)
                {
                    updateCoinsTextMenu();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }

                if (PoolGameManager.Instance.coinsTextShop != null)
                {
                    updateCoinsTextShop();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }


                Debug.Log("Coins updated successfull ");
            }, (error1) =>
            {
                Debug.Log("Coins updated error " + error1.ErrorMessage);
            }, null);
        }
    }


    public void updateCoinsTextMenu()
    {
        if (PoolGameManager.Instance.coinsCount != 0)
        {
            PoolGameManager.Instance.coinsTextMenu.GetComponent<Text>().text = PoolGameManager.Instance.coinsCount.ToString("0,0", CultureInfo.InvariantCulture).Replace(',', ' ');
        }
        else
        {
            PoolGameManager.Instance.coinsTextMenu.GetComponent<Text>().text = "0";
        }

    }

    public void updateCoinsTextShop()
    {
        if (PoolGameManager.Instance.coinsCount != 0)
        {
            PoolGameManager.Instance.coinsTextShop.GetComponent<Text>().text = PoolGameManager.Instance.coinsCount.ToString("0,0", CultureInfo.InvariantCulture).Replace(',', ' ');
        }
        else
        {
            PoolGameManager.Instance.coinsTextShop.GetComponent<Text>().text = "0";
        }
    }

    public void getPlayerDataRequest()
    {
        GetUserDataRequest getdatarequest = new GetUserDataRequest()
        {
            PlayFabId = PoolGameManager.Instance.playfabManager.PlayFabId,
        };

        PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
        {

            Dictionary<string, UserDataRecord> data = result.Data;

            if (data.ContainsKey("Coins"))
            {
                Debug.Log("Got coins from playfab");
                PoolGameManager.Instance.coinsCount = Int32.Parse(data["Coins"].Value);
                // if (GameManager.Instance.coinsTextMenu != null)
                // {
                //     GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                // }
            }
            else
            {
                PoolGameManager.Instance.coinsCount = 0;
            }

            if (data.ContainsKey("UsedCue"))
            {
                string[] d = data["UsedCue"].Value.Split(';');
                PoolGameManager.Instance.cueIndex = Int32.Parse(d[0].Replace("'", ""));
                PoolGameManager.Instance.cuePower = Int32.Parse(d[1].Replace("'", ""));
                PoolGameManager.Instance.cueAim = Int32.Parse(d[2].Replace("'", ""));
                PoolGameManager.Instance.cueTime = Int32.Parse(d[3].Replace("'", ""));

                Debug.Log("Using cue: " + PoolGameManager.Instance.cueIndex);
            }

            if (data.ContainsKey("Chats"))
            {
                if (data["Chats"].Value != null)
                    PoolGameManager.Instance.ownedChats = data["Chats"].Value;
            }

            if (data.ContainsKey("Cues"))
            {

                PoolGameManager.Instance.ownedCues = data["Cues"].Value;
                Debug.Log("Owned Cues: " + PoolGameManager.Instance.ownedCues);
            }

            //SceneManager.LoadScene("Menu");
            StartCoroutine(loadSceneMenu());
        }, (error) =>
        {
            Debug.Log("Data updated error " + error.ErrorMessage);
        }, null);
    }


    private IEnumerator loadSceneMenu()
    {
        yield return new WaitForSeconds(0.1f);

        if (isInMaster && isInLobby)
        {
            SceneManager.LoadScene("Pool_Menu");
        }
        else
        {
            StartCoroutine(loadSceneMenu());
        }

    }

    public void RegisterNewAccountWithID()
    {
        string email = regiterEmail.GetComponent<Text>().text;
        string password = registerPassword.GetComponent<Text>().text;
        string nickname = registerNickname.GetComponent<Text>().text;

        registerInvalidInput.SetActive(false);

        if (Regex.IsMatch(email, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$") && password.Length >= 6 && nickname.Length > 0)
        {



            RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                Email = email,
                Password = password,
                RequireBothUsernameAndEmail = false
            };

            PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
            {
                PlayFabId = result.PlayFabId;
                Debug.Log("Got PlayFabID: " + PlayFabId);

                registerCanvas.SetActive(false);
                PlayerPrefs.SetString("email_account", email);
                PlayerPrefs.SetString("password", password);
                PlayerPrefs.SetString("LoggedType", "EmailAccount");
                PlayerPrefs.Save();
                PoolGameManager.Instance.nameMy = nickname;

                Dictionary<string, string> data = new Dictionary<string, string>();

                data.Add("LoggedType", "EmailAccount");
                //data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
                data.Add("PlayerName", UserDetailsManager.userName);
                //data.Add("PlayerAvatarUrl", GameManager.Instance.avatarMyUrl);

                setInitNewAccountData(data);
                //addCoinsRequest(StaticStrings.initCoinsCount);

                // UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
                // {
                //     //DisplayName = GameManager.Instance.nameMy,
                //     DisplayName = GameManager.Instance.playfabManager.PlayFabId
                // };

                // PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
                // {
                //     Debug.Log("Title Display name updated successfully");
                // }, (error) =>
                // {
                //     Debug.Log("Title Display name updated error: " + error.Error);

                // }, null);


                // Dictionary<string, string> data = new Dictionary<string, string>();

                // data.Add("LoggedType", "EmailAccount");
                // //data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
                // data.Add("PlayerName", GameManager.Instance.nameMy);
                // //data.Add("PlayerAvatarUrl", GameManager.Instance.avatarMyUrl);

                // UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
                // {
                //     Data = data,
                //     Permission = UserDataPermission.Public
                // };

                // PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
                // {
                //     Debug.Log("Data updated successfull ");
                // }, (error1) =>
                // {
                //     Debug.Log("Data updated error " + error1.ErrorMessage);
                // }, null);



                fbManager.showLoadingCanvas();
                StartCoroutine(checkIfCanGetPhotonToken());
                //GetPhotonToken();




                //GetFriends ();
            },
                (error) =>
                {

                    //                    
                    //if (error.ErrorMessage.Equals("EmailAddressNotAvailable"))
                    //{
                    registerInvalidInput.SetActive(true);
                    registerInvalidInput.GetComponent<Text>().text = error.ErrorMessage;
                    //}
                    Debug.Log("Error registering new account with email: " + error.ErrorMessage + "\n" + error.ErrorDetails);
                    //Debug.Log(error.ErrorMessage);
                });
        }
        else
        {
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = "Invalid input specified";
        }


    }

    public void LoginWithFacebook()
    {
        LoginWithFacebookRequest request = new LoginWithFacebookRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            AccessToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString
        };

        PlayFabClientAPI.LoginWithFacebook(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("LoggedType", "Facebook");
            data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
            if (result.NewlyCreated)
                data.Add("PlayerName", UserDetailsManager.userName);
            else
            {
                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = result.PlayFabId,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                {

                    Dictionary<string, UserDataRecord> data2 = result2.Data;


                    if (data2.ContainsKey("PlayerName"))
                    {
                        PoolGameManager.Instance.nameMy = data2["PlayerName"].Value;
                    }
                    else
                        data.Add("PlayerName", UserDetailsManager.userName);
                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);
            }
            data.Add("PlayerAvatarUrl", PoolGameManager.Instance.avatarMyUrl);


            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData(data);
                //addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                string id = PlayFabId;
                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = id,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                {
                    Dictionary<string, UserDataRecord> data2 = result2.Data;

                    if (!data2.ContainsKey("FirstTitleLogin"))
                    {
                        Debug.Log("First login for this title. Set initial data");
                        setInitNewAccountData(data);
                        //addCoinsRequest(StaticStrings.initCoinsCount);
                    }

                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);

                Debug.Log("(existing account)");
            }


            // UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            // {
            //     //DisplayName = GameManager.Instance.nameMy,
            //     DisplayName = GameManager.Instance.playfabManager.PlayFabId
            // };

            // PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            // {
            //     Debug.Log("Title Display name updated successfully");
            // }, (error) =>
            // {
            //     Debug.Log("Title Display name updated error: " + error.Error);

            // }, null);


            // Dictionary<string, string> data = new Dictionary<string, string>();

            // data.Add("LoggedType", "Facebook");
            // data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
            // if (result.NewlyCreated)
            //     data.Add("PlayerName", GameManager.Instance.nameMy);
            // else
            // {
            //     GetUserDataRequest getdatarequest = new GetUserDataRequest()
            //     {
            //         PlayFabId = result.PlayFabId,

            //     };

            //     PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
            //     {

            //         Dictionary<string, UserDataRecord> data2 = result2.Data;


            //         GameManager.Instance.nameMy = data2["PlayerName"].Value;
            //     }, (error) =>
            //     {
            //         Debug.Log("Data updated error " + error.ErrorMessage);
            //     }, null);
            // }
            // data.Add("PlayerAvatarUrl", GameManager.Instance.avatarMyUrl);

            // UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            // {
            //     Data = data,
            //     Permission = UserDataPermission.Public
            // };

            // PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            // {
            //     Debug.Log("Data updated successfull ");
            // }, (error1) =>
            // {
            //     Debug.Log("Data updated error " + error1.ErrorMessage);
            // }, null);



            StartCoroutine(checkIfCanGetPhotonToken());
            //GetPhotonToken();




            //GetFriends ();
        },
            (error) =>
            {
                Debug.Log("Error logging in player with custom ID: " + error.ErrorMessage + "\n" + error.ErrorDetails);
                PoolGameManager.Instance.connectionLost.showDialog();
                //Debug.Log(error.ErrorMessage);
            });
    }

    private string androidUnique()
    {
        AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityPlayerActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject unityPlayerResolver = unityPlayerActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass androidSettingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        return androidSettingsSecure.CallStatic<string>("getString", unityPlayerResolver, "android_id");
    }

    public void LoginWithEmailAccount()
    {


        loginInvalidEmailorPassword.SetActive(false);



        string email = "";
        string password = "";
        if (PlayerPrefs.HasKey("email_account"))
        {
            email = PlayerPrefs.GetString("email_account");
            password = PlayerPrefs.GetString("password");
            //email = UserDetailsManager.userEmail;
            //password = UserDetailsManager.userId;
        }
        else
        {
            email = loginEmail.GetComponent<Text>().text;
            password = loginPassword.GetComponent<Text>().text;

        }






        LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = email,
            Password = password
        };


        PlayFabClientAPI.LoginWithEmailAddress(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            Dictionary<string, string> data = new Dictionary<string, string>();

            loginCanvas.SetActive(false);
            PlayerPrefs.SetString("email_account", email);
            PlayerPrefs.SetString("password", password);
            PlayerPrefs.SetString("LoggedType", "EmailAccount");
            PlayerPrefs.Save();



            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData();
                //addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                string id = PlayFabId;
                GetUserDataRequest getdatarequest2 = new GetUserDataRequest()
                {
                    PlayFabId = id,

                };

                PlayFabClientAPI.GetUserData(getdatarequest2, (result2) =>
                {
                    Dictionary<string, UserDataRecord> data2 = result2.Data;

                    if (!data2.ContainsKey("FirstTitleLogin"))
                    {
                        Debug.Log("First login for this title. Set initial data");
                        setInitNewAccountData();
                        //addCoinsRequest(StaticStrings.initCoinsCount);
                    }

                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);
                Debug.Log("(existing account)");
            }


            GetUserDataRequest getdatarequest = new GetUserDataRequest()
            {
                PlayFabId = result.PlayFabId,

            };

            PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
            {

                Dictionary<string, UserDataRecord> data2 = result2.Data;


                PoolGameManager.Instance.nameMy = data2["PlayerName"].Value;
            }, (error) =>
            {
                Debug.Log("Data updated error " + error.ErrorMessage);
            }, null);





            fbManager.showLoadingCanvas();

            StartCoroutine(checkIfCanGetPhotonToken());
            //GetPhotonToken();

        },
             (error) =>
             {
                 loginInvalidEmailorPassword.SetActive(true);
                 Debug.Log("Error logging in player with custom ID: " + error.ErrorMessage);
                 //Debug.Log(error.ErrorMessage);

                 //GameManager.Instance.connectionLost.showDialog();
             });
    }

    public void Login()
    {
        string customId = "";
        if (PlayerPrefs.HasKey("unique_identifier"))
        {
            customId = PlayerPrefs.GetString("unique_identifier");
        }
        else
        {
            customId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("unique_identifier", customId);
        }




        Debug.Log("UNIQUE IDENTIFIER: " + customId);

        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = customId //SystemInfo.deviceUniqueIdentifier
        };



        PlayFabClientAPI.LoginWithCustomID(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            Dictionary<string, string> data = new Dictionary<string, string>();


            string name = result.PlayFabId;
            if (PlayerPrefs.HasKey("GuestPlayerName"))
            {
                name = PlayerPrefs.GetString("GuestPlayerName");
            }
            else
            {
                name = UserDetailsManager.userName;
                for (int i = 0; i < 6; i++)
                {
                    name += UnityEngine.Random.Range(0, 9);
                }
                PlayerPrefs.SetString(" ", name);
                PlayerPrefs.Save();
            }


            data.Add("LoggedType", "Guest");
            data.Add("PlayerName", name);

            if (result.NewlyCreated)
            {
                Debug.Log("(new account guest)");
                setInitNewAccountData(data);
                //addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                string id = PlayFabId;
                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = id,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                {
                    Dictionary<string, UserDataRecord> data2 = result2.Data;

                    if (!data2.ContainsKey("FirstTitleLogin"))
                    {
                        Debug.Log("First login for this title. Set initial data");
                        setInitNewAccountData(data);
                        //addCoinsRequest(StaticStrings.initCoinsCount);
                    }

                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);
                Debug.Log("(existing account)");
            }



            // string name = result.PlayFabId;
            // if (PlayerPrefs.HasKey("GuestPlayerName"))
            // {
            //     name = PlayerPrefs.GetString("GuestPlayerName");
            // }
            // else
            // {
            //     name = "Guest";
            //     for (int i = 0; i < 6; i++)
            //     {
            //         name += UnityEngine.Random.Range(0, 9);
            //     }
            //     PlayerPrefs.SetString("GuestPlayerName", name);
            //     PlayerPrefs.Save();
            // }


            // data.Add("LoggedType", "Guest");
            // data.Add("PlayerName", name);



            // UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            // {
            //     //DisplayName = name,
            //     DisplayName = PlayFabId
            // };

            // PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            // {
            //     Debug.Log("Title Display name updated successfully");
            // }, (error) =>
            // {
            //     Debug.Log("Title Display name updated error: " + error.Error + "   " + PlayFabId);

            // }, null);


            // UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            // {
            //     Data = data,
            //     Permission = UserDataPermission.Public
            // };




            // PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            // {
            //     Debug.Log("Data updated successfull ");
            // }, (error1) =>
            // {
            //     Debug.Log("Data updated error " + error1.ErrorMessage + error1.ToString());
            // }, null);

            PoolGameManager.Instance.nameMy = name;

            PlayerPrefs.SetString("LoggedType", "Guest");
            PlayerPrefs.Save();

            fbManager.showLoadingCanvas();

            StartCoroutine(checkIfCanGetPhotonToken());
            //GetPhotonToken();

        },
            (error) =>
            {
                Debug.Log("Error logging in player with custom ID:");
                Debug.Log(error.ErrorMessage);
                PoolGameManager.Instance.connectionLost.showDialog();
            });
    }



    IEnumerator checkIfCanGetPhotonToken()
    {
        yield return new WaitForSeconds(3f);
        UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
        {
            //DisplayName = name,
            DisplayName = PlayFabId
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
        {
            Debug.Log("Title Display name updated successfully");
        }, (error) =>
        {
            Debug.Log("Title Display name updated error: " + error.Error + "   " + PlayFabId);

        }, null);

        yield return new WaitForSeconds(3f);




        GetPhotonToken();


    }


    public void GetPlayfabFriends()
    {
        if (alreadyGotFriends)
        {
            Debug.Log("show firneds FFFF");
            if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
            {
                fbManager.getFacebookInvitableFriends();
            }
            else
            {

                facebookFriendsMenu.showFriends(null, null, null);
            }
        }
        else
        {
            Debug.Log("IND");
            GetFriendsListRequest request = new GetFriendsListRequest();
            request.IncludeFacebookFriends = true;
            PlayFabClientAPI.GetFriendsList(request, (result) =>
            {

                Debug.Log("Friends list Playfab: " + result.Friends.Count);
                var friends = result.Friends;

                List<string> playfabFriends = new List<string>();
                List<string> playfabFriendsName = new List<string>();
                List<string> playfabFriendsFacebookId = new List<string>();


                chatClient.RemoveFriends(PoolGameManager.Instance.friendsIDForStatus.ToArray());

                List<string> friendsToStatus = new List<string>();


                int index = 0;
                foreach (var friend in friends)
                {


                    playfabFriends.Add(friend.FriendPlayFabId);

                    Debug.Log("Title: " + friend.TitleDisplayName);
                    GetUserDataRequest getdatarequest = new GetUserDataRequest()
                    {
                        PlayFabId = friend.TitleDisplayName,
                    };


                    int ind2 = index;

                    PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                    {

                        Dictionary<string, UserDataRecord> data2 = result2.Data;
                        playfabFriendsName[ind2] = data2["PlayerName"].Value;
                        Debug.Log("Added " + data2["PlayerName"].Value);
                        PoolGameManager.Instance.facebookFriendsMenu.updateName(ind2, data2["PlayerName"].Value, friend.TitleDisplayName);

                    }, (error) =>
                    {

                        Debug.Log("Data updated error " + error.ErrorMessage);
                    }, null);

                    playfabFriendsName.Add("");

                    friendsToStatus.Add(friend.FriendPlayFabId);

                    index++;
                }

                PoolGameManager.Instance.friendsIDForStatus = friendsToStatus;

                chatClient.AddFriends(friendsToStatus.ToArray());



                PoolGameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
                //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

                if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
                {
                    fbManager.getFacebookInvitableFriends();
                }
                else
                {
                    PoolGameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
                    //facebookFriendsMenu.showFriends (null, null, null);
                }
                //alreadyGotFriends = true;


            }, OnPlayFabError);
        }


    }





    // Generic PlayFab callback for errors.
    void OnPlayFabError(PlayFabError error)
    {
        Debug.Log("Playfab Error: " + error.ErrorMessage);
    }

    // #######################  PHOTON  ##########################

    void GetPhotonToken()
    {
        GetPhotonAuthenticationTokenRequest request = new GetPhotonAuthenticationTokenRequest();
        request.PhotonApplicationId = PhotonAppID.Trim();//GameConstants.PhotonAppId.Trim();
                                                         // get an authentication ticket to pass on to Photon
        PlayFabClientAPI.GetPhotonAuthenticationToken(request, OnPhotonAuthenticationSuccess, OnPlayFabError);
    }


    public string authToken;
    // callback on successful GetPhotonAuthenticationToken request 
    void OnPhotonAuthenticationSuccess(GetPhotonAuthenticationTokenResult result)
    {
        string photonToken = result.PhotonCustomAuthenticationToken;
        Debug.Log(string.Format("Yay, logged in session token: {0}", photonToken));
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
        PhotonNetwork.AuthValues.AddAuthParameter("username", this.PlayFabId);
        PhotonNetwork.AuthValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
        PhotonNetwork.AuthValues.UserId = this.PlayFabId;
        PhotonNetwork.ConnectUsingSettings("1.0");
        PhotonNetwork.playerName = this.PlayFabId;

        //		PhotonNetwork.JoinLobby ();


        authToken = result.PhotonCustomAuthenticationToken;
        // chatClient = new ChatClient(this);
        // GameManager.Instance.chatClient = chatClient;
        // // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
        // ExitGames.Client.Photon.Chat.AuthenticationValues authValues = new ExitGames.Client.Photon.Chat.AuthenticationValues();
        // authValues.UserId = this.PlayFabId;
        // authValues.AuthType = ExitGames.Client.Photon.Chat.CustomAuthenticationType.Custom;
        // authValues.AddAuthParameter("username", this.PlayFabId);
        // authValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
        // chatClient.Connect(this.PhotonChatID, "1.0", authValues);
        getPlayerDataRequest();
        connectToChat();

    }

    public void connectToChat()
    {
        chatClient = new ChatClient(this);
        PoolGameManager.Instance.chatClient = chatClient;
        // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
        Photon.Chat.AuthenticationValues authValues = new Photon.Chat.AuthenticationValues();
        authValues.UserId = this.PlayFabId;
        authValues.AuthType = Photon.Chat.CustomAuthenticationType.Custom;
        authValues.AddAuthParameter("username", this.PlayFabId);
        authValues.AddAuthParameter("Token", authToken);
        chatClient.Connect(this.PhotonChatID, "1.0", authValues);
    }

    public void OnConnected()
    {
        Debug.Log("Photon Chat connected!!!");
        chatClient.Subscribe(new string[] { "invitationsChannel" });
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        PoolGameManager.Instance.opponentDisconnected = true;



        if (PoolGameManager.Instance.controlAvatars != null)
        {
            if (PoolGameManager.Instance.readyToAnimateCoins)
            {
                PoolGameManager.Instance.controlAvatars.playerDisconnected();

            }
            else
            {
                PoolGameManager.Instance.controlAvatars.playerRejected = true;
            }

            PoolGameManager.Instance.controlAvatars.hideMessageBubble();

            //GameManager.Instance.controlAvatars.playerRejected = true;
        }

        if (PoolGameManager.Instance.cueController != null)
        {
            PoolGameManager.Instance.cueController.HideAllControllers();
            Debug.Log("Player disconnected. You won");
            PoolGameManager.Instance.playerDisconnected = true;
            PhotonNetwork.LeaveRoom();
            PoolGameManager.Instance.iWon = true;
            PoolGameManager.Instance.gameControllerScript.showMessage(PoolGameManager.Instance.nameOpponent + " disconnected from room");
            PoolGameManager.Instance.stopTimer = true;
            PoolGameManager.Instance.cueController.youWonMessage.SetActive(true);
            PoolGameManager.Instance.audioSources[3].Play();
            PoolGameManager.Instance.cueController.youWonMessage.GetComponent<Animator>().Play("YouWinMessageAnimation");
        }
    }

    public void showMenu()
    {

        menuCanvas.gameObject.SetActive(true);

        playerName.GetComponent<Text>().text = PoolGameManager.Instance.nameMy;

        if (PoolGameManager.Instance.avatarMy != null)
            playerAvatar.GetComponent<Image>().sprite = PoolGameManager.Instance.avatarMy;

        splashCanvas.SetActive(false);
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed to a new channel - set online status!");

        //splashCanvas.SetActive (false);

        chatClient.SetOnlineStatus(ChatUserStatus.Online);


        // getPlayerDataRequest();
        //SceneManager.LoadScene("Menu");

        //menuCanvas.gameObject.SetActive (true);

        //playerName.GetComponent <Text> ().text = GameManager.Instance.nameMy;

        //if(GameManager.Instance.avatarMy != null)
        //	playerAvatar.GetComponent <Image> ().sprite = GameManager.Instance.avatarMy;


    }


    public void challengeFriend(string id, string message)
    {
        chatClient.SendPrivateMessage(id, "INVITE_SEND;" + id + this.PlayFabId + ";" + PoolGameManager.Instance.nameMy + ";" + message);

        //chatClient.PublishMessage( "invitationsChannel", "So Long, and Thanks for All the Fish!" );
        Debug.Log("Send invitation to: " + id);

        //		RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        //		PhotonNetwork.JoinOrCreateRoom(id+this.PlayFabId, roomOptions, TypedLobby.Default);

    }




    string roomname;
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (!sender.Equals(this.PlayFabId))
        {
            if (message.ToString().Contains("INVITE_SEND"))
            {
                string roomName = message.ToString().Split(';')[1];
                int payout = Int32.Parse(message.ToString().Split(';')[3]);
                PoolGameManager.Instance.tableNumber = Int32.Parse(message.ToString().Split(';')[4]);
                Debug.Log("INVITE_SEND " + message + "  " + sender + " room: " + roomName);
                PoolGameManager.Instance.payoutCoins = payout;
                PoolGameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(0, message.ToString().Split(';')[2], sender, roomName);

                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_ACCEPT"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_ACCEPT " + message + "  " + sender + " room: " + roomName);
                PoolGameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(2, message.ToString().Split(';')[2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_REJECT"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_REJECT " + message + "  " + sender + " room: " + roomName);
                PoolGameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(1, message.ToString().Split(';')[2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_START"))
            {
                string roomName = message.ToString().Split(';')[1];
                //				PhotonNetwork.JoinRoom (roomName);

                Debug.Log("INVITE_START " + message + "  " + sender + " room: " + roomName);
                //				GameManager.Instance.invitationDialog.GetComponent <PhotonChatListener> ().showInvitationDialog (1, message.ToString ().Split (';') [2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_STOP"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_STOP " + message + "  " + sender + " room: " + roomName);


                PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().playerRejected = true;

                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = sender,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
                {

                    Dictionary<string, UserDataRecord> data = result.Data;


                    if (data.ContainsKey("LoggedType"))
                    {
                        if (data["LoggedType"].Value.Equals("Facebook"))
                        {
                            //callApiToGetOpponentData (data ["FacebookID"].Value);
                            getOpponentData(data);
                        }
                        else
                        {
                            Debug.Log("DUPADUPA");
                            if (data.ContainsKey("PlayerName"))
                            {
                                PoolGameManager.Instance.nameOpponent = data["PlayerName"].Value;
                                PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                                backButtonMatchPlayers.SetActive(false);
                            }
                            else
                            {
                                PoolGameManager.Instance.nameOpponent = "Guest453678";
                                PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                                PoolGameManager.Instance.controlAvatars.playerRejected = true;
                            }


                            //SceneManager.LoadScene ("GameScene");
                        }
                    }
                    else
                    {
                        PoolGameManager.Instance.nameOpponent = "Guest453678";
                        PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        PoolGameManager.Instance.controlAvatars.playerRejected = true;
                    }

                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);



                //				GameManager.Instance.invitationDialog.GetComponent <PhotonChatListener> ().showInvitationDialog (1, message.ToString ().Split (';') [2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
        }
        //		Debug.Log ("INVITE RECEIVED " + message + "  " + sender);
        //
        //		roomname = message.ToString ();
        //
        //		Invoke ("join", 3.0f);

    }

    public void join()
    {
        PhotonNetwork.JoinRoom(roomname);
    }

    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnChatStateChange(ChatState state)
    {

    }


    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from photon");
        //GameManager.Instance.connectionLost.showDialog();
        switchUser();
    }

    public void DisconnecteFromPhoton()
    {
        PhotonNetwork.Disconnect();
    }

    public void switchUser()
    {



        PlayFabId = "";
        initCoinsAdded = false;
        //if(GameManager.Instance.playfabManager != null)
        PoolGameManager.Instance.playfabManager.destroy();
        //if(GameManager.Instance.facebookManager != null)    
        PoolGameManager.Instance.facebookManager.destroy();
        //if(GameManager.Instance.connectionLost != null)
        PoolGameManager.Instance.connectionLost.destroy();
        //if(GameManager.Instance.adsScript != null)
        //PoolGameManager.Instance.adsScript.destroy();
        PoolGameManager.Instance.avatarMy = null;
        PoolGameManager.Instance.logged = false;

        //PlayerPrefs.DeleteAll();
        PoolGameManager.Instance.resetAllData();
        PoolGameManager.Instance.coinsCount = 0;
        SceneManager.LoadScene("LoginSplash");
    }

    public void OnDisconnected()
    {
        Debug.Log("Chat disconnected called!!!!!!!!!!! Reconnect");
        connectToChat();

        //GameManager.Instance.connectionLost.showDialog();
    }



    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {

    }

    public void OnUnsubscribed(string[] channels)
    {

    }


    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log("STATUS UPDATE CHAT!");
        Debug.Log("Status change for: " + user + " to: " + status);

        bool foundFriend = false;
        for (int i = 0; i < PoolGameManager.Instance.friendsStatuses.Count; i++)
        {
            string[] friend = PoolGameManager.Instance.friendsStatuses[i];
            if (friend[0].Equals(user))
            {
                PoolGameManager.Instance.friendsStatuses[i][1] = "" + status;
                foundFriend = true;
                break;
            }
        }

        if (!foundFriend)
        {
            PoolGameManager.Instance.friendsStatuses.Add(new string[] { user, "" + status });
        }

        if (PoolGameManager.Instance.facebookFriendsMenu != null)
            PoolGameManager.Instance.facebookFriendsMenu.updateFriendStatus(status, user);
    }



    public override void OnJoinedLobby()
    {
        //getCoinsRequest();
        Debug.Log("OnJoinedLobby");

        isInLobby = true;
        //		PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        isInMaster = true;
        // when AutoJoinLobby is off, this method gets called when PUN finished the connection (instead of OnJoinedLobby())
        //PhotonNetwork.JoinRandomRoom();
        //		RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        //		PhotonNetwork.JoinOrCreateRoom("debugRoom", roomOptions, TypedLobby.Default);

        PhotonNetwork.JoinLobby();

    }

    public void JoinRoomAndStartGame()
    {
        //		RoomOptions roomOptions = new RoomOptions () { isVisible = false, maxPlayers = 2 };
        //		//PhotonNetwork.Joi
        //		PhotonNetwork.JoinOrCreateRoom("debugRoom", roomOptions, TypedLobby.Default);

        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "tbl", PoolGameManager.Instance.tableNumber }, { "isAvailable", true } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnPhotonRandomJoinFailed()
    {

        //RoomOptions roomOptions = new RoomOptions () { isVisible = true, maxPlayers = 2 };
        // PhotonNetwork.CreateRoom (null, roomOptions, TypedLobby.Default);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new String[] { "tbl", "isAvailable" };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "tbl", PoolGameManager.Instance.tableNumber }, { "isAvailable", true } };
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);


    }




    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        Debug.Log("Owner room: " + roomOwner);

        PoolGameManager.Instance.avatarOpponent = null;

        if (!roomOwner)
        {

            PoolGameManager.Instance.backButtonMatchPlayers.SetActive(false);

            GetUserDataRequest getdatarequest = new GetUserDataRequest()
            {
                PlayFabId = PhotonNetwork.otherPlayers[0].NickName,

            };

            PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
            {

                Dictionary<string, UserDataRecord> data = result.Data;


                if (data.ContainsKey("LoggedType"))
                {
                    if (data["LoggedType"].Value.Equals("Facebook"))
                    {
                        //callApiToGetOpponentData (data ["FacebookID"].Value);
                        getOpponentData(data);
                    }
                    else
                    {
                        Debug.Log("DUPADUPA");
                        if (data.ContainsKey("PlayerName"))
                        {
                            PoolGameManager.Instance.nameOpponent = data["PlayerName"].Value;
                            PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        }
                        else
                        {
                            PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                            PoolGameManager.Instance.controlAvatars.playerRejected = true;
                            PoolGameManager.Instance.nameOpponent = "Guest568253";
                        }


                        //SceneManager.LoadScene ("GameScene");
                    }
                }
                else
                {
                    PoolGameManager.Instance.nameOpponent = "Guest453678";
                    PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                    PoolGameManager.Instance.controlAvatars.playerRejected = true;
                }

            }, (error) =>
            {
                Debug.Log("Data updated error " + error.ErrorMessage);
            }, null);


        }



    }


    public override void OnCreatedRoom()
    {
        roomOwner = true;
        PoolGameManager.Instance.roomOwner = true;
        Debug.Log("OnCreatedRoom");

    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom called");
        roomOwner = false;
        PoolGameManager.Instance.roomOwner = false;
        PoolGameManager.Instance.resetAllData();

    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {

        PoolGameManager.Instance.controlAvatars.hideLongTimeMessage();
        //SceneManager.LoadScene ("GameScene");
        PhotonNetwork.room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isAvailable", false } });
        Debug.Log("New player joined");
        PoolGameManager.Instance.backButtonMatchPlayers.SetActive(false);
        //backButtonMatchPlayers.SetActive (false);
        GetUserDataRequest getdatarequest = new GetUserDataRequest()
        {
            PlayFabId = PhotonNetwork.otherPlayers[0].NickName,

        };

        PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
        {
            Debug.Log("Data updated successfull ");
            Dictionary<string, UserDataRecord> data = result.Data;



            if (data.ContainsKey("LoggedType"))
            {
                if (data["LoggedType"].Value.Equals("Facebook"))
                {

                    getOpponentData(data);
                    //callApiToGetOpponentData (data["FacebookID"].Value);

                    //				data.Add("PlayerName", GameManager.Instance.nameMy);
                    //				data.Add("PlayerAvatarUrl"

                }
                else
                {
                    if (data.ContainsKey("PlayerName"))
                    {
                        PoolGameManager.Instance.nameOpponent = data["PlayerName"].Value;
                        PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                    }
                    else
                    {
                        Debug.Log("Player joined room issue Error");
                        PoolGameManager.Instance.nameOpponent = "Guest675824";
                        PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        PoolGameManager.Instance.controlAvatars.playerRejected = true;
                    }




                    //SceneManager.LoadScene ("GameScene");

                }
            }
            else
            {
                PoolGameManager.Instance.nameOpponent = "Guest453678";
                PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                PoolGameManager.Instance.controlAvatars.playerRejected = true;
            }

        }, (error) =>
        {
            Debug.Log("Data updated error " + error.ErrorMessage);
        }, null);
    }

    private void getOpponentData(Dictionary<string, UserDataRecord> data)
    {
        if (data.ContainsKey("PlayerName"))
            PoolGameManager.Instance.nameOpponent = data["PlayerName"].Value;
        else
            PoolGameManager.Instance.nameOpponent = "Guest857643";
        if (data.ContainsKey("PlayerAvatarUrl"))
        {
            StartCoroutine(loadImageOpponent(data["PlayerAvatarUrl"].Value));
        }
        else
        {
            PoolGameManager.Instance.avatarOpponent = null;
            PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
            PoolGameManager.Instance.backButtonMatchPlayers.SetActive(false);
        }

    }

    public IEnumerator loadImageOpponent(string url)
    {
        // Load avatar image

        Debug.Log("Opponent image url: " + url);
        // Start a download of the given URL
        WWW www = new WWW(url);

        // Wait for download to complete
        yield return www;


        PoolGameManager.Instance.avatarOpponent = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f), 32);

        PoolGameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
        PoolGameManager.Instance.backButtonMatchPlayers.SetActive(false);

        //		SceneManager.LoadScene ("GameScene");
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }

    //	private void callApiToGetOpponentData(string id)
    //	{
    //
    //
    //		FB.API(id + "?fields=first_name", Facebook.Unity.HttpMethod.GET, delegate(IGraphResult result) {
    //			GameManager.Instance.nameOpponent = result.ResultDictionary ["first_name"].ToString ();
    //
    //			FB.API("/" + id + "/picture?type=square&height=92&width=92", Facebook.Unity.HttpMethod.GET, delegate(IGraphResult result2) {
    //				if (result2.Texture != null) {
    //					// use texture
    //					GameManager.Instance.avatarOpponent = Sprite.Create(result2.Texture, new Rect(0, 0, result2.Texture.width, result2.Texture.height), new Vector2(0.5f, 0.5f), 32);
    //					SceneManager.LoadScene ("GameScene");
    //				}
    //			});
    //		});
    //	}









    //	public void OnGUI()
    //	{
    //		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    //	}





}
