using System;
using System.Collections.Generic;
using UnityEngine;
using static Managers.DebugManager;

namespace Managers
{
	[Serializable]
	public class DebugCategorySettings
	{
		public DebugCategory category;
		public bool enabled = true;
		public LogLevel logLevelFilter = LogLevel.LOG;
	}

	public class DebugManager : SingletonBase<DebugManager>
	{

		public enum DebugCategory { SPAWNING, AI, COMBAT, UI, GENERAL, EDITOR_TOOL, INPUT, ENVIRONMENT, STAT_METRICS}

		public enum LogLevel { LOG, WARNING, ERROR }

		[Header("Global Debug Settings")]
		[HorizontalRule]
		[ColoredGroup][SerializeField] private bool enableAllLogs = true;

		[Header("Per-Category Debug Settings")]
		[HorizontalRule]
		[SerializeField] private List<DebugCategorySettings> debugCategories = new();

		private Dictionary<DebugCategory, DebugCategorySettings> debugSettings = new();

		public override void Awake()
		{
			base.Awake();
			// Initialize category settings
			foreach (var setting in debugCategories)
			{
				debugSettings[setting.category] = setting;
			}
		}

		public static void Log(string message, DebugCategory category = DebugCategory.GENERAL, LogLevel level = LogLevel.LOG)
		{
			if (Instance == null || !Instance.enableAllLogs) return;

			if (Instance.debugSettings.TryGetValue(category, out DebugCategorySettings settings))
			{
				if (!settings.enabled || level < settings.logLevelFilter) return;

				string formattedMessage = $"[{category}] {message}";

				switch (level)
				{
					case LogLevel.WARNING:
						Debug.LogWarning(formattedMessage);
						break;
					case LogLevel.ERROR:
						Debug.LogError(formattedMessage);
						break;
					default:
						Debug.Log(formattedMessage);
						break;
				}
			}
		}

		public static void ToggleDebugCategory(DebugCategory category, bool state)
		{
			if (Instance != null && Instance.debugSettings.TryGetValue(category, out var settings))
			{
				settings.enabled = state;
			}
		}

		public static void SetGlobalDebugState(bool state)
		{
			if (Instance != null)
			{
				Instance.enableAllLogs = state;
			}
		}

		public static void SetLogLevelFilter(DebugCategory category, LogLevel level)
		{
			if (Instance != null && Instance.debugSettings.TryGetValue(category, out var settings))
			{
				settings.logLevelFilter = level;
			}
		}
	}

}