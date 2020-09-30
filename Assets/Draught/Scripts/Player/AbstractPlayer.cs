using System.Collections;
using UnityEngine;
using UnityEngine.UI;

abstract public class AbstractPlayer {

    protected Board board;
    protected Piece currentPiece = null;
    protected bool isCapturing = false;
    protected bool isSucessiveCapture = false;

    /**
     * Sign the start of that player's turn.
     */
    public abstract void Play();

    /**
     * It's called when the movement chose by this player is finished.
     */
    public abstract void NotifyEndOfMovement();

    /// <summary>
    /// Return true if the current piece used was a king.
    /// </summary>
    public bool UsedKingPiece()
    {
        if (currentPiece != null && currentPiece.GetComponent<KingPiece>())
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Verify if some piece int the piece list parameter can capture.
    /// Return true if some piece can capture and false if doesn't.
    /// </summary>
    public bool SomePieceCanCapture(ArrayList piecesList)
    {
        ArrayList captureMovements;
        foreach (Piece piece in piecesList)
        {
            captureMovements = piece.GetCaptureMovements();
          //  UnityEngine.Debug.Log("GetCaptureMovements " + captureMovements.Count);
          
            if (captureMovements.Count != 0  )
            {
                return true;
            }
        }
        return false;
    }

    public void HighlightPlayablePieces(ArrayList piecesList)
    {
        int table;
        if(CheckersMultiplayer.Instance.IsTableTen)
        {
            table = 10;
        }
        else
        {
            table = 8;
        }
        foreach (Piece piece in piecesList)
        {
            TileHandler tile = piece.transform.parent.GetComponent<TileHandler>();
            IntVector2 pos = tile.getPosition();
            int targetRow = pos.x + 1;
            int targetColumn = pos.y - 1;

            for (int i = 0; i < 2; i++)
            {
                if (targetColumn > 0 && targetColumn<= table && targetRow > 0 && targetRow <= table)
                {
                    TileHandler possibleTile = board.GetTile(targetRow, targetColumn);
                    
                   
                    if (!possibleTile.HasChild())
                    {
                        //Green that tile
                        tile.GetComponent<Image>().color = Color.green;
                        board.HighLightedArray.Add(tile.gameObject);
                       
                    }
                    if(possibleTile.HasChildEnemy())
                    {
                        tile.GetComponent<Image>().color = Color.green;
                        board.HighLightedArray.Add(tile.gameObject);
                    }

                }
                targetColumn = pos.y + 1;
            }
        }
    }

    public bool SomePieceCanWalk(ArrayList piecesList)
    {
        ArrayList walkMovements;
        foreach (Piece piece in piecesList)
        {
            walkMovements = piece.GetWalkMovements();
            if (walkMovements.Count != 0)
            {
                
                return true;
            }
        }
        return false;
    }

    public void SetIsCapturing(bool value)
    {
        this.isCapturing = value;
    }

    public bool GetIsCapturing()
    {
        return this.isCapturing;
    }

    public bool GetIsSucessiveCapture()
    {
        if (CheckersMultiplayer.Instance.IsMultiPlayer)
        {
            if (this.isSucessiveCapture)
            {
                PhotonView pv = PhotonView.Get(CheckersMultiplayer.Instance.photonView);

                pv.RPC("onReceivedTurn", PhotonTargets.All, "player");
                GameController.instance.turn = GameController.Turn.enemyTurn;
                Debug.Log("on  player ");
            }
           
        }
        return this.isSucessiveCapture;
    }
    public bool GetIsSucessiveCaptureOpponent()
    {

        if(CheckersMultiplayer.Instance.IsMultiPlayer)
        {
            if(this.isSucessiveCapture)
            {
                PhotonView pv = PhotonView.Get(CheckersMultiplayer.Instance.photonView);

                pv.RPC("onReceivedTurn", PhotonTargets.All, "opponent");
                GameController.instance.turn = GameController.Turn.playerTurn;
                Debug.Log("on  opponent ");
            }
            
        }
        return this.isSucessiveCapture;
    }
}
