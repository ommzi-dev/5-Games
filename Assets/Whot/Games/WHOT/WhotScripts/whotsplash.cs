using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class WhotSplash : MonoBehaviour
{
    public float SplashActiveTime = 3.0f;
    private CanvasGroup _group;
    public GameObject HomeMenu, HomeScreen, ExitFromGameplayObject, MenuCanvas;
    public GameObject ExitBtn, ShareBtn;
    IEnumerator Start()
    {
        _group = UIUtilities.GetCanvasGroup(gameObject);

        yield return new WaitForSeconds(SplashActiveTime);

#if UNITY_WEBGL
		ShareBtn.SetActive (false);
		ExitBtn.SetActive (false);
#endif
        _group.DOFade(0, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            /*	if(AlertController.instance.isChallengeReceived||AlertController.instance.isChallengeSent)
                {
                    HomeScreen.SetActive(false);
                    gameObject.SetActive(false);
                    WHOTMultiplayerManager.wagerAmount = 1;
                    WHOTMultiplayerManager.playingWagered = false;
                //	WHOTMultiplayerManager.instance.Connect();
                    ExitFromGameplayObject.SetActive(true);
                }
                else
                {*/

            if (PlayerPrefs.GetInt("WHOTRestartOnline") == 1)
            {
                PlayerPrefs.SetInt("WHOTRestartOnline", 0);
                PlayerPrefs.SetInt("WHOTRestart", 0);

                HomeScreen.SetActive(false);
                gameObject.SetActive(false);
                WHOTMultiplayerManager.wagerAmount = 1;
                WHOTMultiplayerManager.playingWagered = false;
                //WHOTLultiplayerManager.instance.Connect();
                ExitFromGameplayObject.SetActive(true);
                return;
            }
            else if (PlayerPrefs.GetInt("WHOTRestart") == 1)
            {
                PlayerPrefs.SetInt("WHOTRestartOnline", 0);
                PlayerPrefs.SetInt("WHOTRestart", 0);
                HomeScreen.SetActive(false);
                gameObject.SetActive(false);
                WhotManager.instance.OnStart();
                return;
            }
            else
            {
                HomeMenu.SetActive(true);
                gameObject.SetActive(false);
            }
            //}
        });
    }
}
