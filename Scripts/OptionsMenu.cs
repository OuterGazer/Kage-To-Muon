using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider SFXSlider;

    private MusicPlayer musicPlayer;

    // Start is called before the first frame update
    void Start()
    {
        this.musicPlayer = GameObject.FindObjectOfType<MusicPlayer>();

        this.volumeSlider.value = PlayerPrefsController.GetMasterVolume();
        this.SFXSlider.value = PlayerPrefsController.GetMasterSFX();
    }

    public void OnVolumeChange()
    {
        float inVolume = this.volumeSlider.value;

        this.musicPlayer.SetVolume(inVolume);
    }

    public void OnSFXChange()
    {
        float inVolume = this.SFXSlider.value;

        this.musicPlayer.SetSFX(inVolume);
    }
}
