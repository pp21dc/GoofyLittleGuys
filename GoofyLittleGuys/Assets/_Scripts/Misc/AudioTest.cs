using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioTest : MonoBehaviour
{
    [SerializeField] Button sfxButton;
    [SerializeField] Button musicButton;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider musicSlider;
    private AudioSource sfxSource;
    private AudioClip sfxClip;
    private AudioSource musicSource;
    private AudioClip musicClip;
    void Start()
    {
        sfxButton.onClick.AddListener(PlaySound);
        sfxSource = sfxButton.GetComponent<AudioSource>();
        sfxClip = sfxSource.clip;
        musicButton.onClick.AddListener(PlayMusic);
        musicSource = GameObject.Find("MusicSource").GetComponent<AudioSource>();
        musicClip = musicSource.clip;

        sfxSlider.onValueChanged.AddListener(delegate { changeSfxVol(); });
        musicSlider.onValueChanged.AddListener(delegate { changeMusicVol(); });
    }

    private void changeSfxVol()
    {
        Managers.AudioManager.Instance.SetSfxVolume(sfxSlider.value);
    }
    private void changeMusicVol()
    {
        Managers.AudioManager.Instance.SetMusicVolume(musicSlider.value);
    }

    private void PlaySound()
    {
        Managers.AudioManager.Instance.PlaySfx("TestSfx",sfxSource);
    }

    private void PlayMusic()
    {
        Managers.AudioManager.Instance.PlayMusic("TestMusic",musicSource);
    }
}
