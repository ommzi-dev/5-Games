using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class WhotmultiMatch : MonoBehaviour {

	public Text avail_token;
	// Use this for initialization
	void Start () {
		
	}
	void OnEnable()
	{
        /*WebServices.instance.TotalTokens((string result1) =>{

			JSONNode j = JSON.Parse(result1);
			if(result1.Contains(WhotCommonConstants.ERROR_FIELD))
			{
				AlertController.instance.ShowAlertwith1options(j[WhotCommonConstants.ERROR_FIELD].Value);
				return;
			}
			var status1 = JSON.Parse(result1);

			WebServices.instance.currentUser.credit_balance = int.Parse( status1["balance"].Value);
			avail_token.text = "AVAILABLE TOKENS: "+WebServices.instance.currentUser.credit_balance.ToString();

		});*/
        avail_token.text= "AVAILABLE COINS : " + UserDetailsManager.userCoins;

    }

	

}
