using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartScriptController : MonoBehaviour
{
    public GameObject splashCanvas;
    public GameObject LoginCanvas;

    public GameObject internetConnectionPopUp;

    void Start()
    {

        StartCoroutine(checkConnection());
    }

    IEnumerator checkConnection()
    {
        yield return new WaitForSeconds(0.1f);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            internetConnectionPopUp.SetActive(true);
            StartCoroutine(checkConnection());
        }
        else
        {
            internetConnectionPopUp.SetActive(false);
            allOkay();
        }
    }

    void allOkay()
    {
        splashCanvas.SetActive(true);
        StartCoroutine(gotologin());
    }

    IEnumerator gotologin()
    {
        yield return new WaitForSeconds(2f);
        splashCanvas.SetActive(false);
        LoginCanvas.SetActive(true);
    }

    public void retry()
    {
        SceneManager.LoadScene("LoginSplash");
    }

}
