using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioSource music1Source;
    [SerializeField] private AudioSource music2Source;

    private AudioSource activeMusic;
    private AudioSource inactiveMusic;

    [SerializeField] private float crossfadeRate = 1.50f;
    private bool isCrossfading;

    [SerializeField] private string introBGMusic;
    [SerializeField] private string levelBGMusic;

    //public ManagerStatus status { get; private set; }

    //private NetworkService network;

    public float soundVolume
    {
        get { return AudioListener.volume; }
        set { AudioListener.volume = value; }
    }
    public bool soundMute
    {
        get { return AudioListener.pause; }
        set { AudioListener.pause = value; }
    }

    private float musicVolume;
    public float MusicVolume
    {
        get { return this.musicVolume; }
        //adjust the volume directly through the audio source instead of the audio listener, so music and SFX are separated
        set { this.musicVolume = value;
            if (this.music1Source != null && !this.isCrossfading)
            {
                this.music1Source.volume = this.musicVolume;
                this.music2Source.volume = this.musicVolume;
            }
        }
    }
    public bool MusicMute
    {
        get { if(this.music1Source != null)
                return this.music1Source.mute;
               
              return false; }

        set { if (this.music1Source != null)
              {
                this.music1Source.mute = value;
                this.music2Source.mute = value;
              }
        }
    }

    /*private void Awake()
    {
        int audioManagerAmount = GameObject.FindObjectsOfType<AudioManager>().Length;

        if (audioManagerAmount > 1)
        {
            this.gameObject.SetActive(false);
            GameObject.Destroy(this.gameObject);
        }
        else
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
    }*/

    public void Start()//NetworkService network)
    {
        //Debug.Log("Audio manager starting...");

        //this.network = network;

        this.music1Source.ignoreListenerVolume = true;
        this.music1Source.ignoreListenerPause = true;
        this.music2Source.ignoreListenerVolume = true;
        this.music2Source.ignoreListenerPause = true;

        this.soundVolume = 1.0f;
        this.musicVolume = 1.0f;

        this.activeMusic = this.music1Source;
        this.inactiveMusic = this.music2Source;

        //this.status = ManagerStatus.Started;
    }

    public void PlayIntroMusic()
    {
        this.PlayMusic(Resources.Load<AudioClip>("Music/" + this.introBGMusic));
    }

    public void PlayLevelMusic()
    {
        this.PlayMusic(Resources.Load<AudioClip>("Music/" + this.levelBGMusic));
    }

    public void StopMusic()
    {
        this.activeMusic.Stop();
        this.inactiveMusic.Stop();
    }

    public void PlaySound(AudioClip audioClip)
    {
        this.soundSource.PlayOneShot(audioClip);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (this.isCrossfading) { return; }

        this.StartCoroutine(this.CrossfadeMusic(clip));
        
    }

    private IEnumerator CrossfadeMusic(AudioClip clip)
    {
        this.isCrossfading = true;

        this.inactiveMusic.clip = clip;
        this.inactiveMusic.volume = 0;
        this.inactiveMusic.Play();

        float scaledRate = this.crossfadeRate * this.musicVolume;

        while(this.activeMusic.volume > 0)
        {
            this.activeMusic.volume -= scaledRate * Time.deltaTime;
            this.inactiveMusic.volume += scaledRate * Time.deltaTime;

            yield return null; //pauses for one frame
        }

        AudioSource temp = this.activeMusic;

        this.activeMusic = this.inactiveMusic;
        this.activeMusic.volume = this.musicVolume;

        this.inactiveMusic = temp;
        this.inactiveMusic.Stop();

        this.isCrossfading = false;
    }
}
