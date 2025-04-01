using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAudioSource : MonoBehaviour
{
    private bool isPlaying = false;
    [SerializeField] private AudioSource waterAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        if (!isPlaying)
        {
            Managers.AudioManager.Instance.PlaySfx("Water", waterAudioSource);
            isPlaying = true;
        }
    }
}
