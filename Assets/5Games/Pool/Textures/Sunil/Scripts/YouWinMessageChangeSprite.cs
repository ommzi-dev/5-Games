using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AssemblyCSharp;

public class YouWinMessageChangeSprite : MonoBehaviour {

    public Sprite other;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void changeSprite() {
        GetComponent<Image>().sprite = other;
    }

    public void loadWinnerScene() {
        if (PoolGameManager.Instance.offlineMode) {
            PoolGameManager.Instance.playfabManager.roomOwner = false;
            PoolGameManager.Instance.roomOwner = false;
            PoolGameManager.Instance.resetAllData();
            SceneManager.LoadScene("Menu");
            Debug.Log("Timeout 7");
            PhotonNetwork.BackgroundTimeout = 0;
            //if (PoolGameManager.Instance.offlineMode && StaticStrings.showAdWhenLeaveGame)
            //    PoolGameManager.Instance.adsScript.ShowAd();

        } else {
            SceneManager.LoadScene("WinnerScene");
        }

    }
}
