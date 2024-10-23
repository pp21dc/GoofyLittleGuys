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
        
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioObject testSfxObject;
        [SerializeField] private AudioObject testMusicObject;

        private Dictionary<string,AudioObject> SfxObjects = new Dictionary<string,AudioObject>();
        private Dictionary<string, AudioObject> MusicObjects = new Dictionary<string, AudioObject>();


        private void Start()
        {
            addNewSfxObject(testSfxObject);
            addNewMusicObject(testMusicObject);
        }
        public void PlaySfx(string key, AudioSource audioSource, AudioClip audioClip, float volume)
        {
            AudioObject objToPlay;
            if (SfxObjects[key] != null)
            {
                objToPlay = SfxObjects[key];
                audioSource.volume = objToPlay.volume;
                audioSource.pitch = objToPlay.pitch;
                audioSource.PlayOneShot(objToPlay.objAudioClip);
            }
        }

        public void PlayMusic(string key, AudioSource audioSource, AudioClip audioClip, float volume)
        {
            AudioObject objToPlay;
            if (MusicObjects[key] != null)
            {
                objToPlay = MusicObjects[key];
                audioSource.volume = objToPlay.volume;
                audioSource.pitch = objToPlay.pitch;
                audioSource.clip = objToPlay.objAudioClip;
                audioSource.Play();
            }
        }

        public void SetSfxVolume(float value)
        {
            mainMixer.SetFloat("sfxVolume", value);

        }

        public void SetMusicVolume(float value)
        {
            mainMixer.SetFloat("musicVolume", value);

        }

        public void addNewSfxObject(AudioObject newSfxObject)
        {
            SfxObjects.Add("TestSfx",newSfxObject);
        }
        public void addNewMusicObject(AudioObject newMusicObject)
        {
            MusicObjects.Add("TestMusic", newMusicObject);
        }

    }
}

