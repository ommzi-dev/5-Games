using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FetchBetHistory : MonoBehaviour
{
    public GameObject historyObj, historyParent, historyPanel;
    List<GameObject> history = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void fetchHistory()
    {
        StartCoroutine(getHistory());
    }
    public IEnumerator getHistory()
    {

        for(int i=0; i < history.Count; i++)
        {
            Destroy(history[i].gameObject);
        }
        history.Clear();
        string url = "http://18.191.157.16:4000/apis/getbethistory";
        WWWForm form = new WWWForm();
        form.AddField("game_id", "");
        UnityWebRequest www = UnityWebRequest.Post(url,form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        www.SetRequestHeader("Authorization", "Bearer " + UserDetailsManager.accessToken);
        www.timeout = 30;
        yield return www.SendWebRequest();

        if (www.error != null || www.isNetworkError)
        {
            Debug.Log("result " + www.error + "Time: " + Time.time);
        }
        else
        {
            historyPanel.SetActive(true);
            Debug.Log("User Stats Response: " + www.downloadHandler.text);
            var statsList = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            if (!www.downloadHandler.text.Contains("No record found"))
            {

                var userDetails = (IList)statsList["result"];

                for (int i = 0; i < userDetails.Count; i++)
                {
                    var inneruserDetails = (IDictionary)userDetails[i];
                    string betresult = "Won";
                    if (inneruserDetails["win"].ToString() == "0")
                    {
                        betresult = "Lost";
                    }

                    history.Add(Instantiate(historyObj, historyParent.transform));
                    history[i].transform.GetChild(0).GetComponent<Text>().text = "Bet Amount : " + inneruserDetails["bet_amount"].ToString() + "                " + betresult;
                    history[i].transform.GetChild(1).GetComponent<Text>().text = "Created : " + inneruserDetails["created"].ToString();
                }
            }
           
        }
    }
}
