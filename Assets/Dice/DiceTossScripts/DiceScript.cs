﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceScript : MonoBehaviour {

	static Rigidbody rb;
	public static Vector3 diceVelocity;
	public bool IsPress;
	public bool isSecondTry;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		IsPress = false;
	}
	
	// Update is called once per frame
	void Update () {
		diceVelocity = rb.velocity;

		if (IsPress)
        {
			isSecondTry = true;
			DiceNumberTextScript.diceNumber = 0;
			float dirX = Random.Range (0, 500);
			float dirY = Random.Range (0, 500);
			float dirZ = Random.Range (0, 500);
			//transform.position = new Vector3 (transform.position.x, transform.position.y,transform.position.z);
			transform.position = new Vector3(0, 10, -12);

			transform.rotation = Quaternion.identity;
			rb.AddForce (transform.up * 1000);
			rb.AddTorque (dirX, dirY, dirZ);
			IsPress = false;
		}
	}
}
