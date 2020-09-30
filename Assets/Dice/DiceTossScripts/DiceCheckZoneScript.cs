using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceCheckZoneScript : MonoBehaviour {
	public static DiceCheckZoneScript instance = null;
	Vector3 diceVelocity;

	// Update is called once per frame
	void FixedUpdate () {
		diceVelocity = DiceScript.diceVelocity;
//		Debug.Log("Dice Velocity " + diceVelocity);
	}
	int dice1;
	int dice2;
	bool isResult;
    private void Awake()
    {
		instance = this;
		isResult = false;

	}
    void OnTriggerStay(Collider col)
	{
        
		if (DiceGameManager.instance.RollButton.activeInHierarchy)
			return;
            
		DiceNumberTextScript.diceNumber = 0;
		if (diceVelocity.x == 0f && diceVelocity.y == 0f && diceVelocity.z == 0f)
		{
			switch (col.gameObject.name) {
                
			case "Side1":
                    if(col.gameObject.transform.parent.name== "Dice1")
                    {
						dice1 = 6;
						DiceGameManager.instance.isDiceOneStay = true;
                    }
					if (col.gameObject.transform.parent.name == "Dice2")
					{
							DiceGameManager.instance.isDiceTwoStay = true;
						dice2 = 6;
					}
					if (!DiceGameManager.instance.isMultiplayer)
					{
						CalculatedResultBot();
					}
                    else
                    {
						if (isResult==false)
						{
							CalculatedResultPlayer();
							
						}

					}
				break;
			case "Side2":
					if (col.gameObject.transform.parent.name == "Dice1")
					{
							DiceGameManager.instance.isDiceOneStay = true;
						dice1 = 5;
					}
					if (col.gameObject.transform.parent.name == "Dice2")
					{
							DiceGameManager.instance.isDiceTwoStay = true;
						dice2 = 5;
					}
					if (!DiceGameManager.instance.isMultiplayer)
					{
						CalculatedResultBot();
					}
					else
					{
						if (isResult == false)
						{
							CalculatedResultPlayer();
							
						}
					}
					break;
			case "Side3":
					if (col.gameObject.transform.parent.name == "Dice1")
					{
							DiceGameManager.instance.isDiceOneStay = true;
						dice1 = 4;
					}
					if (col.gameObject.transform.parent.name == "Dice2")
					{
							DiceGameManager.instance.isDiceTwoStay = true;
						dice2 = 4;
					}
					if (!DiceGameManager.instance.isMultiplayer)
					{
						CalculatedResultBot();
					}
					else
					{
						if (isResult == false)
						{
							CalculatedResultPlayer();
							
						}
					}
					break;
			case "Side4":
					if (col.gameObject.transform.parent.name == "Dice1")
					{
							DiceGameManager.instance.isDiceOneStay = true;
						dice1 = 3;
					}
					if (col.gameObject.transform.parent.name == "Dice2")
					{
							DiceGameManager.instance.isDiceTwoStay = true;
						dice2 = 3;
					}
					if (!DiceGameManager.instance.isMultiplayer)
					{
						CalculatedResultBot();
					}
					else
					{
						if(isResult == false)
						{
							CalculatedResultPlayer();
							
						}
					}

					break;
			case "Side5":
					if (col.gameObject.transform.parent.name == "Dice1")
					{
							DiceGameManager.instance.isDiceOneStay = true;
						dice1 = 2;
					}
					if (col.gameObject.transform.parent.name == "Dice2")
					{
							DiceGameManager.instance.isDiceTwoStay = true;
						dice2 = 2;
					}
					if (!DiceGameManager.instance.isMultiplayer)
					{
						CalculatedResultBot();
					}
					else
					{
						if (isResult == false)
						{
							CalculatedResultPlayer();
							
						}
					}
					break;
			case "Side6":
					if (col.gameObject.transform.parent.name == "Dice1")
					{
							DiceGameManager.instance.isDiceOneStay = true;
						dice1 = 1;
					}
					if (col.gameObject.transform.parent.name == "Dice2")
					{
							DiceGameManager.instance.isDiceTwoStay = true;
						dice2 = 1;
					}
					if (!DiceGameManager.instance.isMultiplayer)
					{
						CalculatedResultBot();
					}
					else
					{
						if (isResult == false)
						{
							CalculatedResultPlayer();
							
						}
					}

					break;
			}
			
		}
	}
	public bool isCoroutineStarted = false;
	public IEnumerator OnPopUp(string name)
    {
		isCoroutineStarted = true;
        
		DiceGameManager.instance.isWin = false;
		yield return new WaitForSecondsRealtime(2 * 2.5f);
        if(name=="win")
        {
			DiceGameManager.instance.winLoseImg.texture = DiceGameManager.instance.winLoseSp[0];
            if(DiceGameManager.instance.isMultiplayer)
            {
				StartCoroutine(DiceMultiplayer.Instance.TransactionPool(UserDetailsManager.userId, DiceMultiplayer.Instance.playerUserId));
			}

		}
		else if (name == "lose")
		{
			DiceGameManager.instance.winLoseImg.texture = DiceGameManager.instance.winLoseSp[1];

		}
		else if (name == "draw")
		{
            if(DiceGameManager.instance.isMultiplayer)
            {
				StartCoroutine(DiceMultiplayer.Instance.DrawPool());

			}
			DiceGameManager.instance.winLoseImg.texture = DiceGameManager.instance.winLoseSp[2];
		}
		DiceGameManager.instance.TryAgainButton.SetActive(true);
		

	}

    public void CalculatedResultBot()
    {
		if (DiceGameManager.instance.isDiceOneStay && DiceGameManager.instance.isDiceTwoStay)
		{
			//DiceGameManager.instance.TryAgainButton.SetActive(true);
			DiceNumberTextScript.diceNumber = dice1 + dice2;


			if (DiceGameManager.instance.playerCount > DiceGameManager.instance.cpuCount)
			{
				StartCoroutine(OnPopUp("win"));

			}
			else if (DiceGameManager.instance.playerCount < DiceGameManager.instance.cpuCount)
			{
				StartCoroutine(OnPopUp("lose"));

			}
			else
			{
				StartCoroutine(OnPopUp("draw"));
			}
		}
	}
    
   

	public void CalculatedResultPlayer()
	{
		
		if (DiceGameManager.instance.isDiceOneStay && DiceGameManager.instance.isDiceTwoStay)
		{
            int temp = dice1 + dice2;
			
			//DiceGameManager.instance.TryAgainButton.SetActive(true);
			//DiceNumberTextScript.diceNumber = temp;
			PhotonView pv = PhotonView.Get(DiceMultiplayer.Instance.photonView);
//			Debug.Log("Player send score");
			pv.RPC("OnPlayerScoreDice", PhotonTargets.Others, temp);
			isResult = true;
			DiceGameManager.instance.multiplayerScoreCount.text = temp.ToString();
			DiceGameManager.instance.multiplayerPlayerCount = temp;
			/*
			if (DiceGameManager.instance.playerCount > DiceGameManager.instance.cpuCount)
			{
				StartCoroutine(OnPopUp("win"));

			}
			else if (DiceGameManager.instance.playerCount < DiceGameManager.instance.cpuCount)
			{
				StartCoroutine(OnPopUp("lose"));

			}
			else
			{
				StartCoroutine(OnPopUp("draw"));
			}
            */
		}
	}
}
