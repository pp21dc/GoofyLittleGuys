using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextToSpeech : SingletonBase<TextToSpeech>
{
    [SerializeField] private AudioSource ttsSource;

    [SerializeField] private List<AudioClip> playerNumbers;

    private bool ttsOn = false;
    public Queue<AudioClip> comboQueue;
    GameObject currentObj;
    TextToSpeechObject ttsObj;
    private void Update()
    {
        if (ttsOn)
        {
            if (currentObj != EventSystem.current.currentSelectedGameObject)
            {
                currentObj = EventSystem.current.currentSelectedGameObject;

                if (currentObj.GetComponent<TextToSpeechObject>() != null)
                    ttsObj = currentObj.GetComponent<TextToSpeechObject>();

                if (ttsObj != null)
                {
                    TTS(ttsObj.clip);
                }
            }
        }
        
    }
    private void TTS(AudioClip clip)
    {
        ttsSource.Stop();
        ttsSource.PlayOneShot(clip);
        Debug.Log("TTS Activate");
    }

    public void toggleTTS()
    {
        switch (ttsOn)
        {
            case true:
                ttsOn = false;
                break;
            case false:
                ttsOn = true;
                break;
        }
    }
    public void PlayComboQueue()
    {
        StartCoroutine(PlayComboQueueCoroutine());
    }

    IEnumerator PlayComboQueueCoroutine()
    {
        AudioClip clip;
        while (comboQueue.Count > 0)
        {
            if (!ttsSource.isPlaying)
            {
                clip = comboQueue.Dequeue();
                ttsSource.PlayOneShot(clip);
            }
            yield return null;
        }
    }
}
