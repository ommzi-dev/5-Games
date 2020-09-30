using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ListOfSounds{
	CheckUp = 0,//checkUp",
	Circle ,//circle",
	Continue ,//continue",
	Crosses ,//cross",
	DealingCard ,//dealing-card",
	Defend ,//defend",
	Draw ,//draw",
	GameLose ,//game lose",
	GameWin ,//game win",
	GeneralMarket ,//gen",
	HoldOn ,//holdOn",
	LastCard ,//lastCard",
	PickThree ,//pickThree",
	PickTwo ,//pickTwo",
	Square ,//square",
	Star ,//star",
	Suspension ,//suspension",
	Triangle ,//triangle",
	Warning ,//warning",

}

public class SoundManger : MonoBehaviour {
	public AudioSource source;
	public static SoundManger instance;

	List<string> soundList = new List<string>{

		"checkUp",//No idea
		"circle",
		"continue",// No idea
		"cross",
		"dealing-card",
		"defend",
		"draw",
		"game lose",
		"game win",
		"gen",
		"holdOn",
		 "lastCard",
		"pickThree",
		"pickTwo",
		"square",
		"star",
		"suspension",
		"triangle",
		"warning",// No idea
	};
	void Start()
	{
		instance = this;
        source = GameObject.Find("SoundManager").transform.GetChild(0).GetComponent<AudioSource>();
	}
	void OnDestroy()
	{
		instance = null;
	}
	public void PlaySound(int id)
	{
		
		AudioClip clip = Resources.Load ("Sounds/" + soundList[id]) as AudioClip;
		source.clip = clip;
		if (string.Equals( PlayerPrefs.GetString (WhotCommonConstants.SoundKey, WhotCommonConstants.soundOn),WhotCommonConstants.soundOn)) {

			source.Play ();
		}
	}
	public void PlaySound(string name)
	{

		AudioClip clip = Resources.Load ("Sounds/" + name) as AudioClip;
		source.clip = clip;
		if (string.Equals( PlayerPrefs.GetString (WhotCommonConstants.SoundKey, WhotCommonConstants.soundOn),WhotCommonConstants.soundOn)) {

			source.Play ();
		}
	}
	// Use this for initialization

}
