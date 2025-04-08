using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextToSpeechObject : MonoBehaviour
{
    public AudioClip clip;

    public void Read(/*AudioClip clip*/)
    {
        Debug.Log("OnPointerEnter");
        //TextToSpeech.Instance.TTS(clip);
    }
}
