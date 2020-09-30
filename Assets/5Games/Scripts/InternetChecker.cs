using System.Collections;
using UnityEngine;

public class InternetChecker : MonoBehaviour
{
    public static InternetChecker Instance;
    public float pingTime = 2f;
    public bool internetConnectBool;
    public GameObject NoInternetPopup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        Debug.Log("Internet: "+ gameObject.name);
    }
   
    public IEnumerator checkInternetConnection(System.Action<bool> action)
    {
        WWW www = new WWW("http://google.com");
        yield return www;
        if (www.error != null)
        {
            Debug.Log("Internet not Available!!");
            action(false);
        }
        else
        {
            Debug.Log("Internet Connected!!");
            action(true);
        }
    }
    
    public void DisplayInternetError()
    {
        Transform canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
        GameObject warning = Instantiate(NoInternetPopup, canvas) as GameObject;
    }
}