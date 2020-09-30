using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    [SerializeField] 
    private Text _roomName;
    private Text RoomName
    {
        get
        {
            return _roomName;
        }
    }

    public void OnClick_CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 2
        };
        if(PhotonNetwork.CreateRoom(RoomName.text, roomOptions, TypedLobby.Default))
        {
            print("create room send success");
        }
        else
        {
            print("create room send failed");
        }
    }

    private void OnPhotonCreateRoomFailed(object[] codeAndMessage)
    {
        print("create room failed " + codeAndMessage);
    }

    private void OnCreatedRoom()
    {
        print("room created success");
    }
    
}
