using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
public class GameController : MonoBehaviour {
    public static GameController instance = null;
    public Text turnText;
    public Text dataText;
    public Image resultPanel;
    public Image creditsPanel;
    public Button creditsButton;
    public int totalCapturebyPlayer;
    public int totalCapturebyEnemy;
    public Text userName;
    public Text coins;
    public enum Turn
    {
        enemyTurn,
        playerTurn,
        playerTurn1
    }
    public Turn turn;
    private int turnsKingMoving;
    private bool inFinal = false;
    private int finalCounter = 0;
    private bool isGameOver = false;
    public Board board;
    private Bot bot;
    private Player player;
    private PlayerOther playerOther;

    public List<BoardConfiguration> historic;
    public Text _playerCapture;
    public Text _enemyCapture;
    public Text displayName, displayCoins, mobileNumber, displayWinningCoins;
    public bool isPlayerTurn;
    public int myTurn; 
    void Awake()
    {
        instance = this;
       
        coins.text = UserDetailsManager.userCoins.ToString();
        userName.text = UserDetailsManager.userName;

        displayName.text = UserDetailsManager.userName;
        displayCoins.text = UserDetailsManager.userCoins.ToString();
        mobileNumber.text = UserDetailsManager.userPhone;
        displayWinningCoins.text = "Total Winnings:"+ UserDetailsManager.userCoinsWon.ToString();

       
        
        
       
        
        turn = Turn.enemyTurn;
        if (resultPanel != null)
            resultPanel.gameObject.SetActive(false);
        else
            Debug.LogError("Couldn't find the result panel object.");

        if (creditsPanel != null)
            creditsPanel.gameObject.SetActive(false);
        else
            Debug.LogError("Couldn't find the credits panel object.");
        
        historic = new List<BoardConfiguration>();
      //  Load();
        bot = new Bot(historic);
        player = new Player();
        playerOther = new PlayerOther();
        turnsKingMoving = 0;
    }
    public void OnClickPrevious()
    {
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        Time.timeScale = 1;
    }
    public void OnCaputrebyPlayer()
    {
        _playerCapture.text = totalCapturebyPlayer.ToString();
    }
    public void OnCaputrebyEnemy()
    {
        _enemyCapture.text = totalCapturebyEnemy.ToString();
    }
    /*
     * Initialize variables.
     */
    void Start()
    {
        board = GameObject.FindGameObjectWithTag("Board").GetComponent<Board> ();
        StartCoroutine(LateStart(0.5f));
        
        if(creditsButton == null)
            Debug.LogError("Credits Button not Found.");
        else
        {
            creditsButton.onClick.AddListener(() => creditsPanel.gameObject.SetActive(true));
        }
        if (turnText == null)
            Debug.LogError("Turn Text not Found.");
        if (dataText == null)
            Debug.LogError("Data text not founded!");

        


       // Debug.Log(board.allTiles[20].GetComponent<TileHandler>());

        if (historic == null)
            dataText.text = "Dados: 0";
        else
            dataText.text = "Dados: " + historic.Count;
    }
    
    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        this.NextTurn();
    }

    
    void Update()
    {

        
        if (Input.GetKeyUp(KeyCode.S))
        {
            Save(new List<BoardConfiguration>());
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            Load();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            Clear();
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            Test();
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            Debug.Log("historic size: " + historic.Count);
        }
    }

    /// <summary>
    /// Random function to write some persitance tests in it.
    /// </summary>
    /// <remarks>
    /// Only used for Debug Purpose.
    /// </remarks>
    public void Test()
    {
        if(CheckersMultiplayer.Instance.IsTableTen)
        {
            BoardConfiguration foo =

                new BoardConfiguration("b#b###################w######w####B###b#########W########w###w######################################");
            Movement movement = new Movement(new IntVector2(7, 1),
            new IntVector2(3, 5), new IntVector2(5, 3));
            foo.AddMovement(movement, 5f);

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameStorage.dat", FileMode.Open);
            historic = (List<BoardConfiguration>)bf.Deserialize(file);
            file.Close();

            bool result = false;
            bool result2 = false;
            foreach (BoardConfiguration bc in historic)
            {
                if (bc.Equals(foo))
                {
                    result = true;
                    Debug.Log("find: " + bc.ToString());

                    Debug.Log("movement contains: " +
                        bc.HasMovementConfiguration(foo.GetMovementsConfigurations()[0]));
                }
            }

            result2 = historic.Contains(foo);

            Debug.Log("result for: " + result + "\nresult contain: " + result2);
        }
        else
        {
            BoardConfiguration foo =
                new BoardConfiguration("b#b###################w######w####B###b#########W########w###w##");
            Movement movement = new Movement(new IntVector2(7, 1),
            new IntVector2(3, 5), new IntVector2(5, 3));
            foo.AddMovement(movement, 5f);

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameStorage.dat", FileMode.Open);
            historic = (List<BoardConfiguration>)bf.Deserialize(file);
            file.Close();

            bool result = false;
            bool result2 = false;
            foreach (BoardConfiguration bc in historic)
            {
                if (bc.Equals(foo))
                {
                    result = true;
                    Debug.Log("find: " + bc.ToString());

                    Debug.Log("movement contains: " +
                        bc.HasMovementConfiguration(foo.GetMovementsConfigurations()[0]));
                }
            }

            result2 = historic.Contains(foo);

            Debug.Log("result for: " + result + "\nresult contain: " + result2);
        }

        

       
        
    }

    /// <summary>
    /// Increment and save the game historic in the 'gameStorage' file.
    /// </summary>
    public void Save(List<BoardConfiguration> list)
    {
        Debug.Log("Save Called.");
        // Create a File.
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gameStorage.dat");

        // Add the new board configuration list in the historic.
        bool existsConf;
        foreach (BoardConfiguration config in list)
        {
            existsConf = false;
            foreach(BoardConfiguration historicConfig in historic)
            {
                // See if it's has a new Movement for that existing configuration.
                if (historicConfig.Equals(config))
                {
                    existsConf = true;
                    if (!historicConfig.HasMovementConfiguration(config.GetMovementsConfigurations()[0]))
                    {
                        historicConfig.AddMovement(config.GetMovementsConfigurations()[0].GetMove(),
                        config.GetMovementsConfigurations()[0].GetAdaptation());
                    } 
                }
            }
            if (!existsConf)
            {
                historic.Add(config);
            }
        }

        bf.Serialize(file, historic);
        file.Close();
    }

    /// <summary>
    /// Load the game historic in the 'gameStorage' file.
    /// </summary>
    public void Load()
    {
        Debug.Log("Load Called.");
        
        if (File.Exists(Application.persistentDataPath + "/gameStorage.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameStorage.dat", FileMode.Open);
            historic = (List<BoardConfiguration>) bf.Deserialize(file);
            file.Close();

            /*
            foreach(BoardConfiguration conf in historic)
            {
                Debug.Log(conf.ToString());
            }
            */
        }
    }

    /// <summary>
    /// Clear the game historic in the 'gameStorage' file.
    /// </summary>
    /// <remarks>
    /// Only used for Debug Purpose.
    /// </remarks>
    public void Clear()
    {
        Debug.Log("Clear Called.");
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gameStorage.dat");
        bf.Serialize(file, new List<BoardConfiguration>());
        file.Close();

    }

    /// <summary>
    /// Change the turn between the enemy and the player.
    /// Also update the UI text.
    /// </summary>
    public void NextTurn()
    {
        board.RefreshAllPieces();

        
        if (turn == Turn.playerTurn)
        {
           
            if (CheckersMultiplayer.Instance.IsMultiPlayer)
            {
                if (!PhotonNetwork.isMasterClient)
                {
                    turn = Turn.enemyTurn;


                    turnText.text = "CPU";
                    board.SomePieceCanCaptureE();
                    playerOther.Play();
                    playerOther.HighlightPlayablePieces(board.GetEnemyPieces());
                }

            }
            else
            {
                turn = Turn.enemyTurn;
                turnText.text = "Player";
                StartCoroutine(BotPlay());
            }
            

        }
        else
        {
            if (CheckersMultiplayer.Instance.IsMultiPlayer)
            {
                if (PhotonNetwork.isMasterClient)
                {
                    Debug.Log("here ");
                    turn = Turn.playerTurn;
                    turnText.text = "CPU";
                    board.SomePieceCanCapture();
                    player.Play();
                    player.HighlightPlayablePieces(board.GetPlayerPieces());
                }
            }
            else
            {
                Debug.Log("here ");
                turn = Turn.playerTurn;
                turnText.text = "CPU";
                board.SomePieceCanCapture();
                player.Play();
                player.HighlightPlayablePieces(board.GetPlayerPieces());
            }


        }


        /*
        if(CheckersMultiplayer.Instance.myTurn==0)
        {
            board.SomePieceCanCaptureE();
            playerOther.Play();
            playerOther.HighlightPlayablePieces(board.GetEnemyPieces());


            Debug.Log("here ");
           
            CheckersMultiplayer.Instance.myTurn = 1;
        }

        else 
        {

            turnText.text = "CPU";
           
            CheckersMultiplayer.Instance.myTurn = 0;

            turn = Turn.playerTurn;
            turnText.text = "CPU";
            board.SomePieceCanCapture();
            player.Play();
            player.HighlightPlayablePieces(board.GetPlayerPieces());
        }
       */
       
    }

   

    IEnumerator BotPlay()
    {
        yield return new WaitForSeconds(1.0f);
        bot.Play();
    }

    public void SendToPlayer(TileHandler tile)
    {
        // Lakhbir
      

        player.SelectionHandler(tile);
       


       

    }
    public void SendOtherToPlayer(TileHandler tile)
    {
        // Lakhbir
        playerOther.SelectionHandler(tile);
        


    }


    public void NotifyPlayerEndOfMovement()
    {
        bool isSucessiveCapture = false;
        float finalValue = -100f;
        IsInFinals();
        if (turn == Turn.playerTurn)
        {
            // See if just move a king piece
            RefreshDrawCounters(player);

            player.NotifyEndOfMovement();
            isSucessiveCapture = player.GetIsSucessiveCapture();
            if (!isSucessiveCapture)
                board.RefreshAllPieces();
            // Verify if the player won the game.
            if (WinGame(bot, this.board.GetEnemyPieces()) && resultPanel != null)
            {
                finalValue = -30f;
                ShowResultPanel("WIN !", true);

                

                PhotonView pv = PhotonView.Get(CheckersMultiplayer.Instance.photonView);

                pv.RPC("LostCall", PhotonTargets.Others);
            }
        }
        else
        {
            // See if just move a king piece
            RefreshDrawCounters(playerOther);

            playerOther.NotifyEndOfMovement();
            isSucessiveCapture = playerOther.GetIsSucessiveCaptureOpponent();
            if (!isSucessiveCapture)
                board.RefreshAllPieces();
            // Verify if the bot won the game.
            if (WinGame(playerOther, this.board.GetPlayerPieces()) & resultPanel != null)
            {
                finalValue = -30f;
                ShowResultPanel("WIN !", true);

               // StartCoroutine(CheckersMultiplayer.Instance.TransactionPool(UserDetailsManager.userId, CheckersMultiplayer.Instance.playerUserId));

                PhotonView pv = PhotonView.Get(CheckersMultiplayer.Instance.photonView);

                pv.RPC("LostCall", PhotonTargets.Others);
            }
        }

        if (inFinal)
            finalCounter += 1;

        if (turnsKingMoving >= 20 || finalCounter >= 10)
        {
            finalValue = 10f;
            ShowResultPanel("DRAW !", false);
            StartCoroutine(CheckersMultiplayer.Instance.DrawPool());
        }
        if (isGameOver)
        {
            if (finalValue > -100f)
                bot.SetLastMovement(finalValue);
            Save(bot.GetConfigList());
        }
        if (!isSucessiveCapture && !isGameOver)
        {
            this.NextTurn();
        }
        if (isSucessiveCapture && !isGameOver)
        {
            this.NextTurn();
        }
    }


    public void NotifyPlayerEndOfMovementBot()
    {
        bool isSucessiveCapture = false;
        float finalValue = -100f;
        IsInFinals();
        if (turn == Turn.playerTurn)
        {
            // See if just move a king piece
            RefreshDrawCounters(player);

            player.NotifyEndOfMovement();
            isSucessiveCapture = player.GetIsSucessiveCapture();
            if (!isSucessiveCapture)
                board.RefreshAllPieces();
            // Verify if the player won the game.
            if (WinGame(bot, this.board.GetEnemyPieces()) && resultPanel != null)
            {
                finalValue = -30f;
                ShowResultPanel("WIN !", true);
            }
        }
        else
        {
            // See if just move a king piece
            RefreshDrawCounters(bot);

            bot.NotifyEndOfMovement();
            isSucessiveCapture = bot.GetIsSucessiveCapture();
            if (!isSucessiveCapture)
                board.RefreshAllPieces();
            // Verify if the bot won the game.
            if (WinGame(player, this.board.GetPlayerPieces()))
            {
                finalValue = 30f;
                ShowResultPanel("LOSE !", false);
            }
        }

        if (inFinal)
            finalCounter += 1;

        if (turnsKingMoving >= 20 || finalCounter >= 10)
        {
            finalValue = 10f;
            //StartCoroutine(CheckersMultiplayer.Instance.DrawPool());

            ShowResultPanel("DRAW !", false);
        }
        if (isGameOver)
        {
            if (finalValue > -100f)
                bot.SetLastMovement(finalValue);
            Save(bot.GetConfigList());
        }
        if (!isSucessiveCapture && !isGameOver)
            
            this.NextTurn();
    }

   
    [PunRPC]
    public void LostCall()
    {
        ShowResultPanel("LOSE !", false);
    }

    [PunRPC]
    public void Win()
    {
        ShowResultPanel("VICTORY", true);
    }
    /// <summary>
    /// Verify the winning condition given a player.
    /// </summary>
    /// <remarks>
    /// #### CONDITIONS ####
    /// 1- the player hasn't pieces.
    /// 2- the player can't move the pieces his has.
    /// </remarks>
    private bool WinGame(AbstractPlayer absEnemy, ArrayList enemiesPieces)
    {
        return enemiesPieces.Count == 0 ||
            (!absEnemy.SomePieceCanCapture(enemiesPieces) &&
            !absEnemy.SomePieceCanWalk(enemiesPieces));
    }

    /// <summary>
    /// Updates the turnKingMoving variable that is incremented
    /// when some player just moves a king.
    /// </summary>
    private void RefreshDrawCounters(AbstractPlayer absPlayer)
    {
        if (absPlayer.UsedKingPiece() && !absPlayer.GetIsCapturing())
        {
            turnsKingMoving += 1;
        }
        else
        {

            turnsKingMoving = 0;
        }
    }

    /// <summary>
    /// See the condition to start with the final countdown.
    /// If the game do not finish
    /// </summary>
    private void IsInFinals ()
    {
        if ( !inFinal &&
            this.board.GetPlayerPieces().Count <= 2 &&
            this.board.GetEnemyPieces().Count <= 2 &&
            this.board.NumberOfPlayerManPieces() + this.board.NumberOfEnemyManPieces() <= 1 )
        {
            Debug.Log("Estamos em finais");
            inFinal = true;
        }
    }

    /// <summary>
    /// Finish the game.
    /// Open the result panel with the text given as parameter.
    /// </summary>
    public void ShowResultPanel(string text, bool hasPlayerWin)
    {
        if(hasPlayerWin==true)
        {

            StartCoroutine(CheckersMultiplayer.Instance.TransactionPool(UserDetailsManager.userId, CheckersMultiplayer.Instance.playerUserId));
        }
        isGameOver = true;
        /*
        if (resultPanel.gameObject.activeSelf)
            return;
            */
        resultPanel.gameObject.SetActive(true);
        resultPanel.GetComponent<PanelController>().PlaySound(hasPlayerWin);
        Text resultText = resultPanel.transform.GetChild(0).GetComponent<Text>();
        resultText.text = text;
    }

    /// <summary>
    /// Return true if is the player turn.
    /// </summary>
    public bool IsPlayerTurn()
    {
        if (turn == Turn.playerTurn)
            return true;
        return false;
    }

    public List<BoardConfiguration> getHistoric()
    {
        return historic;
    }
}
