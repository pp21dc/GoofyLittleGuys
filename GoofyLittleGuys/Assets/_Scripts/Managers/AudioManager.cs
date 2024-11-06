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
        [SerializeField] private AudioObject[] SfxObjects;
        [SerializeField] private AudioObject[] MusicObjects;

        private Dictionary<string,AudioObject> SfxDictionary = new Dictionary<string,AudioObject>();
        private Dictionary<string, AudioObject> MusicDictionary = new Dictionary<string, AudioObject>();


        private void Start()
        {
            for(int i = 0; i < SfxObjects.Length; i++)
            {
                addNewSfxObject(SfxObjects[i].key, SfxObjects[i]);
            }
            for (int i = 0; i < MusicObjects.Length; i++)
            {
                addNewMusicObject(MusicObjects[i].key, MusicObjects[i]);
            }
        }
        public void PlaySfx(string key, AudioSource audioSource)
        {
            AudioObject objToPlay;
            if (SfxDictionary[key] != null)
            {
                objToPlay = SfxDictionary[key];
                audioSource.volume = objToPlay.volume;
                audioSource.pitch = objToPlay.pitch;
                audioSource.PlayOneShot(objToPlay.objAudioClip);
            }
        }

        public void PlayMusic(string key, AudioSource audioSource)
        {
            AudioObject objToPlay;
            if (MusicDictionary[key] != null)
            {
                objToPlay = MusicDictionary[key];
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

        /// <summary>
        /// Takes a name/key for the new SFX object to add to the list, as well as the object itself. Then adds that 
        /// SFX object to the list of SFX objects.
        /// </summary>
        /// <param name="key"></param> -> What to call the new object
        /// <param name="newSfxObject"></param> -> The new object
        public void addNewSfxObject(string key, AudioObject newSfxObject)
        {
            SfxDictionary.Add(key,newSfxObject);
        }

        /// <summary>
        /// Takes a name/key for the new music object to add to the list, as well as the object itself. Then adds that 
        /// music object to the list of music objects.
        /// </summary>
        /// <param name="key"></param> -> What to call the new object
        /// <param name="newMusicObject"></param> -> The new object
        public void addNewMusicObject(string key, AudioObject newMusicObject)
        {
            MusicDictionary.Add(key, newMusicObject);
        }

    }
}

