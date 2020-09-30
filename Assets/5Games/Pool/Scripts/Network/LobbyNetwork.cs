using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class LobbyNetwork : UnityEngine.MonoBehaviour
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("1.0");
    }

    private void OnConnectedToMaster()
    {
        print("connected to master");
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.playerName = PlayerNetwork.Instance.playerName;
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    private void OnJoinedLobby()
    {
        print("joined lobby");
        if (!PhotonNetwork.inRoom)
        {
            MainCanvasManager.Instance.LobbyCanvas.transform.SetAsLastSibling();
        }
    }
}
