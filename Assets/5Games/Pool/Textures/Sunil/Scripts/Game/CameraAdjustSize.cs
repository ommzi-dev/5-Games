using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraAdjustSize : MonoBehaviour
{

    private float widthToBeSeen = 14.2f;

    void Start()
    {

        if ((float)Screen.width / (float)Screen.height > 16.0f / 9.0f)
        {
            Debug.Log("Not 16:9");
            Camera.main.orthographicSize = 4;//widthToBeSeen * Screen.height / Screen.width * 0.6f;
            CanvasScaler scaler = GameObject.Find("UICanvas").GetComponent<CanvasScaler>();
            scaler.matchWidthOrHeight = 1.0f;
            CanvasScaler scaler2 = GameObject.Find("UICanvas2").GetComponent<CanvasScaler>();
            scaler2.matchWidthOrHeight = 1.0f;
        }
        // else if ((float)Screen.width / (float)Screen.height < 16.0f / 9.0f)
        // {

        // }
        else
        {
            Debug.Log("16:9");
            Camera.main.orthographicSize = widthToBeSeen * Screen.height / Screen.width * 0.5f;
        }

        QualitySettings.SetQualityLevel(5, true);
    }

}
