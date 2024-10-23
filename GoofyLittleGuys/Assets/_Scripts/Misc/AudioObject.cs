using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="AudioObject",menuName ="AudioObject")]
public class AudioObject : ScriptableObject
{
    public AudioClip objAudioClip;
    public string key;
    public float volume;
    public float pitch;
}
