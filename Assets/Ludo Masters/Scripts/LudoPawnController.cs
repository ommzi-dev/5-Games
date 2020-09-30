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
using UnityEngine;
using UnityEngine.UI;
namespace Ludo
{
    public class LudoPawnController : MonoBehaviour
    {
        public AudioSource killedPawnSound;
        public AudioSource inHomeSound;
        //public GameObject pawnTop;
        //public GameObject pawnTopMultiple;

        public GameObject dice;
        private GameDiceController diceController;
        public GameObject pawnInJoint = null;
        public bool mainInJoint = false;
        public GameObject highlight;
        public bool isOnBoard = false;

        private LudoGameController ludoController;

        public RectTransform[] path;
        public RectTransform targetObject;
        private int currentPosition = -1;

        private float singlePathSpeed = 0.13f;
        private float MoveToStartPositionSpeed = 0.25f;
        private RectTransform rect;
        private Vector3 initScale;

        public bool isMinePawn = false;

        public int index;
        public bool myTurn;

        [HideInInspector]
        private int playerIndex;
        public AudioSource[] sound;
        public Vector2 initPosition;
        private bool canMakeJoint = false;
        private int currentAudioSource = 0;

        void Start()
        {
            //Debug.Log("Game mode: " + GameManager.Instance.mode.ToString());
            diceController = dice.GetComponent<GameDiceController>();
            ludoController = GameObject.Find("GameSpecific").GetComponent<LudoGameController>();
            rect = GetComponent<RectTransform>();
            initScale = rect.localScale;
            initPosition = rect.anchoredPosition;
            GetComponent<Button>().interactable = false;

            if (GameManager.Instance.mode == MyGameMode.Master)
            {
                canMakeJoint = true;
            }
        }

        public void setPlayerIndex(int index)
        {
            this.playerIndex = index;
            //Debug.Log("player index: "+ playerIndex);
        }

        public void Highlight(bool active)
        {
            if (GameManager.Instance.currentPlayer.isBot)
            {
                GetComponent<Button>().interactable = false;
                highlight.SetActive(false);
            }
            else
            {
                if (active)
                {
                    GetComponent<Button>().interactable = true;
                    highlight.SetActive(true);
                }
                else
                {
                    GetComponent<Button>().interactable = false;
                    highlight.SetActive(false);
                }
            }
        }

        public int GetMoveScore(int steps)
        {
            if (steps == 6 && !isOnBoard)
            {
                return 300;
            }
            else
            {
                if (isOnBoard)
                {
                    if (GameManager.Instance.mode == MyGameMode.Quick && GameManager.Instance.currentPlayer.canEnterHome)
                    {
                        return 500;
                    }

                    if (pawnInJoint != null)
                    {
                        steps = steps / 2;
                    }

                    // finish
                    if (currentPosition + steps == path.Length - 1)
                    {
                        return 1000;
                    }

                    // safe place
                    if (!path[currentPosition].GetComponent<LudoPathObjectController>().isProtectedPlace && path[currentPosition + steps].GetComponent<LudoPathObjectController>().isProtectedPlace)
                    {
                        return 400;
                    }

                    // joint
                    LudoPathObjectController pathControl = path[currentPosition + steps].GetComponent<LudoPathObjectController>();
                    if (pathControl.pawns.Count > 0)
                    {
                        for (int i = 0; i < pathControl.pawns.Count; i++)
                        {
                            if (pathControl.pawns[i].GetComponent<LudoPawnController>().playerIndex == playerIndex)
                            {
                                return 700;
                            }
                        }
                    }

                    if (pathControl.pawns.Count > 0)
                    {
                        for (int i = 0; i < pathControl.pawns.Count; i++)
                        {
                            if (pathControl.pawns[i].GetComponent<LudoPawnController>().playerIndex != playerIndex)
                            {
                                return 500;
                            }
                        }
                    }

                    if (path[currentPosition].GetComponent<LudoPathObjectController>().isProtectedPlace)
                    {
                        return -100;
                    }
                }
            }
            return 0;
        }

