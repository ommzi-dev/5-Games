using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("SFX References")]
    [SerializeField] AudioSource sfxAudioSource;
    public List<AudioClip> sfxClips;    //App Button Click-0, Button Click-1, Draw-2, Victory-3, gameover-4, popup-5    

    [Header("Music References")]
    [SerializeField] AudioSource musicAudioSource;
    public List<AudioClip> musicClips;    

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if(Instance != this)
        {
            Destroy(gameObject);
        }

        SetDefaultValues();
        
    }

    void SetDefaultValues()
    {
        if(!PlayerPrefs.HasKey("IsSoundOn"))
            PlayerPrefs.SetInt("IsSoundOn", 1);
        if (!PlayerPrefs.HasKey("IsMusicOn"))
            PlayerPrefs.SetInt("IsMusicOn", 1);
        if (!PlayerPrefs.HasKey("IsVibrationOn"))
            PlayerPrefs.SetInt("IsVibrationOn", 1);
        ResetSound();
        ResetMusic();
    }

    #region SoundSettings
    public void ResetSound()
    {
        Debug.Log("Reset Sound: "+ PlayerPrefs.GetInt("IsSoundOn"));
        if (sfxAudioSource == null)
            transform.GetChild(0).GetComponent<AudioSource>();
        sfxAudioSource.loop = false;
        sfxAudioSource.volume = (PlayerPrefs.GetInt("IsSoundOn") == 1)? 1f: 0f;
    }

    public void PlaySound(int clipNum)
    {
        if(clipNum < sfxClips.Count)
            sfxAudioSource.PlayOneShot(sfxClips[clipNum]);
    }
    #endregion

    #region MusicSettings
    public void ResetMusic()
    {
        Debug.Log("Reset Music: "+ PlayerPrefs.GetInt("IsMusicOn"));
        if (musicAudioSource == null)
            transform.GetChild(1).GetComponent<AudioSource>();
        musicAudioSource.playOnAwake = true;
        musicAudioSource.loop = true;
        musicAudioSource.volume = (PlayerPrefs.GetInt("IsMusicOn") == 1) ? 1f : 0f;
        PlayMusic(0);
    }

    public void PlayMusic(int clipNum)
    {
        if (clipNum < musicClips.Count)
        {
            musicAudioSource.clip = musicClips[clipNum];
            musicAudioSource.Play();
        }
    }

    private void OnDisable()
    {
        //PlayerPrefs.SetInt("LoggedIn", 0);
        PlayerPrefs.DeleteKey("LoggedIn");
        PlayerPrefs.DeleteKey("pin");
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("LoggedIn");
        PlayerPrefs.DeleteKey("pin");
    }
    #endregion
}
