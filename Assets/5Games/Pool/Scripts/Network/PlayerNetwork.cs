using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour
{
    public static PlayerNetwork Instance;
    public string playerName;
    void Awake()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        Instance = this;
    }

    private void Start()
    {
        playerName = UserDetailsManager.userName;
    }
}
