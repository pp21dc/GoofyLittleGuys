using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;
    public void SetSfxVolume(float value)
    {
        mainMixer.SetFloat("sfxVolume", value);

    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("musicVolume", value);

    }
}
