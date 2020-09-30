using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MENU : MonoBehaviour
{
    public static MENU instance = null;
    public bool ifTableTen = false;
    public Text userName;
    public Text coins;
    public Text coinDisplay, winningCoins, displayName;
    public Text mobileDisplay;
    public GameObject table8x8, table10x10;
    public bool isPracticeClick;
    public GameObject mulitplayerScreen;
    public Text userNameOnsearch;
    // Start is called before the first frame update
    private void Awake()
    {
        // DontDestroyOnLoad(this);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        userName.text = UserDetailsManager.userName;
        coins.text = UserDetailsManager.userCoins.ToString();
        winningCoins.text = "Total Winnings:" +UserDetailsManager.userCoinsWon.ToString();
        displayName.text = UserDetailsManager.userName;
        mobileDisplay.text = UserDetailsManager.userPhone;
        coinDisplay.text = UserDetailsManager.userCoins.ToString();
        userNameOnsearch.text = UserDetailsManager.userName;
    }
    public void OnEasy()
    {
       
        PlayerPrefs.SetString("difficult", "easy");
    }
    public void OnMedium()
    {
       

            PlayerPrefs.SetString("difficult", "medium");


    }
    public void OnHard()
    {
       

            PlayerPrefs.SetString("difficult", "hard");

    }
    public void OnPracticeClick()
    {
        CheckersMultiplayer.Instance.IsMultiPlayer = false;
    }
    public void OnMultiPlayerClick()
    {
        CheckersMultiplayer.Instance.IsMultiPlayer = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Start()
    {
        PlayerPrefs.SetString("difficult", "easy");

    }
    public void Reload()
    {
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        Time.timeScale = 1;
    }
    public void OnClickNext()
    {
        SceneManager.LoadScene(1);
    }
    public void OnExitClick()
    {
        SceneManager.LoadScene("LoginSplash");

    }
    public void OnPressPractice()
    {
        isPracticeClick = true;
    }
    public void OnPressMultiplayer()
    {
        isPracticeClick = false;
    }
    public void OnSelectTable(int index)
    {
        if(index==0)
        {
            if(isPracticeClick)
            {
                table8x8.SetActive(true);
                table10x10.SetActive(false);
                CheckersMultiplayer.Instance.homeButttonSinglePlayer.SetActive(true);
            }
            else
            {
                mulitplayerScreen.SetActive(true);
            }
            CheckersMultiplayer.Instance.IsTableTen = false;
            PlayerPrefs.SetString("table10", "no");
           // SceneManager.LoadScene("8X8");
          
        }
        else
        {

           if (isPracticeClick)
            {
                table8x8.SetActive(false);
                table10x10.SetActive(true);
                CheckersMultiplayer.Instance.homeButttonSinglePlayer.SetActive(true);
            }
           else
            {
                mulitplayerScreen.SetActive(true);

            }
            PlayerPrefs.SetString("table10", "yes");
            CheckersMultiplayer.Instance.IsTableTen = true;

            // SceneManager.LoadScene("10X10");
        }
    }
}
