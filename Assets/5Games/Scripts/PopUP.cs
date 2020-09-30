using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUP : MonoBehaviour {
    public Text msg, title;
	// Use this for initialization
	void Start () {
        SoundManager.Instance.PlaySound(5);
	}
}
