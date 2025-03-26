using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine;
using Managers;
using UnityEngine.Audio;
public class SettingsManager
{
	private static SettingsManager instance;
	private GameSettings settings;
	private readonly string settingsFilePath = Path.Combine(Application.persistentDataPath, "gameSettings.dat");

	public static SettingsManager Instance => instance ?? (instance = new SettingsManager());

	private SettingsManager()
	{
		LoadSettings();  // Automatically load settings when created
	}

	public GameSettings GetSettings()
	{
		return settings;
	}

	public void SaveSettings()
	{
		try
		{
			using (FileStream file = File.Create(settingsFilePath))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(file, settings);
			}
		}
		catch (Exception e)
		{
			DebugManager.Log($"Failed to save settings: {e.Message}", DebugManager.DebugCategory.GENERAL, DebugManager.LogLevel.ERROR);
		}
	}

	public void LoadSettings()
	{
		if (File.Exists(settingsFilePath))
		{
			try
			{
				using (FileStream file = File.Open(settingsFilePath, FileMode.Open))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					settings = (GameSettings)formatter.Deserialize(file);
				}
			}
			catch (Exception e)
			{
				DebugManager.Log($"Failed to load settings, using default values: {e.Message}", DebugManager.DebugCategory.GENERAL, DebugManager.LogLevel.ERROR);
				settings = new GameSettings(); // Fallback to defaults
			}
		}
		else
		{
			settings = new GameSettings(); // Default values if no file exists
		}
	}

	public void ResetToDefaults()
	{
		settings = new GameSettings();
		SaveSettings();
	}
}
