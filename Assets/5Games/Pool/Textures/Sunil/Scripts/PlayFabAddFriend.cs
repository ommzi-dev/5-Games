using UnityEngine;
using System.Collections;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine.SceneManagement;
using AssemblyCSharp;

public class PlayFabAddFriend : MonoBehaviour {

    public GameObject menuObject;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {


    }

    public void AddFriend() {
        menuObject.GetComponent<Animator>().Play("hideMenuAnimation");
        if (!PoolGameManager.Instance.offlineMode) {
            PhotonNetwork.RaiseEvent(192, 1, true, null);



            AddFriendRequest request = new AddFriendRequest() {
                FriendPlayFabId = PhotonNetwork.otherPlayers[0].name
            };



            PlayFabClientAPI.AddFriend(request, (result) => {
                Debug.Log("Added friend successfully");
                PoolGameManager.Instance.friendButtonMenu.SetActive(false);
                PoolGameManager.Instance.smallMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(PoolGameManager.Instance.smallMenu.GetComponent<RectTransform>().sizeDelta.x, 260.0f);
                //GameManager.Instance.playfabManager.chatClient.AddFriends(new string[] {PhotonNetwork.otherPlayers[0].name});
            }, (error) => {
                Debug.Log("Error adding friend: " + error.Error);
            }, null);
        }

    }

    public void showMenu() {
        menuObject.GetComponent<Animator>().Play("ShowMenuAnimation");
    }

    public void hideMenu() {
        menuObject.GetComponent<Animator>().Play("hideMenuAnimation");
    }

    public void LeaveGame() {
        if (PoolStaticStrings.showAdWhenLeaveGame)
            //PoolGameManager.Instance.adsScript.ShowAd();
        SceneManager.LoadScene("Menu");
        PhotonNetwork.BackgroundTimeout = 0;
        Debug.Log("Timeout 3");
        PoolGameManager.Instance.cueController.removeOnEventCall();
        PhotonNetwork.LeaveRoom();

        PoolGameManager.Instance.playfabManager.roomOwner = false;
        PoolGameManager.Instance.roomOwner = false;
        PoolGameManager.Instance.resetAllData();

    }
}
