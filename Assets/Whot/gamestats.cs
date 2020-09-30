using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
public class gamestats : MonoBehaviour {


	public Text tm_c, wm_c, et_c, favorite_game, tm_d, wm_d, et_d,tm_f, wm_f,et_f, tm_w, wm_w, et_w,tm_ww, wm_ww, et_ww, tm_p, wm_p, et_p;
	// Use this for initialization
	void Start () {
		
	}
	
	void OnEnable()
	{
		
		/*WebServices.instance.GetGameStats ((string result) => {

			JSONNode data = JSON.Parse( JSON.Parse(result)["stats"].ToString());

			var status = JSON.Parse(result);

			//Debug.Log("favourite game: "+status["fav"].Value);

			if(string.IsNullOrEmpty(status["fav"].Value))
			{
				favorite_game.text = "NO FAVOURITE GAME YET";
			}
			else
			{
				favorite_game.text = "FAVOURITE GAME: "+status["fav"].Value.ToUpper();

			}
				
			for(int i = 0; i < data.Count; i++)
			{
				Stats stats = JsonUtility.FromJson<Stats>(data[i].ToString());
				//Debug.Log("Game Stats Info: "+stats.battles_won);
				if(string.Equals(stats.game_name.ToLower(),"chess"))
				{
					tm_c.text = stats.total_battles;
					wm_c.text = stats.battles_won;
					et_c.text = stats.coins_won;
				}
				if(string.Equals(stats.game_name.ToLower(),"draughts"))
				{
					tm_d.text = stats.total_battles;
					wm_d.text = stats.battles_won;
					et_d.text = stats.coins_won;
				}
				if(string.Equals(stats.game_name.ToLower(),"four to score"))
				{
					tm_f.text = stats.total_battles;
					wm_f.text = stats.battles_won;
					et_f.text = stats.coins_won;
				}

				if(string.Equals(stats.game_name.ToLower(),"whot"))
				{
					tm_w.text = stats.total_battles;
					wm_w.text = stats.battles_won;
					et_w.text = stats.coins_won;
				}
				if(string.Equals(stats.game_name.ToLower(),"word wars"))
				{
					tm_ww.text = stats.total_battles;
					wm_ww.text = stats.battles_won;
					et_ww.text = stats.coins_won;
				}
				if(string.Equals(stats.game_name,"8 ball pool"))
				{
					tm_p.text = stats.total_battles;
					wm_p.text = stats.battles_won;
					et_p.text = stats.coins_won;
				}

			}
				
		});*/
	}
}
