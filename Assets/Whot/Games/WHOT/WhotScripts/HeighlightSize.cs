using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeighlightSize : MonoBehaviour {
	public GridLayoutGroup gl;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void OnEnable () {
		GetComponent<RectTransform> ().sizeDelta = gl.cellSize+new Vector2(10,10);
		
	}
}
