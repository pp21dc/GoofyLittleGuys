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
		[Header("Mixer")]
		[SerializeField] private AudioMixer mainMixer;
		[Header("Audio Objects")]
		[SerializeField] private AudioObject[] sfxObjects;
		[SerializeField] private AudioObject[] musicObjects;

		private Dictionary<string, AudioObject> sfxDictionary = new Dictionary<string, AudioObject>();
		private Dictionary<string, AudioObject> musicDictionary = new Dictionary<string, AudioObject>();

		private void Start()
		{
			//sound effects
			foreach (AudioObject clip in sfxObjects)
			{
				if (sfxDictionary.ContainsKey(clip.key))
				{
					Debug.LogError($"{clip.key} already exists in SFX dictionary. Detected for {clip.name}. Changing key now");
					clip.key += $"NAME_ERROR";
				}
				sfxDictionary.Add(clip.key, clip);
			}
			
			//music
			foreach (AudioObject clip in musicObjects)
			{
				if (musicDictionary.ContainsKey(clip.key))
				{
					Debug.LogError($"{clip.key} already exists in SFX dictionary. Detected for {clip.name}. Changing key now");
					clip.key += $"NAME_ERROR";
				}
				musicDictionary.Add(clip.key, clip);
			}
		}

		public void PlaySfx(string key, AudioSource source)
		{
			if (sfxDictionary[key] != null)
			{
				// Sound source setup
				source.clip = getRandomClip(sfxDictionary[key]);
				source.volume = sfxDictionary[key].volume;
				source.pitch = Random.Range(sfxDictionary[key].pitch.x, sfxDictionary[key].pitch.y);
				source.spatialBlend = sfxDictionary[key].isSpatial ? 1.0f : 0.0f;

				// Adjust volume and panning based on player proximity
				AdjustAudioForProximity(source);

				source.Play();
			}
		}

		public void PlayMusic(string key, AudioSource source)
		{
			AudioObject objToPlay;
			if (musicDictionary[key] != null)
			{
				objToPlay = musicDictionary[key];
				// Set the basic properties (volume, pitch)
				source.volume = objToPlay.volume;
				source.pitch = Random.Range(objToPlay.pitch.x, objToPlay.pitch.y);

				source.clip = getRandomClip(objToPlay);
				source.Play();
			}
		}

		/// <summary>
		/// Method that adjusts volume of audio source based on proximity to any player and pans the audio based on what side of the screen the player's screen is on.
		/// This is done so that one AudioListener can be used (Unity only likes 1 audio listener in a scene at a time, no more or less) but sounds are still played
		/// relative to player positions in game and on the screen.
		/// </summary>
		/// <param name="audioSource">The audio source we're adjusting.</param>
		private void AdjustAudioForProximity(AudioSource audioSource)
		{
			float minDistance = 1f; // Minimum hearing distance for full volume
			float maxDistance = 20f; // Maximum distance to hear sound (volume reaches 0)

			// Find the nearest player (could be multiple players in a multiplayer game)
			float closestDistance = Mathf.Infinity;
			PlayerBody closestPlayer = null;
			foreach (var player in GameManager.Instance.Players)
			{
				float distance = Vector3.Distance(player.transform.position, audioSource.transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestPlayer = player;
				}
			}

			// Adjust volume based on distance
			float volume = Mathf.Clamp01(1 - (closestDistance - minDistance) / (maxDistance - minDistance));
			audioSource.volume = volume;


			//Adjust panning based on relative player cam position and position of the sound relative to that player's viewport.

			// Get the closest player's camera rect (adjusted based on screen position)
			Rect cameraRect = closestPlayer.Controller.PlayerCam.rect;

			// Normalize the sound's position relative to the player's viewport (0 to 1)
			Vector3 soundScreenPos = closestPlayer.Controller.PlayerCam.WorldToScreenPoint(audioSource.transform.position);
			float soundScreenX = soundScreenPos.x / Screen.width;   // Sound's position in the screen space (0 = left, 1 = right)

			float pan = 0f;

			if (cameraRect.x < 0.5f)  // Camera is on the left side of the screen
			{
				// Map sound position in the player's viewport to pan range -1 to 0
				pan = Mathf.Lerp(-1f, 0f, soundScreenX);  // soundScreenX is from 0 to 1, so this maps it correctly
			}
			else if (cameraRect.x >= 0.5f)  // Camera is on the right side of the screen
			{
				// Map sound position in the player's viewport to pan range 0 to 1
				pan = Mathf.Lerp(0f, 1f, soundScreenX);  // soundScreenX is from 0 to 1, so this maps it correctly
			}

			// Apply the calculated pan to the audio source
			audioSource.panStereo = pan;

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
		/// Picks a random clip from the array of clips held by a given audio object
		/// </summary>
		/// <param name="audioObject"></param>
		/// <returns></returns>
		private AudioClip getRandomClip(AudioObject audioObject)
		{
			int index = Random.Range(0, audioObject.clips.Length);
			return audioObject.clips[index];
		}
	}
}
