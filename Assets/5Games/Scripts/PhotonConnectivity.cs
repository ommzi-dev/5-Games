using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonConnectivity : MonoBehaviour
{
    private void OnApplicationFocus(bool focus)
    {
        if (focus && !PhotonNetwork.connected)
            GetPhotonToken();
        
    }

    public void GetPhotonToken()
    {
        //Debug.Log("Get Photon Token Called!!");
        Application.runInBackground = true;
        if (!PhotonNetwork.connected)
        {
            Debug.Log("Connecting Photon!");
            PhotonNetwork.networkingPeer.DisconnectTimeout = 180000;
            PhotonNetwork.ConnectUsingSettings("1.0");

        }               
    }
}
