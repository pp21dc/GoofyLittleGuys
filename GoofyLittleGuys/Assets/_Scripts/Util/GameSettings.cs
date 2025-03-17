using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class GameSettings
{
	public float masterVolume = 1.0f;
	public float musicVolume = 0.8f;
	public float sfxVolume = 0.8f;
	public float rumbleAmount = 1.0f;
	public int resolutionIndex = 0;
	public bool isFullscreen = true;

	// Add any other settings you need  
}
