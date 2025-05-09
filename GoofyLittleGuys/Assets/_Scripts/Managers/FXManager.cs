using Managers;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : SingletonBase<FXManager>
{
	[System.Serializable]
	public class EffectEntry
	{
		public string name; // Name of the effect
		public GameObject effect; // Corresponding GameObject
	}

	[SerializeField] private List<EffectEntry> effects;

	private Dictionary<string, GameObject> effectsDictionary;

	public override void Awake()
	{
		base.Awake();
		InitializeEffectsDictionary();
	}

	private void InitializeEffectsDictionary()
	{
		// Initialize the dictionary
		effectsDictionary = new Dictionary<string, GameObject>();

		// Populate the dictionary with the entries
		foreach (var entry in effects)
		{
			if (!effectsDictionary.ContainsKey(entry.name))
			{
				effectsDictionary.Add(entry.name, entry.effect);
			}
			else
			{
				Managers.DebugManager.Log($"FXManager: Duplicate effect name '{entry.name}' detected. Only the first occurrence will be used.", DebugManager.DebugCategory.GENERAL, DebugManager.LogLevel.WARNING);
			}
		}
	}

	// Method to get an effect by name
	public GameObject GetEffect(string effectName)
	{
		if (effectsDictionary.TryGetValue(effectName, out GameObject effect))
		{
			return effect;
		}

		Managers.DebugManager.Log($"FXManager: Effect '{effectName}' not found in the dictionary.", DebugManager.DebugCategory.GENERAL, DebugManager.LogLevel.ERROR);
		return null;
	}
}
