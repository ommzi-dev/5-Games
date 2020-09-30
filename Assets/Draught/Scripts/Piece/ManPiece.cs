using System.Collections;
using UnityEngine;

public class ManPiece : Piece {
    
    protected int forward;
    protected string kingVersionPath;
    
    /// <summary>
    /// Return a list of Movements given the current position.
    /// </summary>
    public override ArrayList GetCaptureMovements(IntVector2 currentPos, ArrayList path)
    {
       // Debug.LogError("HIS I");
        ArrayList possibleCaptureMovents = new ArrayList();

        if (CanCapture(1, 1, currentPos) && !base.AlreadyCaptured(path, new IntVector2(currentPos.x + 1, currentPos.y + 1)))
        {
            possibleCaptureMovents.Add(new Movement(currentPos,
                new IntVector2(currentPos.x + 2, currentPos.y + 2),
                new IntVector2(currentPos.x + 1, currentPos.y + 1)));
        }
        if (CanCapture(1, -1, currentPos) && !base.AlreadyCaptured(path, new IntVector2(currentPos.x + 1, currentPos.y - 1)))
        {
            possibleCaptureMovents.Add(new Movement(currentPos,
                new IntVector2(currentPos.x + 2, currentPos.y - 2),
                new IntVector2(currentPos.x + 1, currentPos.y - 1)));
        }
        if (CanCapture(-1, 1, currentPos) && !base.AlreadyCaptured(path, new IntVector2(currentPos.x - 1, currentPos.y + 1)))
        {
            possibleCaptureMovents.Add(new Movement(currentPos,
                new IntVector2(currentPos.x - 2, currentPos.y + 2),
                new IntVector2(currentPos.x - 1, currentPos.y + 1)));
        }
        if (CanCapture(-1, -1, currentPos) && !base.AlreadyCaptured(path, new IntVector2(currentPos.x - 1, currentPos.y - 1)))
        {
            possibleCaptureMovents.Add(new Movement(currentPos,
                new IntVector2(currentPos.x - 2, currentPos.y - 2),
                new IntVector2(currentPos.x - 1, currentPos.y - 1)));
        }

        return possibleCaptureMovents;
    }

    /// <summary>
    /// Return all the walk movements of the piece in a ArrayList.
    /// </summary>
    public override ArrayList GetWalkMovements()
    {
        ArrayList possibleWalkMovents = new ArrayList();
        ArrayList possibleWalkMoventsHighlight = new ArrayList();
        Vector2Int temp;

        if (CanWalk(forward, 1))
        {
           possibleWalkMovents.Add(new Movement(base.position, new IntVector2(base.position.x + forward, base.position.y + 1)) );
          //  Debug.Log("possible moves " + base.position );

            /*
            foreach (Piece piece in board.allPlayerPieces)
            {
                IntVector2 tempN;
                tempN = new IntVector2(piece.transform.parent.GetComponent<TileHandler>().getRow(), piece.transform.parent.GetComponent<TileHandler>().getColumn());
                temp = new Vector2Int( piece.transform.parent.GetComponent<TileHandler>().row, piece.transform.parent.GetComponent<TileHandler>().column);
                possibleWalkMovents.Add(new Movement(tempN, new IntVector2(temp.x + forward, temp.y+1)));
               
               
            }
            */
            /*
            Vector2Int temp;
            foreach (Piece piece in board.allPlayerPieces)
            {
                temp = new Vector2Int(piece.transform.parent.GetComponent<TileHandler>().row , piece.transform.parent.GetComponent<TileHandler>().column );
                possibleWalkMoventsHighlight.Add(new Movement(board.allPlayerPieces.IndexOf(i), new IntVector2(temp.x + forward, temp.y+1)));
                for (int j = 0; j < possibleWalkMoventsHighlight.Count; j++)
                {
                    Debug.Log("possible moves " + possibleWalkMoventsHighlight[j]);

                }
                //Debug.Log("Col "+ piece.transform.parent.GetComponent<TileHandler>().column + "Row " + piece.transform.parent.GetComponent<TileHandler>().row);
                // Debug.Log("NAME  " + piece.name);

            }
            */
        }
        if (CanWalk(forward, -1))
        {
            possibleWalkMovents.Add(new Movement(base.position, new IntVector2(base.position.x + forward, base.position.y - 1)));

            /*
            foreach (Piece piece in board.allPlayerPieces)
            {
                IntVector2 tempN;
                tempN = new IntVector2(piece.transform.parent.GetComponent<TileHandler>().getRow(), piece.transform.parent.GetComponent<TileHandler>().getColumn());
                temp = new Vector2Int(piece.transform.parent.GetComponent<TileHandler>().row, piece.transform.parent.GetComponent<TileHandler>().column);
                possibleWalkMovents.Add(new Movement(tempN, new IntVector2(temp.x + forward, temp.y - 1)));


            }
            */
        }
       

        /*
        Vector2Int temp;
        for (int i = 0; i < board.allPlayerPieces.Count; i++)
        {
           
                temp = new Vector2Int(board.allPlayerPieces[i].GetComponent<TileHandler>().row, board.allTiles[i].GetComponent<TileHandler>().column + 1);
                possibleWalkMoventsHighlight.Add(new Movement(base.position, new IntVector2(temp.x + forward, temp.y)));
                for (int j = 0; j < possibleWalkMoventsHighlight.Count; j++)
                {
                    Debug.Log("possible moves " + possibleWalkMoventsHighlight[j]);

                }
            
        }
        
        */


      //  Debug.Log("base.position  " + base.position + " base.position.x " + base.position.x + " base.position.y" + base.position.y);
        
        return possibleWalkMovents;
    }

