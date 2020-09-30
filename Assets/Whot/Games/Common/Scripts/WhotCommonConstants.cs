using UnityEngine;
using UnityEngine.UI;

public class WhotCommonConstants
{

	public static string GameDifficultyEasy = "Easy";
	public static string GameDifficultyMedium = "Medium";
	public static string GameDifficultyHard = "Hard";

	public static string ChessWhite = "White";
	public static string ChessBlack = "Black";

	public static string chessDifficultyKey = "chessdifficulty";
	public static string chessColorKey = "chessColor";
	public static string chessFirstPlay = "chessFirstPlay";

	public static string SoundKey = "Sound";
	public static string soundOn="True";
	public static string soundOff="False";

	public static string Chess = "Chess";
	public static string Checkers = "Checkers";
	public static string FourToScore = "FourToScore";
	public static string WHOT = "WHOT";
	public static string WordWars = "WordWars";
	public static string Pool = "Pool";

	public static string PROFILE_CONSTANT = "profile";
	public static string ERROR_FIELD = "error";
	public static string DATA_FIELD = "data";
	public static string MESSAGE_FIELD = "mssg";
	public static string TOKEN_KEY = "token";
	public static string Success = "success";
	public static string ID = "user_id";

	public static string BANK_DETAILS= "bankdetails";
	public static string CREDITS = "credit_balance";

	public static int chessID = 1;
	public static int checkersID = 2;
	public static int fourtoscoreID = 3;
	public static int whotID = 4;
	public static int wwID = 5;
	public static int poolID = 6;
	public static string outcome_win="won";
	public static string outcome_lose="lost";

	public static string outcome_draw="draw";

	public static string stats = "stats";

	public static void AssignTexture(RawImage r, Texture t)
	{ 
		try
		{
			r.transform.localScale = new Vector3 (1,1,1);
			r.texture = t;
			r.transform.localScale = new Vector3 (r.transform.localScale.x, r.transform.localScale.y * ((float)t.height / (float)t.width), r.transform.localScale.z);
			//Debug.Log ("Y scal heighte"+t.height+" Widht: "+t.width);
		}
		catch {
		}
	}
}
