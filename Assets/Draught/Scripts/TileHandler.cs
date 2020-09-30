using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour {

    // Private Variables.
    public static Dictionary<char, int> collumDic;
    /*= new Dictionary<char, int>()
    {
        {'A', 1},
        {'B', 2},
        {'C', 3},
        {'D', 4},
        {'E', 5},
        {'F', 6},
        {'G', 7},
        {'H', 8},
        {'I', 9},
        {'J', 10},
    };
    */
    private Board board;
    public int row;
    public int column;
    private AudioClip sound;
    private AudioSource soundSource;
    public bool IsRed = false;
    public string tilePos;
	// Use this for initialization
	void Awake () {

        board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board> ();
        
    if(CheckersMultiplayer.Instance.IsTableTen)
    {
        collumDic = new Dictionary<char, int>()
    {
        {'A', 1},
        {'B', 2},
        {'C', 3},
        {'D', 4},
        {'E', 5},
        {'F', 6},
        {'G', 7},
        {'H', 8},
        {'I', 9},
        {'J', 10},
    };
    }
    else
    {
            collumDic = new Dictionary<char, int>()
    {
        {'A', 1},
        {'B', 2},
        {'C', 3},
        {'D', 4},
        {'E', 5},
        {'F', 6},
        {'G', 7},
        {'H', 8},
       
    };
        }
        // Add the 'clickHandler' method to the onClick listener.
        Button btn = GetComponent<Button> ();
        btn.onClick.AddListener (ClickHandler);

        if(this.GetComponent<Image>().color == board.redLineColor)
        {
            IsRed = true;
        }
        // Get Tile Position (row and collumn) by it's name.

        if(CheckersMultiplayer.Instance.IsTableTen)
        {

            string tileName = transform.name;
            if ((tileName == "BlackTile10A") || (tileName == "Tile10B") || (tileName == "BlackTile10C") || (tileName == "Tile10D")
                 || (tileName == "BlackTile10E") || (tileName == "Tile10F") || (tileName == "BlackTile10G") || (tileName == "Tile10H")
                 || (tileName == "BlackTile10I") || (tileName == "Tile10J"))
            {
                tilePos = tileName.Substring(tileName.Length - 3, 3);
                string tilePosition = tileName.Substring(tileName.Length - 3, 3);
                //row = (int)System.Char.GetNumericValue(tilePosition[1]);
                row = 10;

                column = collumDic[tilePosition[2]];



            }
            else
            {
                tilePos = tileName.Substring(tileName.Length - 2, 2);
                string tilePosition = tileName.Substring(tileName.Length - 2, 2);
                row = (int)System.Char.GetNumericValue(tilePosition[0]);
                column = collumDic[tilePosition[1]];

            }
        }
        else
        {
            string tileName = transform.name;
            string tilePosition = tileName.Substring(tileName.Length - 2, 2);
            row = (int)System.Char.GetNumericValue(tilePosition[0]); ;
            column = collumDic[tilePosition[1]];
        }
        

       
        //Debug.Log("row: " + tilePosition[0] + " collumn: " + tilePosition[1]);
    }

    public void ClickHandler()
    {
        for (int i = 0; i < board.HighLightedArray.Count; i++)
        {
            board.HighLightedArray[i].GetComponent<Image>().color = Color.black;
            if(board.HighLightedArray[i].GetComponent<TileHandler>().IsRed)
            {
                board.HighLightedArray[i].GetComponent<Image>().color = board.redLineColor;

            }
        }
        board.HighLightedArray.Clear();

        if (CheckersMultiplayer.Instance.IsMultiPlayer)
        {
            if(PhotonNetwork.isMasterClient)
            {

                if (CheckersMultiplayer.Instance.turnof == "player")
                {
                    board.TileClicked(this);
                }
            }
            else
            {

                
            }
            if (CheckersMultiplayer.Instance.turnof == "opponent")
            {
                board.TileClickedOther(this);
            }
            /*
            if (GameController.instance.turn == GameController.Turn.playerTurn)
            {
              //  Debug.Log("Frome " + board.allTiles.IndexOf(gameObject));
                
                board.TileClicked(this);
            }
            if (GameController.instance.turn == GameController.Turn.enemyTurn)
            {
                board.TileClickedOther(this);
            }
            */
        }
        else
        {
            board.TileClicked(this);
        }

    }

    public int getRow()
    {
        return this.row;
    }

    public int getColumn()
    {
        return this.column;
    }

    public IntVector2 getPosition()
    {
        return new IntVector2(this.row, this.column);

    }

    public bool HasChild()
    {
       
        return (this.transform.childCount > 0);

    }
    public bool HasChildEnemy()
    {
        if(this.transform.childCount > 0)
        {
            if (this.transform.GetChild(0).name == "GrayPiece")
            {
                return true;
            }
            
        }
        return false;
        
       
    }
    public GameObject GetChild()
    {
        if (HasChild())
        {
            return this.transform.GetChild(0).gameObject;
        }
        return null;
    }
}
