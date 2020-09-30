using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class DiceGameManager : MonoBehaviour
{
    public static DiceGameManager instance = null;
    public GameObject[] dice;
    public GameObject RollButton,TryAgainButton;
    public bool isDiceOneStay, isDiceTwoStay;
    public Text cpuText;
    public bool isPressOnRoll;
    public GameObject planner;
    public Text userName, coins;
    public Text gamePlayText;
    public Texture[] winLoseSp;
    public RawImage winLoseImg;
    public int playerCount;
    public int cpuCount;
    public Text timerText;
    public bool isMultiplayer;
    public Text multiplayerScoreCount;
    public int multiplayerPlayerCount;
    public bool isWin;
    // Start is called before the first frame update
    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        instance = this;
        TryAgainButton.SetActive(false);
        RollButton.SetActive(true);
        planner.GetComponent<BoxCollider>().enabled = false;

        userName.text = UserDetailsManager.userName;
        coins.text = UserDetailsManager.userCoins.ToString();
        gamePlayText.text = UserDetailsManager.userName;
    }
    bool isCallByShake;
    // Update is called once per frame
    void Update()
    {
        Vector3 acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        Vector3 deltaAcceleration = acceleration - lowPassValue;

        if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
        {
            // Perform your "shaking actions" here. If necessary, add suitable
            // guards in the if check above to avoid redundant handling during
            // the same shake (e.g. a minimum refractory period).
            Debug.Log("Shake event detected at time " + Time.time);
            if (RollButton.activeInHierarchy)
            {
                OnRollPress();
            }

        }

    }

    public void SetFlagButton(int index )
    {
        if(index==0)
        {
            isMultiplayer = false;
        }
        else
        {
            isMultiplayer = true;
        }

    }
    private void Start()
    {
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
    }
    public void OnRollPress()
    {
        DiceNumberTextScript.diceNumber = 0;
        isDiceOneStay = false;
        isDiceTwoStay = false;
         dice[0].GetComponent<DiceScript>().IsPress = true;
        dice[1].GetComponent<DiceScriptSecond>().IsPress = true;
        RollButton.SetActive(false);
        isPressOnRoll = true;
        if (isMultiplayer)
        {
            StartCoroutine(EnAblePlanner());
        }
        else
        {
            StartCoroutine(calculateBotScore());
        }



    }

    public void OnPressPractice()
    {
        Time.timeScale = 2.5f;
        DiceMultiplayer.Instance.mainMenu.SetActive(false);

        StartCoroutine(StartCountdown());
    }
    public void OnRollPlayAgain()
    {
        //SceneManager.LoadScene(0);
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        Time.timeScale = 1;

    }
    float accelerometerUpdateInterval = 1.0f / 60.0f;
  
    float lowPassKernelWidthInSeconds = 1.0f;
  
    float shakeDetectionThreshold = 2.0f;

    float lowPassFilterFactor;
    Vector3 lowPassValue;

    IEnumerator calculateBotScore()
    {

        yield return new WaitForSeconds(Random.Range(1f, 3f));
        int random = Random.Range(1, 13);
        cpuText.text = random.ToString();
        cpuCount = random;
        planner.GetComponent<BoxCollider>().enabled = true;

    }

    IEnumerator EnAblePlanner()
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        planner.GetComponent<BoxCollider>().enabled = true;
    }
    public void HomeMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("LoginSplash");
    }

    float currCountdownValue;
    public IEnumerator StartCountdown(float countdownValue = 30)
    {
        timerText.text = "30";
        currCountdownValue = countdownValue;
        while (currCountdownValue > 0)
        {
            yield return new WaitForSeconds(1.0f*2.5f);
            currCountdownValue--;
            timerText.text = currCountdownValue.ToString();
        }
        if(currCountdownValue==0)
        {
            Time.timeScale = 1;
            StartCoroutine(DiceCheckZoneScript.instance.OnPopUp("lose"));
        }
    }

   
}
