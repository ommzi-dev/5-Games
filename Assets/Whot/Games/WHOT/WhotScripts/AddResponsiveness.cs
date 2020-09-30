using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AddResponsiveness : MonoBehaviour {

	public GridLayoutGroup playerGrid,opponent1Grid /*, opponent2Grid, opponent3Grid*/;
	public RectTransform rt1/*, rt2, rt3*/;
	bool set = false;
	// Use this for initialization
	void Start () {
        //		var rt = gameObject.GetComponent<RectTransform> ();
        //Debug.Log("  content width is   "+rt.rect.height);
    }
	
	// Update is called once per frame
	void Update () {
		if ( !set &&rt1.rect.height > 0) {
			set = true;
			playerGrid.cellSize = new Vector2(rt1.rect.width,rt1.rect.height);
			opponent1Grid.cellSize = new Vector2(rt1.rect.width,rt1.rect.height);
			//opponent2Grid.cellSize = new Vector2(rt2.rect.width,rt2.rect.height);
			//opponent3Grid.cellSize = new Vector2(rt3.rect.width,rt3.rect.height);
		}
	}
}
