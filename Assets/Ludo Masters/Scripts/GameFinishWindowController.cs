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
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace Ludo
{
    public class GameFinishWindowController : MonoBehaviour
    {

        public GameObject Window;
        public GameObject[] AvatarsMain;
        public GameObject[] AvatarsImage;
        public GameObject[] Names;
        public GameObject[] Backgrounds;
        public GameObject[] PrizeMainObjects;
        public GameObject[] prizeText;
        public GameObject[] placeIndicators;

        public Texture lose, win;
        public GameObject winloseimg;
        public Text ResText1, ResText2;
        // Use this for initialization
        void Start()
        {
            for (int i = 0; i < AvatarsMain.Length; i++)
            {
                AvatarsMain[i].SetActive(false);
            }

        }

        //public void showWindow(List<PlayerObject> playersFinished, List<PlayerObject> otherPlayers)
        public void showWinLoseWindow()
        {
            //if (secondPlacePrize == 0)
            //{
            //    PrizeMainObjects[1].SetActive(false);
            //}

            //prizeText[0].GetComponent<Text>().text = firstPlacePrize.ToString();
            //prizeText[1].GetComponent<Text>().text = secondPlacePrize.ToString();
            LudoGameController.Instance.opponentDisconnectPopup.SetActive(false);
            //Window.transform.localScale = Vector3.one;
            
            Debug.Log("Show Result Window called");
            if (GameGUIController.Instance.winnerId == UserDetailsManager.userId)
            {
                winloseimg.GetComponent<RawImage>().texture = win;
                Debug.Log("Player Won!!");
                if (!GameManager.Instance.offlineMode)
                {
                    //StopAllCoroutines();
                    ResText1.text = "COINS WON: ";
                    ResText2.text = (LudoMultiplayer.Instance.winAmt).ToString();
                    ResText1.gameObject.SetActive(true);
                    ResText2.gameObject.SetActive(true);
                    PhotonNetwork.RaiseEvent((int)EnumPhoton.FinishedGame, UserDetailsManager.userId, true, null);
                }
                else
                {
                    ResText1.gameObject.SetActive(false);
                    ResText2.gameObject.SetActive(false);
                }
                SoundManager.Instance.PlaySound(3);
            }
            else
            {
                Debug.Log("Player Lost!!");
                winloseimg.GetComponent<RawImage>().texture = lose;
                if (!GameManager.Instance.offlineMode)
                {
                    // StopAllCoroutines();
                    ResText1.text = "COINS LOST: ";
                    ResText2.text = LudoMultiplayer.Instance.betAmount.ToString();
                    ResText1.gameObject.SetActive(true);
                    ResText2.gameObject.SetActive(true);
                }
                else
                {
                    ResText1.gameObject.SetActive(false);
                    ResText2.gameObject.SetActive(false);
                }
                SoundManager.Instance.PlaySound(4);
            }
            Window.SetActive(true);


            /* for (int i = 0; i < playersFinished.Count; i++)
             {
                 AvatarsMain[i].SetActive(true);
                 AvatarsImage[i].GetComponent<RawImage>().texture = playersFinished[i].avatar;
                 Names[i].GetComponent<Text>().text = playersFinished[i].name;
                 if (playersFinished[i].id.Equals(UserDetailsManager.userId))
                 {
                     Backgrounds[i].SetActive(true);
                 }
             }

             int counter = 0;
             for (int i = playersFinished.Count; i < playersFinished.Count + otherPlayers.Count; i++)
             {
                 //if (i == 1)
                 //{
                 //    PrizeMainObjects[1].SetActive(false);
                 //}
                 AvatarsMain[i].SetActive(true);
                 AvatarsImage[i].GetComponent<RawImage>().texture = otherPlayers[counter].avatar;
                 Names[i].GetComponent<Text>().text = otherPlayers[counter].name;
                 if (otherPlayers[counter].id.Equals(UserDetailsManager.userId))
                 {
                     Backgrounds[i].SetActive(true);
                 }
                 if (otherPlayers.Count > 1)
                     placeIndicators[i].SetActive(false);
                 counter++;
             }*/

            if(LudoMultiplayer.Instance.isOnlineMode)
                StartCoroutine(TransactionPool(GameGUIController.Instance.winnerId));
        }

        public IEnumerator TransactionPool(string winner)
        {
            Debug.Log("TransactionPool: " + LudoMultiplayer.Instance.poolId);
            WWWForm form = new WWWForm();
            form.AddField("winner_id", winner);
            form.AddField("game_id", "1"); /// 1 represents Ludo game
            form.AddField("poolid", LudoMultiplayer.Instance.poolId);
            UnityWebRequest www = UnityWebRequest.Post(UserDetailsManager.serverUrl + "transaction001", form);
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

            www.timeout = 15;
            yield return www.SendWebRequest();
            LudoMultiplayer.Instance.poolId = "";
            //StartCoroutine(GetUserStats());
            if (www.error != null || www.isNetworkError)
            {
                Debug.Log("Transation Completed with error: " + www.error);
            }
            else
            {
                Debug.Log("Transation Completed Successfully" + www.downloadHandler.text);
            }
        }
    }
}