using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Written By: Bryan Bedard
// Edited by: Bryan Bedard, 
// Purpose: To manage playing Audio (both Music and SFX)
namespace Managers
{
    public class AudioManager : SingletonBase<AudioManager>
    {
        public static AudioManager instance;
        [SerializeField] private AudioMixer mainMixer;

        public void PlaySfx(AudioSource audioSource, AudioClip audioClip, float volume)
        {
            audioSource.volume = volume;
            audioSource.PlayOneShot(audioClip);
        }

        public void PlayMusic(AudioSource audioSource, AudioClip audioClip, float volume)
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
        }

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

