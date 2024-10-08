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

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        public void PlaySfx(AudioSource audioSource, AudioClip audioClip, float volume)
        {
            // Assign the volume
            audioSource.volume = volume;

            // Play the clip without interrupting other audio from this source
            audioSource.PlayOneShot(audioClip);
        }

        public void PlayMusic(AudioSource audioSource, AudioClip audioClip, float volume)
        {
            // Assign the clip
            audioSource.clip = audioClip;

            // Assign the volume
            audioSource.volume = volume;

            // Play the clip 
            audioSource.Play();
        }


    }
}

