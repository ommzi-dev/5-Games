using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SelectedTabluButtonClicked : MonoBehaviour
{

    public int tableNumber;
    public int fee;

    // Use this for initialization

    void Start()
    {
        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        gameObject.GetComponent<Button>().onClick.AddListener(startGame);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void startGame()
    {


        Debug.Log("Fee: " + fee + "  Coins: " + PoolGameManager.Instance.coinsCount);
        if (PoolGameManager.Instance.coinsCount >= fee)
        {

            if (PoolGameManager.Instance.inviteFriendActivated)
            {
                PoolGameManager.Instance.tableNumber = tableNumber;
                PoolGameManager.Instance.payoutCoins = fee;
                PoolGameManager.Instance.initMenuScript.backToMenuFromTableSelect();
                PoolGameManager.Instance.playfabManager.challengeFriend(PoolGameManager.Instance.challengedFriendID, "" + fee + ";" + tableNumber);

            }
            else
            {
                PoolGameManager.Instance.tableNumber = tableNumber;
                PoolGameManager.Instance.payoutCoins = fee;
                PoolGameManager.Instance.facebookManager.startRandomGame();
            }

        }
        else
        {
            PoolGameManager.Instance.dialog.SetActive(true);
        }

    }


}
