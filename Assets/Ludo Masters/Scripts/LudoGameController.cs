/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using System;
using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Ludo
{
    public class LudoGameController : PunBehaviour, IMiniGame
    {
        public static LudoGameController Instance;

        public GameObject[] dice;
        public GameObject GameGui;
        public GameGUIController gUIController;
        public GameObject[] Pawns1;
        public GameObject[] Pawns2;
        public GameObject[] Pawns3;
        public GameObject[] Pawns4;

        public GameObject gameBoard;
        public GameObject gameBoardScaler;
        public GameObject exitPopup, menuDialog;
        public GameObject arrowForScore;
        bool playerState = true;
        bool opponentState = true;
        bool checkPoolStatusAfterDisconnect = false;

        [HideInInspector]
        public int steps1 = 5, steps2 = 5;
        public static int dice1Value, dice2Value, currentSelectedMove;
        public static bool hasMovedStep1, hasMovedStep2;
        public bool nextShotPossible;
        private int SixStepsCount = 0;
        public int finishedPawns = 0;
        private int botCounter = 0;
        private List<GameObject> botPawns;

        public Button dice1Move, dice2Move, diceSumMove;
        public Text dice1, dice2, diceSum;

        public void highlightSelectedMove(int diceNum)
        {
            currentSelectedMove = diceNum;
            Debug.Log("Current Selected Move: " + currentSelectedMove);
            switch (diceNum)
            {
                case 1:
                    HighlightPawnsToMove(1, dice1Value);
                    break;
                case 2:
                    HighlightPawnsToMove(1, dice2Value);
                    break;
                case 3:
                    HighlightPawnsToMove(1, dice1Value + dice2Value);
                    break;
            }
        }

        public void CheckForPossibleMove()
        {
            //botPawns = new List<GameObject>();
            gUIController.restartTimer();
            GameObject[] pawns = GameManager.Instance.currentPlayer.pawns;
            bool movePossible = false;
            int firstMove = 0;
            //int possiblePawns = 0;
            //GameObject lastPawn = null;
            Debug.Log(dice1Value + " " + dice2Value);
            currentSelectedMove = 1;
            for (int i = 0; i < pawns.Length; i++)
            {
                bool possible = pawns[i].GetComponent<LudoPawnController>().CheckIfCanMove(dice1Value, false);
                if (possible)
                {
                    firstMove = 1;
                    //lastPawn = pawns[i];
                    movePossible = true;
                    //possiblePawns++;
                    //botPawns.Add(pawns[i]);
                }
            }
            if (!movePossible || (dice2Value == 6 && dice1Value != 6))
            {
                currentSelectedMove = 2;
                for (int i = 0; i < pawns.Length; i++)
                {
                    bool possible = pawns[i].GetComponent<LudoPawnController>().CheckIfCanMove(dice2Value, false);
                    if (possible)
                    {
                        firstMove = 2;
                        //lastPawn = pawns[i];
                        movePossible = true;
                        //possiblePawns++;
                        //botPawns.Add(pawns[i]);
                    }
                }
            }
            Debug.Log("Move Possible: " + movePossible);
            if (!movePossible)
            {
                if (GameGui != null)
                {
                    Debug.Log("game controller call finish turn");
                    gUIController.PauseTimers();
                    Invoke("sendFinishTurnWithDelay", 1.0f);
                }
            }
            else
            {
                if (GameManager.Instance.currentPlayer.isBot)
                {
                    if (firstMove == 1)
                    {
                        currentSelectedMove = 1;
                        HighlightPawnsToMove(1, dice1Value);
                    }
                    else
                    {
                        currentSelectedMove = 2;
                        HighlightPawnsToMove(1, dice2Value);
                    }
                }
                else
                {
                    Unhighlight();
                    dice1Move.interactable = true;
                    dice2Move.interactable = true;
                    diceSumMove.interactable = true;
                    arrowForScore.SetActive(true);
                }
            }
        }

        public void HighlightPawnsToMove(int player, int steps)
        {
            Debug.Log("HighlightPawnsToMove" + steps);
            botPawns = new List<GameObject>();

            gUIController.restartTimer();

            GameObject[] pawns = GameManager.Instance.currentPlayer.pawns;

            this.steps1 = steps;
            /*if (steps == 12)      // Checked in Game Dice Controller Script
            {
                nextShotPossible = true;
                SixStepsCount++;
                if (SixStepsCount == 3)
                {
                    nextShotPossible = false;
                    if (GameGui != null)
                    {
                        //gUIController.SendFinishTurn();
                        Invoke("sendFinishTurnWithDelay", 1.0f);
                    }
                    return;
                }
            }
            else
            {
                SixStepsCount = 0;
                nextShotPossible = false;
            }*/

            bool movePossible = false;

            int possiblePawns = 0;
            GameObject lastPawn = null;
            for (int i = 0; i < pawns.Length; i++)
            {
                bool possible = pawns[i].GetComponent<LudoPawnController>().CheckIfCanMove(steps, true);
                if (possible)
                {
                    lastPawn = pawns[i];
                    movePossible = true;
                    possiblePawns++;
                    botPawns.Add(pawns[i]);
                }
            }

            Debug.Log("movePossible" + movePossible);

            if (possiblePawns == 1)
            {
                //Debug.Log("Possible Pawns 1");
                if (GameManager.Instance.currentPlayer.isBot)
                {
                    StartCoroutine(movePawn(lastPawn, false));
                }
                else
                {
                    lastPawn.GetComponent<LudoPawnController>().MakeMove();
                    //StartCoroutine(MovePawnWithDelay(lastPawn));
                }

            }
            else
            {
                //Debug.Log("Possible Pawns 2");
                if (possiblePawns == 2 && lastPawn.GetComponent<LudoPawnController>().pawnInJoint != null)
                {
                    if (GameManager.Instance.currentPlayer.isBot)
                    {
                        if (!lastPawn.GetComponent<LudoPawnController>().mainInJoint)
                        {
                            StartCoroutine(movePawn(lastPawn, false));
                            Debug.Log("AAA");
                        }
                        else
                        {
                            StartCoroutine(movePawn(lastPawn.GetComponent<LudoPawnController>().pawnInJoint, false));
                            Debug.Log("BBB");
                        }

                    }
                    else
                    {
                        if (!lastPawn.GetComponent<LudoPawnController>().mainInJoint)
                        {
                            lastPawn.GetComponent<LudoPawnController>().MakeMove();
                        }
                        else
                        {
                            lastPawn.GetComponent<LudoPawnController>().pawnInJoint.GetComponent<LudoPawnController>().MakeMove();
                        }
                        //lastPawn.GetComponent<LudoPawnController>().MakeMove();
                    }
                }
                else
                {
                    Debug.Log("Possible Pawns: " + possiblePawns);
                    if (possiblePawns > 0 && GameManager.Instance.currentPlayer.isBot)
                    {
                        int bestScoreIndex = 0;
                        int bestScore = int.MinValue;
                        // Make bot move
                        for (int i = 0; i < botPawns.Count; i++)
                        {
                            int score = botPawns[i].GetComponent<LudoPawnController>().GetMoveScore(steps);
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestScoreIndex = i;
                            }
                        }
                        StartCoroutine(movePawn(botPawns[bestScoreIndex], true));
                    }
                }
            }
            //Debug.Log("hdfshkhlfd");
            /* if(GameManager.Instance.currentPlayer.isBot)
             {
                 Debug.Log("isBot");
                 if (hasMovedStep1)
                 {
                     currentSelectedMove = 2;
                     Debug.Log(" calling2 HighlightPawnsToMove" + dice2Value);
                     HighlightPawnsToMove(1, dice2Value);
                     return;
                 }
                 if (hasMovedStep2)
                 {
                     currentSelectedMove = 1;
                     Debug.Log(" calling1 HighlightPawnsToMove" + dice1Value);
                     HighlightPawnsToMove(1, dice1Value);
                     return;
                 }
             }*/

            if (!movePossible)
            {
                Debug.Log("Move Not possible!");
                if (GameGui != null && GameManager.Instance.currentPlayer.isBot)
                {
                    Debug.Log("game controller call finish turn");
                    gUIController.PauseTimers();
                    Invoke("sendFinishTurnWithDelay", 1.0f);
                }
                else
                {
                    if (hasMovedStep1 || hasMovedStep2)
                    {
                        dice1Move.interactable = false;
                        dice2Move.interactable = false;
                        diceSumMove.interactable = false;
                        arrowForScore.SetActive(false);
                        gUIController.PauseTimers();
                        Invoke("sendFinishTurnWithDelay", 1.0f);
                    }
                    else
                    {
                        GameGUIController.Instance.ShowMessageDialog("Message", "Try Other Move First!!");
                        Debug.Log("Try other moves first!");
                    }
                }
            }
        }

        private IEnumerator MovePawnWithDelay(GameObject lastPawn)
        {
            //yield return new WaitForSeconds(1.0f);

            lastPawn.GetComponent<LudoPawnController>().MakeMove();
            yield return null;
        }

        public void sendFinishTurnWithDelay()
        {
            gUIController.SendFinishTurn();
        }

        public void Unhighlight()
        {
            for (int i = 0; i < Pawns1.Length; i++)
            {
                Pawns1[i].GetComponent<LudoPawnController>().Highlight(false);
            }

            for (int i = 0; i < Pawns2.Length; i++)
            {
                Pawns2[i].GetComponent<LudoPawnController>().Highlight(false);
            }

            for (int i = 0; i < Pawns3.Length; i++)
            {
                Pawns3[i].GetComponent<LudoPawnController>().Highlight(false);
            }

            for (int i = 0; i < Pawns4.Length; i++)
            {
                Pawns4[i].GetComponent<LudoPawnController>().Highlight(false);
            }

        }

        void IMiniGame.BotTurn(bool first)
        {
            Debug.Log("Bot Turn: " + first);
            if (first)
            {
                SixStepsCount = 0;
            }
            Debug.Log("GameManager.Instance.botDelays.Count: " + GameManager.Instance.botDelays.Count);
            //Invoke("RollDiceWithDelay", GameManager.Instance.botDelays[(botCounter + 1) % GameManager.Instance.botDelays.Count]);
            Invoke("RollDiceWithDelay", 0.1f);
            botCounter++;
            //throw new System.NotImplementedException();
        }

        public IEnumerator movePawn(GameObject pawn, bool delay)
        {
            //Debug.Log("Move Pawn: " + delay);
            //if (delay)
            //{
            //    yield return new WaitForSeconds(GameManager.Instance.botDelays[(botCounter + 1) % GameManager.Instance.botDelays.Count]);
            //    botCounter++;
            //}
            pawn.GetComponent<LudoPawnController>().MakeMovePC();
            yield return null;
        }

        public void RollDiceWithDelay()
        {
            GameManager.Instance.currentPlayer.dice.GetComponent<GameDiceController>().RollDiceBot(GameManager.Instance.botDiceValues[(botCounter + 1) % GameManager.Instance.botDelays.Count]);
        }

        void IMiniGame.CheckShot()
        {
            throw new System.NotImplementedException();
        }

        void IMiniGame.setMyTurn()
        {
            SixStepsCount = 0;
            GameManager.Instance.diceShot = false;
            dice[GameManager.Instance.myPlayerIndex].SetActive(true);
            StartCoroutine(dice[GameManager.Instance.myPlayerIndex].GetComponent<GameDiceController>().EnableShot());
            //for (int i=1; i<dice.Length; i++)
            //{
            //    dice[i].SetActive(true);
            //}            
        }

        void IMiniGame.setOpponentTurn()
        {
            SixStepsCount = 0;
            GameManager.Instance.diceShot = false;

            Unhighlight();
        }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            GameManager.Instance.miniGame = this;
            PhotonNetwork.OnEventCall += this.OnEvent;
        }

        // Use this for initialization
        void Start()
        {
            if (Instance == null)
                Instance = this;
            // Scale gameboard


            // float scalerWidth = gameBoardScaler.GetComponent<RectTransform>().rect.size.x;
            // float boardWidth = gameBoard.GetComponent<RectTransform>().rect.size.x;

            // gameBoard.GetComponent<RectTransform>().localScale = new Vector2(scalerWidth / boardWidth, scalerWidth / boardWidth);

            gUIController = GameGui.GetComponent<GameGUIController>();

            dice1Move.interactable = false;
            dice2Move.interactable = false;
            diceSumMove.interactable = false;
            arrowForScore.SetActive(false);
        }
        
        void OnDestroy()
        {
            PhotonNetwork.OnEventCall -= this.OnEvent;
        }

        private void OnEvent(byte eventcode, object content, int senderid)
        {
            Debug.Log("Received event Ludo: " + eventcode);            

            if (eventcode == (int)EnumGame.DiceRoll)
            {

                gUIController.PauseTimers();
                string[] data = ((string)content).Split(';');
                dice1Value = int.Parse(data[0]);
                dice2Value = int.Parse(data[1]);
                int pl = int.Parse(data[2]);

                GameManager.Instance.playerObjects[pl].dice.GetComponent<GameDiceController>().RollDiceStart(dice1Value, dice2Value);
            }
            else if (eventcode == (int)EnumGame.PawnMove)
            {
                string[] data = ((string)content).Split(';');
                Debug.Log("Pawn:" + data[0]);
                //int index = int.Parse(data[0]);
                string pawnName = data[0];

                int pl = int.Parse(data[1]);
                dice1Value = int.Parse(data[2]);
                dice2Value = int.Parse(data[3]);
                currentSelectedMove = int.Parse(data[4]);
                if (currentSelectedMove == 1)
                    steps1 = dice1Value;        // steps1 var is used as value for move
                else if (currentSelectedMove == 2)
                    steps1 = dice2Value;
                else if (currentSelectedMove == 3)
                    steps1 = dice1Value + dice2Value;

                int pIndex = -1;
                for (int i = 0; i < GameManager.Instance.playerObjects[pl].pawns.Length; i++)
                {
                    if (GameManager.Instance.playerObjects[pl].pawns[i].name == pawnName)
                    {
                        pIndex = i;
                        break;
                    }
                }
                GameManager.Instance.playerObjects[pl].pawns[pIndex].GetComponent<LudoPawnController>().MakeMovePC();
            }
            else if (eventcode == (int)EnumGame.PawnRemove)
            {
                string data = (string)content;
                string[] messages = data.Split(';');
                int index = int.Parse(messages[1]);
                int playerIndex = int.Parse(messages[0]);

                GameManager.Instance.playerObjects[playerIndex].pawns[index].GetComponent<LudoPawnController>().GoToInitPosition(false);
            }
            else if (eventcode == 151)
            {
                if (LudoMultiplayer.Instance.gameStarted)
                {
                    StartDisconnectTimer();
                    Time.timeScale = 0f;
                    opponentState = false;
                    Debug.Log("Opponent Left!");
                }
            }
            else if (eventcode == 152)
            {
                if (playerState)
                    Time.timeScale = 1f;
                StopDisconnectTimer();
                opponentState = true;
                Debug.Log("Opponent Joined Back!");
            }
            else if (eventcode == 153)
            {
                Debug.Log("Opponent Quit");
                Debug.Log("Player is Winner.. Do Transaction!");
                string data = (string)content;
                string[] messages = data.Split(';');
                Time.timeScale = 1f;
                string playerId = messages[0];
                int playerIndex = GameGUIController.Instance.GetPlayerPosition(playerId);
                GameGUIController.Instance.setPlayerDisconnected(playerIndex);
                GameGUIController.Instance.FinishedGame(UserDetailsManager.userId);
                StopDisconnectTimer();
            }

        }


        #region PlayerDisconnected
        void Update()
        {
            if (LudoMultiplayer.Instance.isOnlineMode)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Connecting.SetActive(true);
                    checkPoolStatusAfterDisconnect = true;
                    Time.timeScale = 0;
                    Debug.Log("Error. Check internet connection!");
                }
                else
                {
                    if (!opponentDisconnectPopup.activeSelf && playerState && opponentState)
                        Time.timeScale = 1;
                    if (playerState)
                        Connecting.SetActive(false);
                    if (checkPoolStatusAfterDisconnect)
                    {
                        checkPoolStatusOnInternetBreakDown();
                    }
                }
            }
            if (!LudoMultiplayer.Instance.gameOver)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (menuDialog.activeSelf)
                        return;
                    if (!exitPopup.gameObject.activeSelf)
                    {
                        exitPopup.SetActive(true);
                    }                    
                }
            }
        }
        void checkPoolStatusOnInternetBreakDown()
        {
            checkPoolStatusAfterDisconnect = false;
            StartCoroutine(checkPoolStatus());
        }

        private void OnApplicationQuit()
        {
            if (LudoMultiplayer.Instance.gameStarted)
            {
                if (PhotonNetwork.room != null)
                {
                    LudoMultiplayer.Instance.checkForceQuit();
                    PhotonNetwork.RaiseEvent(153, UserDetailsManager.userId, true, null);
                    PhotonNetwork.SendOutgoingCommands();
                }
            }
        }

        IEnumerator checkPoolStatus()
        {
            Debug.Log("checkpoolstatus" + LudoMultiplayer.Instance.poolId);
            WWWForm form = new WWWForm();

            form.AddField("poolid", LudoMultiplayer.Instance.poolId);
            UnityWebRequest www = UnityWebRequest.Post("http://18.191.157.16:4000/apis/getpoolresult", form);
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);

            www.timeout = 15;
            yield return www.SendWebRequest();

            if (www.error != null || www.isNetworkError)
            {
                Debug.Log("error in pool status: " + www.error);
            }
            else
            {
                Debug.Log("checkpoolstatus" + www.downloadHandler.text);
                var statsList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;

                var result = (IDictionary)statsList["result"];

                if (www.downloadHandler.text.Contains("Room does not exists!"))
                {
                    yield return null;
                }
                else if (Convert.ToBoolean(result["success"].ToString()))
                {
                    if (result["userStatus"].ToString() == "Winner")
                    {
                        GameGUIController.Instance.FinishedGame(UserDetailsManager.userId);
                        Debug.Log("Player is Winner!!");
                    }
                    else if (result["userStatus"].ToString() == "Looser")
                    {
                        //WhotManager.instance.AnnounceWinner(false);
                        Debug.Log("Player is Looser!!");
                    }
                    else
                    {
                        PhotonNetwork.ReconnectAndRejoin();
                        StartCoroutine(sendFeedback());
                    }
                }
            }
        }

        IEnumerator sendFeedback()
        {
            //yield return new WaitForSecondsRealtime(1f);

            if (PhotonNetwork.room != null)
            {

                PhotonNetwork.RaiseEvent(152, 1, true, null);
                PhotonNetwork.SendOutgoingCommands();
                Connecting.SetActive(false);
            }
            else
            {
                StartCoroutine(sendFeedback());
            }
            yield return null;
        }
        #endregion

        #region DisconnectTimer
        public int disconnectTimer;
        public Text opponentDisconnectTimer;
        public GameObject opponentDisconnectPopup;
        public GameObject Connecting;

        private void OnApplicationFocus(bool focus)
        {
            playerState = focus;
            if (GameManager.Instance.offlineMode)
                return;
            if (focus)
            {

                Debug.Log("Application resume: " +opponentState);
                if (opponentState)
                    Time.timeScale = 1f;
                Connecting.SetActive(true);
                if (PhotonNetwork.room != null)
                {
                    Debug.Log("Sending Event 152");
                    Connecting.SetActive(false);
                    PhotonNetwork.RaiseEvent(152, 1, true, null);
                    PhotonNetwork.SendOutgoingCommands();
                }
                StartCoroutine(checkPoolStatus());
            }
            else
            {
                // playerState = false;
                Debug.Log("Application pause");
                Connecting.SetActive(true);
                PhotonNetwork.RaiseEvent(151, 1, true, null);
                PhotonNetwork.SendOutgoingCommands();
                Time.timeScale = 0f;                
            }
        }

        public void StartDisconnectTimer()
        {
            Debug.Log("Starting Disconnect Timer!");
            disconnectTimer = 180;
            opponentDisconnectTimer.text = disconnectTimer.ToString();
            opponentDisconnectPopup.SetActive(true);
            StartCoroutine(UpdateDisconnectTimer());
            Time.timeScale = 0f;
        }

        IEnumerator UpdateDisconnectTimer()
        {
            yield return new WaitForSecondsRealtime(1f);
            disconnectTimer -= 1;
            Debug.Log("Disconnect Timer: " + disconnectTimer);
            opponentDisconnectTimer.text = disconnectTimer.ToString();
            if (disconnectTimer > 0  && !GameGUIController.Instance.iFinished)
            {
                if (Time.timeScale == 0)
                    StartCoroutine(UpdateDisconnectTimer());
            }
            else
            {
                StopDisconnectTimer();
            }
        }

        public void StopDisconnectTimer()
        {
            //Time.timeScale = 1f;
            Debug.Log("Stopping Disconnect Timer!");
            opponentDisconnectPopup.SetActive(false);
            StopCoroutine(UpdateDisconnectTimer());
        }
        #endregion
    }
}