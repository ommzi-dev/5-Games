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
    public class GameDiceController : MonoBehaviour
    {

        public Sprite[] diceValueSprites;
        public GameObject arrowObject;
        public GameObject diceValueObject1, diceValueObject2;
        public GameObject diceAnim1, diceAnim2;

        // Use this for initialization
        public bool isMyDice = false;
        public GameObject LudoController;
        public LudoGameController controller;
        public int player = 1;
        private Button button;

        public GameObject notInteractable;
        bool dice1Set, dice2Set;
        private int steps1 = 0, steps2 = 0;
        void Start()
        {
            button = GetComponent<Button>();
            controller = LudoController.GetComponent<LudoGameController>();            
            button.interactable = false;
        }

        private void OnEnable()
        {
           // Debug.Log("Steps: " + LudoGameController.dice1Value + " " + LudoGameController.dice2Value);
            if (LudoGameController.dice1Value != 0 && LudoGameController.dice2Value != 0)
            {
                diceValueObject1.GetComponent<Image>().sprite = diceValueSprites[LudoGameController.dice1Value - 1];
                diceValueObject2.GetComponent<Image>().sprite = diceValueSprites[LudoGameController.dice2Value - 1];
            }
        }

        public void SetDiceValue(int diceNum)
        {
            //Debug.Log("Set dice value called");
            if (diceNum == 1)
            {
                dice1Set = true;
                diceValueObject1.GetComponent<Image>().sprite = diceValueSprites[steps1 - 1];
                diceValueObject1.SetActive(true);
                diceAnim1.SetActive(false);
            }
            else if (diceNum == 2)
            {
                dice2Set = true;
                diceValueObject2.GetComponent<Image>().sprite = diceValueSprites[steps2 - 1];
                diceValueObject2.SetActive(true);
                diceAnim2.SetActive(false);
            }
            if (dice1Set && dice2Set)
            {
                dice1Set = false;
                dice2Set = false;
                LudoGameController.Instance.gUIController.restartTimer();

                if ((steps1 + steps2) == 12)
                {
                    LudoGameController.Instance.nextShotPossible = true;
                    //SixStepsCount++;
                    //if (SixStepsCount == 3)
                    //{
                    //    LudoGameController.Instance.nextShotPossible = false;
                    //    if (LudoGameController.Instance.GameGui != null)
                    //    {
                    //          gUIController.SendFinishTurn();
                    //          Invoke("sendFinishTurnWithDelay", 1.0f);
                    //    }
                    //    return;
                    //}
                }
                else
                {
                    //    SixStepsCount = 0;
                    LudoGameController.Instance.nextShotPossible = false;
                }

                if (isMyDice)
                {
                    LudoGameController.Instance.dice1.text = steps1.ToString();
                    LudoGameController.Instance.dice2.text = steps2.ToString();
                    LudoGameController.Instance.diceSum.text = (steps1 + steps2).ToString();
                }
                if (isMyDice || GameManager.Instance.currentPlayer.isBot)
                    LudoGameController.Instance.CheckForPossibleMove();                
            }
        }

        public IEnumerator EnableShot()
        {
            yield return new WaitForSecondsRealtime(1f);
            if (GameManager.Instance.currentPlayer.isBot)
            {
                Debug.Log("Enable Shot: Is Bot : "+ gameObject.name);
                GameManager.Instance.miniGame.BotTurn(false);
                notInteractable.SetActive(false);
            }
            else
            {
                Debug.Log("Enable Shot: Is Player: "+ gameObject.name);
                if (PlayerPrefs.GetInt(StaticStrings.VibrationsKey, 0) == 0)
                {
                    Debug.Log("Vibrate");
                #if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
                #endif
                }
                else
                {
                    Debug.Log("Vibrations OFF");
                }
                controller.gUIController.myTurnSource.Play();
                notInteractable.SetActive(false);
                button.interactable = true;
                arrowObject.SetActive(true);
            }
        }

        public void DisableShot()
        {
            notInteractable.SetActive(true);
            button.interactable = false;
            arrowObject.SetActive(false);
        }

        public void EnableDiceShadow()
        {
            notInteractable.SetActive(true);
        }

        public void DisableDiceShadow()
        {
            notInteractable.SetActive(false);
        }
        int aa = 0;
        int bb = 0;
        public void RollDice()
        {
            if (isMyDice)
            {

                controller.nextShotPossible = false;
                controller.gUIController.PauseTimers();
                button.interactable = false;
                Debug.Log("Roll Dice");
                arrowObject.SetActive(false);
                // if (aa % 2 == 0) steps = 6;
                // else steps = 2;
                // aa++;
                steps1 = Random.Range(1, 7);
                steps2 = Random.Range(1, 7);
                RollDiceStart(steps1, steps2);
                string data = steps1 + ";" + steps2 + ";" + controller.gUIController.GetCurrentPlayerIndex();
                PhotonNetwork.RaiseEvent((int)EnumGame.DiceRoll, data, true, null);

                Debug.Log("Value: " + steps1);
            }
        }

        public void RollDiceBot(int value)
        {

            LudoGameController.Instance.nextShotPossible = false;
            LudoGameController.Instance.gUIController.PauseTimers();

            Debug.Log("Roll Dice bot: " +gameObject.name);

            // if (bb % 2 == 0) steps = 6;
            // else steps = 2;
            // bb++;

            steps1 = value;
            steps2 = Random.Range(1, 7);
            RollDiceStart(steps1, steps2);

        }

        public void RollDiceStart(int steps1, int steps2)
        {
            Debug.Log("RollDiceStart:" + steps1 + " " + steps2);
            GetComponent<AudioSource>().Play();
            this.steps1 = steps1;
            this.steps2 = steps2;
            LudoGameController.dice1Value = steps1;
            LudoGameController.dice2Value = steps2;
            diceValueObject1.SetActive(false);
            diceAnim1.SetActive(true);
            diceAnim1.GetComponent<Animator>().Play("RollDiceAnimation");
            diceValueObject2.SetActive(false);
            diceAnim2.SetActive(true);
            diceAnim2.GetComponent<Animator>().Play("RollDiceAnimation");
        }
    }
}