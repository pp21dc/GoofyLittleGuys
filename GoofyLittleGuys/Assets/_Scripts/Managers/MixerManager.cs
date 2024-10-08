using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Written By: Bryan Bedard
// Edited by: Bryan Bedard, 
// Purpose: To manage audio volumes for Music, SFX and both simultaneously (AKA Master)
namespace Managers
{
    public class MixerManager : SingletonBase<MixerManager>
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

}
