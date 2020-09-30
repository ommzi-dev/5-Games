using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class CardTumb : MonoBehaviour {

	public Text cardName;
	public DOTweenAnimation tweens;
    
    public GameObject card;
	public GameObject highlight;
	public bool checkVisibility;
	public Camera camera;
	Material cardMaterial;
    // Use this for initialization
	void Start()
	{
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}



    public void DestroyElement()
	{
        Debug.Log("Destroying "+gameObject.name);
		Destroy (gameObject);
	}

	public void PlayTween(int type)
	{
		if (type == 1)
			tweens.DOPlayById ("1");
		else if (type == 2)
			tweens.DOPlayById ("2");
		else
			tweens.DOPlayById ("3");
		
//			GetComponents<DOTweenAnimation> () [0].DOPlayForward ();
//		else
//			GetComponents<DOTweenAnimation> () [1].DOPlayForward ();
	}

	public void PlayHintAnim()
	{
		highlight.SetActive (true);
//		tweens.DOPlayById ("2");
	}

	public void StopHintAnim()
	{
		highlight.SetActive (false);
//		tweens.DOComplete ();
	}

	public void OnPlayTween()
	{
	}

	public void SetCardName(string name,bool show = true)
	{
		cardMaterial = card.GetComponent<Renderer> ().material;
		cardName.text = name;
		ShowImageOnCard (name,show);
	}

	void ShowImageOnCard(string cardName,bool show)
	{
		string colorKey = cardName.Substring (0, 3);
		string cardNum = cardName.Substring (3, cardName.Length - 3);
		string folderName = WhotConstants.GetColorFolder (colorKey);
        cardMaterial.mainTexture = Resources.Load<Texture>("cards/" + folderName + "/" + cardNum);
        
        if (show)
            card.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        else
            card.transform.localRotation = Quaternion.Euler(0, 0f, 0);
    }
}
