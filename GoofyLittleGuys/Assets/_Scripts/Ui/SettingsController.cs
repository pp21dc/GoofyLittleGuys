using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SettingsController : MonoBehaviour
{
    [SerializeField] private AudioMixer MasterMixer;


    // Find a way to make these generic
    public void ModifyMasterVolume(float value)
    {
        MasterMixer.SetFloat("masterVolume", value);
    }
    public void ModifySfxVolume(float value)
    {
        MasterMixer.SetFloat("sfxVolume", value);
    }
    public void ModifyMusicVolume(float value)
    {
        MasterMixer.SetFloat("musicVolume", value);
    }
}
