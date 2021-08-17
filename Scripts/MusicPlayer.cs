using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] string[] sceneMusic;
    [SerializeField] AudioClip sampleSFX;


    private void Awake()
    {
        MusicPlayer[] singletonMusic = GameObject.FindObjectsOfType<MusicPlayer>();
        if (singletonMusic.Length > 1)
        {
            this.gameObject.SetActive(false);
            GameObject.Destroy(this.gameObject);
        }
        else
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        this.audioSource = this.gameObject.GetComponent<AudioSource>();
        this.audioSource.ignoreListenerVolume = true;
        //this.audioSource.ignoreListenerPause = true;

        this.audioSource.volume = PlayerPrefsController.GetMasterVolume();
        AudioListener.volume = PlayerPrefsController.GetMasterSFX();
    }

    private void Start()
    {
        this.PlaysceneMusic(0);
    }

    public void SetVolume(float inVolume)
    {
        this.audioSource.volume = inVolume;
        PlayerPrefsController.SetMasterVolume(inVolume);
    }

    public void SetSFX(float inVolume)
    {
        AudioListener.volume = inVolume;
        PlayerPrefsController.SetMasterSFX(inVolume);
        this.audioSource.PlayOneShot(this.sampleSFX, inVolume);
    }

    public void PlaysceneMusic(int sceneIndex)
    {
        this.audioSource.Stop();
        this.audioSource.clip = Resources.Load<AudioClip>("Music/" + this.sceneMusic[sceneIndex]);
        this.audioSource.Play();
    }
}