        public bool CheckIfCanMove(int steps, bool shouldHighlight)
        {
            //Debug.Log(steps + " " + isOnBoard +  " " + LudoGameController.currentSelectedMove);
            if (steps == 6 && !isOnBoard && LudoGameController.currentSelectedMove != 3)
            {
                if(shouldHighlight)
                    Highlight(true);
                return true;
            }
            else
            {
                if (isOnBoard)
                {
                    if (pawnInJoint != null)
                    {
                        if (steps % 2 != 0)
                            return false;
                        else
                        {
                            steps = steps / 2;
                        }
                    }

                    if (currentPosition + steps < path.Length)
                    {
                        LudoPathObjectController pathControl = path[currentPosition + steps].GetComponent<LudoPathObjectController>();

                        //Debug.Log("pawns count on destination: " + pathControl.pawns.Count);
                        if (pathControl.pawns.Count == 2 && pathControl.pawns[0].GetComponent<LudoPawnController>().pawnInJoint != null)
                        {
                            Debug.Log("im inside");
                            if (pawnInJoint != null)
                            {
                                Debug.Log("return true");
                                if (pathControl.pawns[0].GetComponent<LudoPawnController>().playerIndex != playerIndex)
                                {
                                    if(shouldHighlight)
                                        Highlight(true);
                                    return true;
                                }
                                else return false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    for (int i = 1; i < steps + 1; i++)
                    {
                        if (currentPosition + i < path.Length)
                        {
                            //Debug.Log("check count: " + path[currentPosition + i].GetComponent<LudoPathObjectController>().pawns.Count);
                            if (path[currentPosition + i].GetComponent<LudoPathObjectController>().pawns.Count > 1)
                            {
                                Debug.Log("more than 1");
                                if (path[currentPosition + i].GetComponent<LudoPathObjectController>().pawns[0].GetComponent<LudoPawnController>().pawnInJoint != null)
                                {
                                    Debug.Log("blockade");
                                    return false;
                                }
                            }
                        }
                    }

                    if (currentPosition == path.Length - 1 || currentPosition + steps > path.Length - 1)
                    {
                        return false;
                    }

                    if ((currentPosition + steps > path.Length - 1 - 6) &&
                        GameManager.Instance.needToKillOpponentToEnterHome &&
                    !GameManager.Instance.playerObjects[playerIndex].canEnterHome)
                    {
                        return false;
                    }

                    Highlight(true);
                    return true;
                }
            }
            return false;
        }

        public void GoToStartPosition()
        {
            Debug.Log("Go to start Pos called!!");
            LudoGameController.Instance.arrowForScore.SetActive(false);
            LudoGameController.Instance.diceSumMove.interactable = false;
            if (LudoGameController.currentSelectedMove == 1)
            {
                LudoGameController.hasMovedStep1 = true;
                LudoGameController.Instance.dice1Move.interactable = false;                                
            }
            else if (LudoGameController.currentSelectedMove == 2)
            {
                LudoGameController.hasMovedStep2 = true;
                LudoGameController.Instance.dice2Move.interactable = false;                
            }
            else if (LudoGameController.currentSelectedMove == 3)
            {
                LudoGameController.hasMovedStep1 = true;
                LudoGameController.hasMovedStep2 = true;
                LudoGameController.Instance.dice1Move.interactable = false;
                LudoGameController.Instance.dice2Move.interactable = false;                
            }

            rect.SetAsLastSibling();
            currentPosition = 0;
            if(LudoGameController.hasMovedStep1 && LudoGameController.hasMovedStep2)
                StartCoroutine(MoveDelayed(0, initPosition, path[currentPosition].anchoredPosition, MoveToStartPositionSpeed, true, true));
            else
                StartCoroutine(MoveDelayed(0, initPosition, path[currentPosition].anchoredPosition, MoveToStartPositionSpeed, false, true));

            if (pawnInJoint != null)
            {
                pawnInJoint.GetComponent<LudoPawnController>().pawnInJoint = null;
                pawnInJoint.GetComponent<LudoPawnController>().GoToStartPosition();
                pawnInJoint = null;
            }
            if (!LudoGameController.hasMovedStep1 || !LudoGameController.hasMovedStep2)
            {
                //StartCoroutine(CheckSecondMoveForBot());
                StartCoroutine(CheckForOpponentPawn());
            }
        }

        public void GoToInitPosition(bool callEnd)
        {
            Debug.Log("Go to init Pos called!!");
            LudoGameController.Instance.arrowForScore.SetActive(false);
            LudoGameController.Instance.diceSumMove.interactable = false;
            if (LudoGameController.currentSelectedMove == 1)
            {
                LudoGameController.hasMovedStep1 = true;
                LudoGameController.Instance.dice1Move.interactable = false;                
            }
            else if (LudoGameController.currentSelectedMove == 2)
            {
                LudoGameController.hasMovedStep2 = true;
                LudoGameController.Instance.dice2Move.interactable = false;                
            }
            else if (LudoGameController.currentSelectedMove == 3)
            {
                LudoGameController.hasMovedStep1 = true;
                LudoGameController.hasMovedStep2 = true;
                LudoGameController.Instance.dice1Move.interactable = false;
                LudoGameController.Instance.dice2Move.interactable = false;                
            }
            killedPawnSound.Play();
            rect.SetAsLastSibling();
            isOnBoard = false;
            //path[currentPosition].GetComponent<LudoPathObjectController>().RemovePawn(this.gameObject);  
            currentPosition = -1;
            //pawnTop.SetActive(true);
            //pawnTopMultiple.SetActive(false);
            if(LudoGameController.hasMovedStep1 && LudoGameController.hasMovedStep2)
                StartCoroutine(MoveDelayed(0, rect.anchoredPosition, initPosition, MoveToStartPositionSpeed, true, false));
            else
                StartCoroutine(MoveDelayed(0, rect.anchoredPosition, initPosition, MoveToStartPositionSpeed, false, false));

            if (pawnInJoint != null)
            {
                pawnInJoint.GetComponent<LudoPawnController>().pawnInJoint = null;
                pawnInJoint.GetComponent<LudoPawnController>().GoToInitPosition(true);
                pawnInJoint = null;
            }
            if (!LudoGameController.hasMovedStep1 || !LudoGameController.hasMovedStep2)
            {
                //StartCoroutine(CheckSecondMoveForBot());
                StartCoroutine(CheckForOpponentPawn());
            }
        }

        public void MoveBySteps(int steps, bool checkForMoves)
        {
            Debug.Log("Move By Steps: " + steps);
            bool last = false, playSound = false;
            LudoPathObjectController controller = path[currentPosition].GetComponent<LudoPathObjectController>();
            controller.RemovePawn(this.gameObject);
            RepositionPawns(controller.pawns.Count, currentPosition);
            rect.SetAsLastSibling();
            
            for (int i = 0; i < steps; i++)
            {
                last = false;
                currentPosition++;
                if (i == steps - 1)
                {
                    playSound = true;
                    if (checkForMoves)
                    {
                        LudoGameController.Instance.arrowForScore.SetActive(false);
                        LudoGameController.Instance.diceSumMove.interactable = false;
                        if (LudoGameController.currentSelectedMove == 1)
                        {
                            LudoGameController.hasMovedStep1 = true;
                            LudoGameController.Instance.dice1Move.interactable = false;                            
                        }
                        else if (LudoGameController.currentSelectedMove == 2)
                        {
                            LudoGameController.hasMovedStep2 = true;
                            LudoGameController.Instance.dice2Move.interactable = false;                            
                        }
                        else if (LudoGameController.currentSelectedMove == 3)
                        {
                            LudoGameController.hasMovedStep1 = true;
                            LudoGameController.hasMovedStep2 = true;
                            LudoGameController.Instance.dice1Move.interactable = false;
                            LudoGameController.Instance.dice2Move.interactable = false;                            
                        }

                        if (LudoGameController.hasMovedStep1 && LudoGameController.hasMovedStep2)
                            last = true;
                        else
                        {
                            // StartCoroutine(CheckSecondMoveForBot());
                            StartCoroutine(CheckForOpponentPawn());
                        }
                    }
                    //else
                    //{
                    //    StartCoroutine(SetPawnPosition());
                    //}
                }
                StartCoroutine(MoveDelayed(i, path[currentPosition - 1].anchoredPosition, path[currentPosition].anchoredPosition, singlePathSpeed, last, playSound));
            }            
        }

        /*IEnumerator SetPawnPosition()
        {
            Debug.Log("CP: " + currentPosition);
            yield return new WaitForSeconds(2f);
            Debug.Log("CP: " + path[currentPosition].name);
            LudoPathObjectController pathController = path[currentPosition].GetComponent<LudoPathObjectController>();
            pathController.AddPawn(this.gameObject);
        }*/

        IEnumerator CheckForOpponentPawn()
        {
            if (currentPosition >= 0)
            {
                Debug.Log("CP: " + currentPosition);
                yield return new WaitForSeconds(1f);
                Debug.Log("CP Name: " + path[currentPosition].name);
                LudoPathObjectController pathController = path[currentPosition].GetComponent<LudoPathObjectController>();
                //pathController.AddPawn(this.gameObject);
                int otherCount = pathController.pawns.Count;
                Debug.Log("CheckForOpponentPawn: " + otherCount);
                if (otherCount > 1) // Check and remove opponent pawns to home
                {
                    for (int i = otherCount - 2; i >= 0; i--)
                    {
                        if (pathController.pawns[i].GetComponent<LudoPawnController>().playerIndex != playerIndex)
                        {
                            int color = pathController.pawns[i].GetComponent<LudoPawnController>().playerIndex;
                            // Count pawns in this color
                            int pawnsInColor = 0;
                            for (int k = 0; k < otherCount; k++)
                            {
                                if (k < pathController.pawns.Count && pathController.pawns[k].GetComponent<LudoPawnController>().playerIndex == color)
                                {
                                    pawnsInColor++;
                                }
                            }
                            Debug.Log("Pawns In color: " + pawnsInColor);
                            if (pawnsInColor >= 1 /*|| canMakeJoint*/)
                            {
                                // Killed opponent pawn, Additional turn
                                //ludoController.nextShotPossible = true;
                                GameManager.Instance.playerObjects[playerIndex].canEnterHome = true;
                                GameManager.Instance.playerObjects[playerIndex].homeLockObjects.SetActive(false);
                                // Move killed pawn to start position and remove from list
                                pathController.pawns[i].GetComponent<LudoPawnController>().GoToInitPosition(false);

                                pathController.RemovePawn(pathController.pawns[i]);
                                Debug.Log("Move by: " + (path.Length - (currentPosition + 1)).ToString());
                                //MoveBySteps(path.Length - (currentPosition + 1), false);
                                StartCoroutine(MoveDelayed(i, path[currentPosition - 1].anchoredPosition, targetObject.anchoredPosition, singlePathSpeed, false, true));
                                yield return new WaitForSeconds(1f);
                                break;
                            }
                        }
                    }
                }

                if ((myTurn || GameManager.Instance.currentPlayer.isBot) && currentPosition == path.Length - 1)
                {                    
                    GameManager.Instance.currentPlayer.finishedPawns++;
                    Debug.Log("FINISHSSSS: " + GameManager.Instance.currentPlayer.finishedPawns + " total:"+ GameManager.Instance.currentPlayer.pawns.Length);
                    //ludoController.finishedPawns++;
                    if (GameManager.Instance.mode == MyGameMode.Quick)
                    {
                        if (GameManager.Instance.currentPlayer.finishedPawns == 1)
                        {
                            ludoController.gUIController.FinishedGame(GameManager.Instance.currentPlayer.id);
                            yield return null;
                        }
                    }
                    else if (GameManager.Instance.currentPlayer.finishedPawns == GameManager.Instance.currentPlayer.pawns.Length)
                    {
                        {
                            ludoController.gUIController.FinishedGame(GameManager.Instance.currentPlayer.id);
                            yield return null;
                        }
                    }
                    //ludoController.nextShotPossible = true;
                }           
            }
            if(!LudoGameController.hasMovedStep1 || !LudoGameController.hasMovedStep2)
                StartCoroutine(CheckSecondMoveForBot());
            yield return null;
        }

        IEnumerator CheckSecondMoveForBot()
        {
            yield return new WaitForSeconds(0.2f);
            //CheckForOpponentPawn();
            //yield return new WaitForSeconds(1f);
            if (LudoGameController.hasMovedStep1 && !LudoGameController.hasMovedStep2 && GameManager.Instance.currentPlayer.isBot)
            {
                LudoGameController.currentSelectedMove = 2;
                Debug.Log(" calling2 HighlightPawnsToMove" + LudoGameController.dice2Value);
                LudoGameController.Instance.HighlightPawnsToMove(1, LudoGameController.dice2Value);
                //return;
            }
            else if (LudoGameController.hasMovedStep2 && !LudoGameController.hasMovedStep1 && GameManager.Instance.currentPlayer.isBot)
            {
                LudoGameController.currentSelectedMove = 1;
                Debug.Log(" calling1 HighlightPawnsToMove" + LudoGameController.dice1Value);
                LudoGameController.Instance.HighlightPawnsToMove(1, LudoGameController.dice1Value);
                //return;
            }
        }

        public void MakeMove()
        {
            
            string data = gameObject.name + ";" + ludoController.gUIController.GetCurrentPlayerIndex() + ";" + LudoGameController.dice1Value + ";" + LudoGameController.dice2Value + ";" + LudoGameController.currentSelectedMove;
            Debug.Log("Make move button: " +data);

            PhotonNetwork.RaiseEvent((int)EnumGame.PawnMove, data, true, null);

            if (pawnInJoint != null) ludoController.steps1 /= 2;
            GameManager.Instance.diceShot = true;
            myTurn = true;
            ludoController.gUIController.PauseTimers();
            ludoController.Unhighlight();

            /*if (LudoGameController.currentSelectedMove == 1)
            {
                LudoGameController.hasMovedStep1 = true;
                LudoGameController.Instance.dice1Move.interactable = false;
                LudoGameController.Instance.diceSumMove.interactable = false;
            }
            else if (LudoGameController.currentSelectedMove == 2)
            {
                LudoGameController.hasMovedStep2 = true;
                LudoGameController.Instance.dice2Move.interactable = false;
                LudoGameController.Instance.diceSumMove.interactable = false;
            }
            else if (LudoGameController.currentSelectedMove == 3)
            {
                LudoGameController.hasMovedStep1 = true;
                LudoGameController.hasMovedStep2 = true;
                LudoGameController.Instance.dice1Move.interactable = false;
                LudoGameController.Instance.dice2Move.interactable = false;
                LudoGameController.Instance.diceSumMove.interactable = false;
            }
            Debug.Log("Move 1: " + LudoGameController.hasMovedStep1 + " Move 2: "+ LudoGameController.hasMovedStep2);
            */
            if (!isOnBoard)
            {
                GoToStartPosition();
            }
            else
            {
                if (pawnInJoint != null)
                {
                    pawnInJoint.GetComponent<LudoPawnController>().MoveBySteps(ludoController.steps1, true);
                }
                MoveBySteps(ludoController.steps1, true);
            }

            isOnBoard = true;
        }

        public void MakeMovePC()
        {
            Debug.Log("Make PC move");
            if (pawnInJoint != null) ludoController.steps1 /= 2;

            myTurn = false;
            ludoController.gUIController.PauseTimers();
                                  
            if (!isOnBoard)
            {
                GoToStartPosition();
            }
            else
            {
                if (pawnInJoint != null)
                {
                    pawnInJoint.GetComponent<LudoPawnController>().MoveBySteps(ludoController.steps1, true);
                }
                MoveBySteps(ludoController.steps1, true);
            }

            isOnBoard = true;            
        }

        private IEnumerator MoveDelayed(int delay, Vector2 from, Vector2 to, float time, bool last, bool playSound)
        {
            rect.localScale = new Vector3(initScale.x * 1.2f, initScale.y * 1.2f, initScale.z);

            yield return new WaitForSeconds(delay * singlePathSpeed);            

            if (last)
            {
                Debug.Log("Calling MoveFinished()");
                LudoGameController.hasMovedStep1 = false;
                LudoGameController.hasMovedStep2 = false;
                iTween.ValueTo(gameObject, iTween.Hash("from", from, "to", to, "time", time, "easetype", iTween.EaseType.linear, "onupdate", "UpdatePosition", "oncomplete", "MoveFinished"));
            }
            else
            {
                iTween.ValueTo(gameObject, iTween.Hash("from", from, "to", to, "time", time, "easetype", iTween.EaseType.linear, "onupdate", "UpdatePosition"));                
            }

            if (playSound)
            {
                sound[currentAudioSource % sound.Length].Play();
                currentAudioSource++;
                Debug.Log("CP: " + currentPosition);
                //yield return new WaitForSeconds(2f);
                Debug.Log("CP: " + path[currentPosition].name);
                LudoPathObjectController pathController = path[currentPosition].GetComponent<LudoPathObjectController>();
                if (!pathController.pawns.Contains(this.gameObject))
                {
                    Debug.Log("Path Controller: " + pathController.name + " adding Pawn!!");
                    pathController.AddPawn(this.gameObject);
                }
            }
        }

        private void resetScale()
        {
            rect.localScale = initScale;
        }

        private void MoveFinished()
        {
            resetScale();

            if (currentPosition >= 0)
            {
                bool canSendFinishTurn = true;

                LudoPathObjectController pathController = path[currentPosition].GetComponent<LudoPathObjectController>();

                if(!pathController.pawns.Contains(this.gameObject))
                    pathController.AddPawn(this.gameObject);

                if (pawnInJoint == null || (pawnInJoint != null && mainInJoint))
                {
                    Debug.Log("Main in joint");
                    int otherCount = pathController.pawns.Count;
                    Debug.Log("Pawns count: " + otherCount);
                    if (!pathController.isProtectedPlace)
                    {
                        if (otherCount > 1) // Check and remove opponent pawns to home
                        {
                            for (int i = otherCount - 2; i >= 0; i--)
                            {
                                if (pathController.pawns[i].GetComponent<LudoPawnController>().playerIndex != playerIndex)
                                {
                                    int color = pathController.pawns[i].GetComponent<LudoPawnController>().playerIndex;
                                    // Count pawns in this color
                                    int pawnsInColor = 0;
                                    for (int k = 0; k < otherCount; k++)
                                    {
                                        if (k < pathController.pawns.Count && pathController.pawns[k].GetComponent<LudoPawnController>().playerIndex == color)
                                        {
                                            pawnsInColor++;
                                        }
                                    }
                                    Debug.Log("Pawns In color: " + pawnsInColor);
                                    if (pawnsInColor >= 1 /*|| canMakeJoint*/)
                                    {
                                        // Killed opponent pawn, Additional turn
                                        //ludoController.nextShotPossible = true;

                                        GameManager.Instance.playerObjects[playerIndex].canEnterHome = true;
                                        GameManager.Instance.playerObjects[playerIndex].homeLockObjects.SetActive(false);
                                        // Move killed pawn to start position and remove from list
                                        pathController.pawns[i].GetComponent<LudoPawnController>().GoToInitPosition(false);

                                        pathController.RemovePawn(pathController.pawns[i]);
                                        Debug.Log("Move by: " + (path.Length - (currentPosition + 1)).ToString());
                                        //MoveBySteps(path.Length - (currentPosition + 1), false);
                                        StartCoroutine(MoveDelayed(i, path[currentPosition - 1].anchoredPosition, targetObject.anchoredPosition, singlePathSpeed, false, true));
                                        break;
                                    }
                                }
                                else
                                {
                                    if (canMakeJoint && pawnInJoint == null)
                                    {
                                        Debug.Log("Joint");
                                        pawnInJoint = pathController.pawns[i];
                                        mainInJoint = true;
                                        pathController.pawns[i].GetComponent<LudoPawnController>().mainInJoint = false;
                                        pathController.pawns[i].GetComponent<LudoPawnController>().pawnInJoint = this.gameObject;
                                        
                                        //pawnTop.SetActive(false);
                                        //pawnTopMultiple.SetActive(true);
                                        //pathController.pawns[i].GetComponent<LudoPawnController>().pawnTop.SetActive(false);
                                        //pathController.pawns[i].GetComponent<LudoPawnController>().pawnTopMultiple.SetActive(true);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (pawnInJoint != null)
                        {
                            canSendFinishTurn = false;
                            //pawnTop.SetActive(true);
                            //pawnTopMultiple.SetActive(false);
                            //pawnInJoint.GetComponent<LudoPawnController>().pawnTop.SetActive(true);
                            //pawnInJoint.GetComponent<LudoPawnController>().pawnTopMultiple.SetActive(false);

                            pawnInJoint.GetComponent<LudoPawnController>().pawnInJoint = null;
                            pawnInJoint = null;
                        }
                    }

                    otherCount = pathController.pawns.Count;

                    if (pawnInJoint == null)
                    {
                        RepositionPawns(otherCount, currentPosition);
                    }

                    if (currentPosition == path.Length - 1)
                    {
                        inHomeSound.Play();
                    }

                    if ((myTurn || GameManager.Instance.currentPlayer.isBot) && currentPosition == path.Length - 1)
                    {
                        
                        GameManager.Instance.currentPlayer.finishedPawns++;
                        Debug.Log("FINISHSSSS: " + GameManager.Instance.currentPlayer.finishedPawns + " total:" + GameManager.Instance.currentPlayer.pawns.Length);

                        //ludoController.finishedPawns++;
                        if (GameManager.Instance.mode == MyGameMode.Quick)
                        {
                            if (GameManager.Instance.currentPlayer.finishedPawns == 1)
                            {
                                ludoController.gUIController.FinishedGame(GameManager.Instance.currentPlayer.id);
                                return;
                            }
                        }
                        else
                        {
                            if (GameManager.Instance.currentPlayer.finishedPawns == GameManager.Instance.currentPlayer.pawns.Length)
                            {
                                ludoController.gUIController.FinishedGame(GameManager.Instance.currentPlayer.id);
                                return;
                            }
                        }
                        //ludoController.nextShotPossible = true;
                    }

                    if (((myTurn && GameManager.Instance.diceShot) || GameManager.Instance.currentPlayer.isBot) && canSendFinishTurn)
                    {
                        if (ludoController.nextShotPossible)
                        {
                            StartCoroutine(GameManager.Instance.currentPlayer.dice.GetComponent<GameDiceController>().EnableShot());
                            ludoController.gUIController.restartTimer();
                        }
                        else
                        {
                            Debug.Log("move finished call finish turn");
                            StartCoroutine(CheckTurnDelay());
                        }
                    }
                    else
                    {
                        ludoController.gUIController.restartTimer();
                    }
                }
            }
        }

        private IEnumerator CheckTurnDelay()
        {
            yield return new WaitForSeconds(1.0f);
            ludoController.gUIController.SendFinishTurn();
        }

        private void RepositionPawns(int otherCount, int currentPosition)
        {
            Debug.Log("Other Count: " + otherCount + " " + currentPosition);
            LudoPathObjectController pathController = path[currentPosition].GetComponent<LudoPathObjectController>();

            float scale = 0.8f;
            float offset = 20f / otherCount;
            float startPos = 0;

            startPos = (-offset / 2) * otherCount + offset / 2;
            scale = 1 - 0.05f * otherCount + 0.05f;

            /*if (otherCount == 1)
            {
                startPos = 0;
                scale = 1;
            }
            else if (otherCount == 2)
            {
                startPos = -offset / 2;
                scale = 0.95f;
            }
            else if (otherCount == 3)
            {
                startPos = -offset;
                scale = 0.85f;
            }
            else if (otherCount == 4)
            {
                startPos = -offset * 1.5f;
                scale = 0.75f;
            }*/

            // Get my pawns, push on top of stack
            List<int> orderPawns = new List<int>();

            for (int i = 0; i < pathController.pawns.Count; i++)
            {
                if (i < pathController.pawns.Count && pathController.pawns[i].GetComponent<LudoPawnController>().playerIndex == GameManager.Instance.myPlayerIndex)
                {
                    orderPawns.Add(i);
                }
                else
                {
                    orderPawns.Insert(0, i);
                }
            }
            // Reposition pawns if more than 1 on spot
            for (int i = 0; i < pathController.pawns.Count; i++)
            {
                RectTransform rT = pathController.pawns[orderPawns[i]].GetComponent<RectTransform>();
                pathController.pawns[orderPawns[i]].GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    path[currentPosition].GetComponent<RectTransform>().anchoredPosition.x + startPos + i * offset,
                    path[currentPosition].GetComponent<RectTransform>().anchoredPosition.y);
                pathController.pawns[orderPawns[i]].GetComponent<RectTransform>().localScale = new Vector2(initScale.x * scale, initScale.y * scale);

                pathController.pawns[orderPawns[i]].GetComponent<RectTransform>().SetAsLastSibling();

            }


            // }
        }

        private void UpdatePosition(Vector2 pos)
        {
            rect.anchoredPosition = pos;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}