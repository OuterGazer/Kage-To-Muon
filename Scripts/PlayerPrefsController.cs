using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsController : MonoBehaviour
{
    //Master Keys goes here
    const string MASTER_VOLUME_KEY = "master volume";
    const string MASTER_SFX_KEY = "master SFX";

    //Other constants goe here
    const float MIN_VOLUME = 0.0f;
    const float MAX_VOLUME = 1.0f;

    public static void SetMasterVolume(float inVolume)
    {
        if(inVolume <= MAX_VOLUME &&
           inVolume >= MIN_VOLUME)
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, inVolume);
        }
        else
        {
            Debug.LogError("The chosen master volume is out of range.");
        }
    }

    public static void SetMasterSFX(float inVolume)
    {
        if (inVolume <= MAX_VOLUME &&
           inVolume >= MIN_VOLUME)
        {
            PlayerPrefs.SetFloat(MASTER_SFX_KEY, inVolume);
        }
        else
        {
            Debug.LogError("The chosen master SFX is out of range.");
        }
    }

    public static float GetMasterVolume(float inDefault = 0.60f)
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, inDefault);
    }

    public static float GetMasterSFX(float inDefault = 0.50f)
    {
        return PlayerPrefs.GetFloat(MASTER_SFX_KEY, inDefault);
    }
}