    /// <summary>
    /// Return true if this piece can capture the other one located in
    /// a given position + offset.
    /// </summary>
    protected override bool CanCapture(int offsetX, int offsetY, IntVector2 pos)
    {
        if(board == null)
        {

            Debug.Log("board is null in " + this.name + ": " + base.position.ToString());
            return false;
        }

        if (base.board.WithinBounds(pos.x + offsetX, pos.y + offsetY))
        {
            TileHandler nextTile =
                base.board.GetTile(pos.x + offsetX, pos.y + offsetY);
            /**
             * See if:
             * The nextTile is occupied.
             * Has a enemy Piece in there.
             * the enemy has NOT been captured yet.
             * and the following tile is within bounds.
             */
            if (nextTile.transform.childCount != 0
                && nextTile.transform.GetChild(0).CompareTag(enemy_tag)
                && !nextTile.transform.GetChild(0).GetComponent<Piece>().HasBeenCaptured()
                && base.board.WithinBounds(pos.x + 2 * offsetX, pos.y + 2 * offsetY))
            {
                nextTile =
                    base.board.GetTile(pos.x + 2 * offsetX, pos.y + 2 * offsetY);
                if (nextTile.transform.childCount == 0)
                {
                    return true;
                }

            }
        }

        return false;
    }

    //// <summary>
    /// Verify if the tile 'currentPosition + offset' can be walked to.
    /// </summary>
    protected override bool CanWalk(int offsetX, int offsetY)
    {
        if (base.board.WithinBounds(base.position.x + offsetX, base.position.y + offsetY))
        {

            TileHandler nextTile = base.board.GetTile(base.position.x + offsetX, base.position.y + offsetY);
            // See if the nextTile is occupied.
            if (nextTile.transform.childCount == 0)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Create a new king Piece and replace this one.
    /// </summary>
    public void Promote()
    {
        // Find the respective promotion line.
        int promotionLine = 1;
        if (this.forward == 1)
        {
            if(CheckersMultiplayer.Instance.IsTableTen)
            { promotionLine = 10; }
            else
            { promotionLine = 8; }
           
        }
         
        if(base.position.x == promotionLine)
        {
            // Create a new King Piece and set it in the same position as this.
            GameObject kingVersion = Resources.Load<GameObject>(this.kingVersionPath);
            if(kingVersion == null)
            {
                Debug.LogError("Path for piece promotion not founded.");
            }
            else
            {
                Piece newPiece = Instantiate(kingVersion, transform.parent.transform, false).GetComponent<Piece>();
                if (newPiece == null)
                {
                    Debug.LogError("Can't create a new piece.");
                    return;
                }
                newPiece.SetCurrentPosition();
                // Destroy this piece
                Destroy(this.gameObject);
            }
        }
    }
}