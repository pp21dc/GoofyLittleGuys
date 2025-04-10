using Managers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
	public GameObject firstSelected;
	[SerializeField] private AudioMixer MasterMixer;
	[SerializeField, DebugOnly] private GameObject previousMenu;

	[Header("Audio Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private Slider masterSlider;
	[ColoredGroup][SerializeField] private Slider musicSlider;
	[ColoredGroup][SerializeField] private Slider sfxSlider;
	[ColoredGroup][SerializeField] private Slider rumbleSlider;
	[ColoredGroup][SerializeField] private Slider brightnessSlider;
	[ColoredGroup][SerializeField] private Slider contrastSlider;

	[Header("Display Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private ScrollableDropdown resolutionDropdown;
	[ColoredGroup][SerializeField] private ScrollableDropdown windowModeDropdown;
	[ColoredGroup][SerializeField] private Slider frameCapSlider;
	[ColoredGroup][SerializeField] private TMP_Text frameCapLabel; // Optional label to show value

	public enum SettingsContext
	{
		Global,
		Player
	}

	private SettingsContext currentContext = SettingsContext.Global;
	private Volume targetPlayerVolume;
	private Volume previewVolume;

	private List<Resolution> supportedResolutions = new();
	private readonly List<int> frameCapSteps = new() { 30, 60, 120, 144, 165, 240, -1 }; // -1 = Unlimited

	public GameObject PreviousMenu{ set => previousMenu = value; }

	private readonly Dictionary<SettingType, string> mixerParams = new()
	{
		{ SettingType.Master, "masterVolume" },
		{ SettingType.Music, "musicVolume" },
		{ SettingType.SFX, "sfxVolume" }
	};

	private void OnEnable()
	{
		StartCoroutine(DelayedSelect());
	}

	private IEnumerator DelayedSelect()
	{
		yield return null; // wait one frame
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(firstSelected);
	}
	private void Start()
	{
		ApplyAllFromSettings();
		SetupResolutionDropdown();
		SetupWindowModeDropdown();
	}


	public void InitializeAsGlobal(Volume preview)
	{
		currentContext = SettingsContext.Global;
		previewVolume = preview;

		brightnessSlider.value = MapSliderFromBrightness(SettingsManager.Instance.GetBrightness());
		contrastSlider.value = MapSliderFromContrast(SettingsManager.Instance.GetContrast());
	}

	public void InitializeAsPlayer(Volume playerVolume)
	{
		currentContext = SettingsContext.Player;
		targetPlayerVolume = playerVolume;

		if (targetPlayerVolume.profile.TryGet(out ColorAdjustments colorAdjust))
		{
			contrastSlider.value = MapSliderFromContrast(colorAdjust.contrast.value);
			brightnessSlider.value = MapSliderFromBrightness(colorAdjust.postExposure.value);
		}
	}

	public void OnBrightnessSliderChanged(float normalizedValue)
	{
		float mappedValue = MapBrightnessSlider(normalizedValue);
		Debug.LogError(currentContext.ToString());
		if (currentContext == SettingsContext.Global)
		{
			SettingsManager.Instance.SetBrightness(mappedValue);
			if (previewVolume && previewVolume.profile.TryGet(out ColorAdjustments ca))
				ca.postExposure.value = mappedValue;
		}
		else if (currentContext == SettingsContext.Player)
		{
			if (targetPlayerVolume && targetPlayerVolume.profile.TryGet(out ColorAdjustments ca))
				ca.postExposure.value = mappedValue;
		}
	}


	public void OnContrastSliderChanged(float normalizedValue)
	{
		float mappedValue = MapContrastSlider(normalizedValue);
		if (currentContext == SettingsContext.Global)
		{
			SettingsManager.Instance.SetContrast(mappedValue);
			if (previewVolume && previewVolume.profile.TryGet(out ColorAdjustments colorAdjust))
				colorAdjust.contrast.value = mappedValue;
		}
		else if (currentContext == SettingsContext.Player)
		{
			if (targetPlayerVolume && targetPlayerVolume.profile.TryGet(out ColorAdjustments colorAdjust))
				colorAdjust.contrast.value = mappedValue;
		}
	}

	float MapBrightnessSlider(float sliderValue)
	{
		return Mathf.Lerp(-2f, 2f, sliderValue);
	}

	float MapSliderFromBrightness(float value)
	{
		return Mathf.InverseLerp(-2f, 2f, value);
	}

	float MapContrastSlider(float sliderValue)
	{
		return Mathf.Lerp(-50f, 50f, sliderValue);
	}

	float MapSliderFromContrast(float value)
	{
		return Mathf.InverseLerp(-50f, 50f, value);
	}


	public void OnMasterChanged() => ApplySetting(SettingType.Master, masterSlider.value);
	public void OnMusicChanged() => ApplySetting(SettingType.Music, musicSlider.value);
	public void OnSFXChanged() => ApplySetting(SettingType.SFX, sfxSlider.value);
	public void OnRumbleChanged()
	{
		ApplySetting(SettingType.Rumble, rumbleSlider.value);

		var haptic = GameManager.Instance.GetHapticEvent("Settings Rumble");
		if (haptic != null)
		{
			foreach (Gamepad gamepad in Gamepad.all)
			{
				if (gamepad != null)
				{
					HapticFeedback.PlayHapticFeedbackDirect(gamepad, haptic.lowFrequency, haptic.highFrequency, haptic.duration);
				}
			}
		}
	}

	public void OnFrameCapChanged()
	{
		Managers.UiManager.Instance.PlayButtonPressSfx();
		int index = Mathf.RoundToInt(frameCapSlider.value);
		int selectedCap = frameCapSteps[Mathf.Clamp(index, 0, frameCapSteps.Count - 1)];

		ApplyFrameCap(selectedCap);

		var settings = SettingsManager.Instance.GetSettings();
		settings.frameCap = selectedCap;

		frameCapLabel.text = selectedCap == -1 ? "Unlimited" : $"{selectedCap}";
	}



	private void ApplyFrameCap(int value)
	{
		Application.targetFrameRate = value;
		QualitySettings.vSyncCount = 0; // Disable vSync to use the frame cap
	}


	private void SetupResolutionDropdown()
	{
		supportedResolutions.Clear();
		List<string> options = new();
		int currentIndex = 0;

		foreach (var res in Screen.resolutions)
		{
			string label = $"{res.width} x {res.height}";
			if (!options.Contains(label))
			{
				options.Add(label);
				supportedResolutions.Add(res);
			}
		}

		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(options);

		var settings = SettingsManager.Instance.GetSettings();
		currentIndex = Mathf.Clamp(settings.resolutionIndex, 0, supportedResolutions.Count - 1);

		resolutionDropdown.SetValueWithoutNotify(currentIndex);
		resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
	}

	private void SetupWindowModeDropdown()
	{
		List<string> modes = new() { "Fullscreen", "Windowed", "Borderless" };
		windowModeDropdown.ClearOptions();
		windowModeDropdown.AddOptions(modes);

		var settings = SettingsManager.Instance.GetSettings();
		int index = Mathf.Clamp(settings.windowModeIndex, 0, modes.Count - 1);
		windowModeDropdown.SetValueWithoutNotify(index);
		windowModeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
	}

	private void OnResolutionChanged(int index)
	{
		Managers.UiManager.Instance.PlayButtonPressSfx();
		if (index < 0 || index >= supportedResolutions.Count) return;

		var settings = SettingsManager.Instance.GetSettings();
		settings.resolutionIndex = index;

		Resolution res = supportedResolutions[index];
		Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
	}

	private void OnWindowModeChanged(int index)
	{
		var settings = SettingsManager.Instance.GetSettings();
		settings.windowModeIndex = index;

		Screen.fullScreenMode = index switch
		{
			0 => FullScreenMode.FullScreenWindow,
			1 => FullScreenMode.Windowed,
			2 => FullScreenMode.MaximizedWindow,
			_ => FullScreenMode.FullScreenWindow
		};
	}


	public void ApplySetting(SettingType type, float value)
	{
		var settings = SettingsManager.Instance.GetSettings();

		switch (type)
		{
			case SettingType.Master:
				settings.masterVolume = value;
				SetMixerVolume(type, value);
				break;
			case SettingType.Music:
				settings.musicVolume = value;
				SetMixerVolume(type, value);
				break;
			case SettingType.SFX:
				settings.sfxVolume = value;
				SetMixerVolume(type, value);
				break;
			case SettingType.Rumble:
				settings.rumbleAmount = value;
				break;
		}

		if (frameCapSlider)
		{
			int savedCap = SettingsManager.Instance.GetSettings().frameCap;
			int index = frameCapSteps.IndexOf(savedCap);
			if (index < 0) index = frameCapSteps.Count - 1; // Default to Unlimited

			frameCapSlider.SetValueWithoutNotify(index);
			OnFrameCapChanged();
		}

		SettingsManager.Instance.SaveSettings();
	}


	private void SetMixerVolume(SettingType type, float value)
	{
		if (mixerParams.TryGetValue(type, out string param))
		{
			float dB = Mathf.Log10(Mathf.Max(value, 0.001f)) * 20f;
			MasterMixer.SetFloat(param, dB);
		}
	}

	public void ApplyAllFromSettings()
	{
		var s = SettingsManager.Instance.GetSettings();

		// Set values without triggering slider events
		if (masterSlider) masterSlider.SetValueWithoutNotify(s.masterVolume);
		if (musicSlider) musicSlider.SetValueWithoutNotify(s.musicVolume);
		if (sfxSlider) sfxSlider.SetValueWithoutNotify(s.sfxVolume);
		if (rumbleSlider) rumbleSlider.SetValueWithoutNotify(s.rumbleAmount);

		ApplySetting(SettingType.Master, s.masterVolume);
		ApplySetting(SettingType.Music, s.musicVolume);
		ApplySetting(SettingType.SFX, s.sfxVolume);
		ApplySetting(SettingType.Rumble, s.rumbleAmount);
	}

	public void OnExitButtonPressed()
	{
		Managers.UiManager.Instance.PlayButtonPressSfx();
		MainMenu mainMenu = previousMenu.GetComponent<MainMenu>();
		if (mainMenu != null)
		{
			mainMenu.ReinitializeMenu();
		}
		UiManager manager = previousMenu.GetComponent<UiManager>();
		if (manager != null)
		{
			manager.ReinitializeMenu();
		}
		UiManager.Instance.SettingsMenu.SetActive(false);
		SettingsManager.Instance.SaveSettings();
	}
}


public enum SettingType
{
	Master,
	Music,
	SFX,
	Rumble
}
