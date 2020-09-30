/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;
using UnityEngine.UI;
namespace Ludo
{
    public class GameConfigrationController : MonoBehaviour
    {
        //public GameObject TitleText;
        public GameObject bidText;
        public GameObject MinusButton;
        public GameObject PlusButton;
        public GameObject[] Toggles;
        private int currentBidIndex = 0;

        private MyGameMode[] modes = new MyGameMode[] { MyGameMode.Classic, MyGameMode.Quick, MyGameMode.Master };
        public GameObject privateRoomJoin;
        // Use this for initialization
        void Start()
        {
            Debug.Log("Game Config: "+ this.gameObject.name);
        }



        // Update is called once per frame
        void Update()
        {

        }


        void OnEnable()
        {
            for (int i = 0; i < Toggles.Length; i++)
            {
                int index = i;
                Toggles[i].GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                    {
                        ChangeGameMode(value, modes[index]);
                    }
                );
            }

            currentBidIndex = 0;
            UpdateBid(true);

            Toggles[0].GetComponent<Toggle>().isOn = true;
            GameManager.Instance.mode = MyGameMode.Classic;

            switch (GameManager.Instance.type)
            {
                case MyGameType.TwoPlayer:
                    //TitleText.GetComponent<Text>().text = "Two Players";
                    Debug.Log("Two Players");
                    break;
                case MyGameType.FourPlayer:
                    //TitleText.GetComponent<Text>().text = "Four Players";
                    Debug.Log("Four Players");
                    break;
                case MyGameType.Private:
                    //TitleText.GetComponent<Text>().text = "Private Room";
                    Debug.Log("Private Room");
                    privateRoomJoin.SetActive(true);
                    break;
            }

        }

        void OnDisable()
        {
            for (int i = 0; i < Toggles.Length; i++)
            {
                int index = i;
                Toggles[i].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            }

            privateRoomJoin.SetActive(false);
            currentBidIndex = 0;
            UpdateBid(false);
            Toggles[0].GetComponent<Toggle>().isOn = true;
            Toggles[1].GetComponent<Toggle>().isOn = false;
            Toggles[2].GetComponent<Toggle>().isOn = false;
        }

        public void setCreatedProvateRoom()
        {
            GameManager.Instance.JoinedByID = false;
        }

        public void startGame()
        {
            if (UserDetailsManager.userCoins >= GameManager.Instance.payoutCoins)
            {
                //if (GameManager.Instance.type != MyGameType.Private)
                //{
                    LudoMultiplayer.Instance.JoinRoomAndStartGame();
                //}
                //else
                //{
                //    if (GameManager.Instance.JoinedByID)
                //    {
                //        Debug.Log("Joined by id!");
                        //GameManager.Instance.matchPlayerObject.GetComponent<SetMyData>().MatchPlayer();
                //    }
               //     else
                //    {
                 //       Debug.Log("Joined and created");
                  //      GameManager.Instance.playfabManager.CreatePrivateRoom();
                 //       GameManager.Instance.matchPlayerObject.GetComponent<SetMyData>().MatchPlayer();
                 //   }

              //  }
            }
            else
            {
                GameManager.Instance.dialog.SetActive(true);
            }
        }

        private void ChangeGameMode(bool isActive, MyGameMode mode)
        {
            if (isActive)
            {
                GameManager.Instance.mode = mode;
            }
        }



        public void IncreaseBid()
        {
            if (currentBidIndex < StaticStrings.bidValues.Length - 1)
            {
                currentBidIndex++;
                UpdateBid(true);
            }
        }

        public void DecreaseBid()
        {
            if (currentBidIndex > 0)
            {
                currentBidIndex--;
                UpdateBid(true);
            }
        }

        private void UpdateBid(bool changeBidInGM)
        {
            bidText.GetComponent<Text>().text = StaticStrings.bidValuesStrings[currentBidIndex];

            if (changeBidInGM)
                GameManager.Instance.payoutCoins = StaticStrings.bidValues[currentBidIndex];

            if (currentBidIndex == 0) MinusButton.GetComponent<Button>().interactable = false;
            else MinusButton.GetComponent<Button>().interactable = true;

            if (currentBidIndex == StaticStrings.bidValues.Length - 1) PlusButton.GetComponent<Button>().interactable = false;
            else PlusButton.GetComponent<Button>().interactable = true;
        }

        public void HideThisScreen()
        {
            gameObject.SetActive(false);
        }
    }
}