using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class GameSettings
{
	// AUDIO
	public float masterVolume = 1.0f;
	public float musicVolume = 0.8f;
	public float sfxVolume = 0.8f;
	public float rumbleAmount = 1.0f;

	// DISPLAY
	public int resolutionIndex = 14;
	public int windowModeIndex = 0; 
	public int frameCap = -1; // -1 = uncapped
	public float contrast = 0f;         // Default unity contrast range is [-100, 100]
	public float brightness = 0f;      // Post Exposure in Unity is generally in EV ([-5, 5] is a nice safe range)


	// Add any other settings you need  
}
