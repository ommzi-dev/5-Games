using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WhotMenu : MonoBehaviour
{

    // Use this for initialization
    public static GameObject go;
    public GameObject mainMenuPanel;
    public GameObject rulesPanel;
    public GameObject selectionModePanel;
    public GameObject MultiplayerModePanel;
    public GameObject userprofile;
    void Start()
    {
        mainMenuPanel.SetActive(true);
        WhotConstants.isAI = true;
        go = gameObject;
        //WhotCommonConstants.AssignTexture(userprofile.GetComponent<RawImage>(),AlertController.instance.profileimg);
        //Screen.orientation = ScreenOrientation.Landscape;
    }

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mainMenuPanel.SetActive(true);
            rulesPanel.SetActive(false);
            selectionModePanel.SetActive(false);
            MultiplayerModePanel.SetActive(false);

            /*if (rulesPanel.activeSelf)
            {
                mainMenuPanel.SetActive(true);
                rulesPanel.SetActive(false);
            }
            else if (selectionModePanel.activeSelf)
            {
                mainMenuPanel.SetActive(true);
                selectionModePanel.SetActive(false);
            }
            else if (MultiplayerModePanel.activeSelf)
            {
                mainMenuPanel.SetActive(true);
                MultiplayerModePanel.SetActive(false);
            }*/
        }
    }

    public void OnExitClick()
    {
        SceneManager.LoadScene("LoginSplash");     
    }
}
