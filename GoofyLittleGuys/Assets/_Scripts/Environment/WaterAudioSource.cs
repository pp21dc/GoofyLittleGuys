using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAudioSource : MonoBehaviour
{
    private bool isPlaying = false;
    [SerializeField] private AudioSource waterAudioSource;
    //[SerializeField] private BoxCollider quietZone; //Larger trigger that will play the sound at half volume
    [SerializeField] private BoxCollider loudZone; //Smaller trigger that plays sound at full volume

    private int numPlayersNear = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //if it wasn't a player that entered, do nothing.
        var playerHit = other.gameObject.GetComponent<LilGuyBase>()?.PlayerOwner;
        if (playerHit == null) return;

        //if a player entered, increment numPlayersNear.
        numPlayersNear++;

        if (!isPlaying && numPlayersNear == 1)
        {
            isPlaying = true;
            PlayLoopingSound("Water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if it wasn't a player that left, do nothing.
        var playerHit = other.gameObject.GetComponent<LilGuyBase>()?.PlayerOwner;
        if (playerHit == null) return;

        //if a player left the trigger, decrease numPlayersNear, setting it to at least zero.
        numPlayersNear--;
        if (numPlayersNear < 0) { numPlayersNear = 0; }

        //if no players are nearby, stop playing the sound and update isPlaying to false.
        if (isPlaying && numPlayersNear <= 0) 
        {
            StopLoopingSound();
            isPlaying = false;
        }
    }

    //Changes this source to loop and plays a sound until StopLoopingSound is called
    private void PlayLoopingSound(string key)
    {
        if (waterAudioSource.loop == false)
        {
            waterAudioSource.loop = true;
        }
        Managers.AudioManager.Instance.PlaySfx(key, waterAudioSource);
    }

    //Changes this source to not loop and stops whatever it is currently playing
    private void StopLoopingSound()
    {
        if (waterAudioSource.loop == true && waterAudioSource.isPlaying)
        {
            waterAudioSource.loop = false;
            waterAudioSource.Stop();
        }
    }
}
