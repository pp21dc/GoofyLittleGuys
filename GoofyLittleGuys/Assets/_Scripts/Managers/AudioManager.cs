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
            addNewSfxObject("TestSfxObj",testSfxObject);
            addNewMusicObject("TestMusicObj", testMusicObject);
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

        /// <summary>
        /// Takes a name/key for the new SFX object to add to the list, as well as the object itself. Then adds that 
        /// SFX object to the list of SFX objects.
        /// </summary>
        /// <param name="key"></param> -> What to call the new object
        /// <param name="newSfxObject"></param> -> The new object
        public void addNewSfxObject(string key, AudioObject newSfxObject)
        {
            SfxObjects.Add(key,newSfxObject);
        }

        /// <summary>
        /// Takes a name/key for the new music object to add to the list, as well as the object itself. Then adds that 
        /// music object to the list of music objects.
        /// </summary>
        /// <param name="key"></param> -> What to call the new object
        /// <param name="newMusicObject"></param> -> The new object
        public void addNewMusicObject(string key, AudioObject newMusicObject)
        {
            MusicObjects.Add(key, newMusicObject);
        }

    }
}

