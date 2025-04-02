using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextToSpeechObject : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    public void Read(AudioClip clip)
    {
        //TextToSpeech.Instance.TTS(clip);
    }
}
