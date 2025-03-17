using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AiHealthUi : MonoBehaviour
{
	[SerializeField] private Slider redBar;					// Current health (instant)
	[SerializeField] private Slider yellowBar;				// Aggravated health (lags behind)
	[SerializeField] private TMP_Text healthText;			// Health label
	[SerializeField] private TMP_Text levelText;			// Level label
	[SerializeField] private LilGuyBase lilGuy;				// Reference to the lil guy

	[SerializeField] private float yellowLerpSpeed = 1f;  // Speed at which yellow bar catches up
	[SerializeField] private float redLerpSpeed = 2.0f;						// Speed for red bar to catch up when healing

	private Coroutine yellowLerpCoroutine;

	public Slider YellowBar => yellowBar;
	
	private void Start()
	{
		// Initializing all slider min, max and current values to lil guy's health stats.
		redBar.minValue = 0;
		yellowBar.maxValue = 0;
		if (lilGuy != null)
		{
			redBar.maxValue = lilGuy.MaxHealth;
			yellowBar.maxValue = lilGuy.MaxHealth;
			redBar.value = lilGuy.Health;
			yellowBar.value = lilGuy.Health;
			UpdateUI();
		}
	}

	/// <summary>
	/// This method is called whenever the health of the lil guy is changed.
	/// </summary>
	/// <param name="newHealth">The new health amount of the lil guy</param>
	public void SetHealth(float newHealth, float oldHealth)
	{
		// Clamp health between 0 and maxHealth
		newHealth = Mathf.Clamp(newHealth, 0, lilGuy.MaxHealth);

		// If health decreased, update red bar immediately and yellow bar with delay
		if (newHealth < oldHealth)
		{
			redBar.value = newHealth;
			if (yellowLerpCoroutine != null)
				StopCoroutine(yellowLerpCoroutine);

			yellowLerpCoroutine = StartCoroutine(LerpYellowBar(newHealth, yellowLerpSpeed));
		}
		// If health increased, update yellow bar immediately and red bar with delay
		else if (newHealth > oldHealth)
		{
			yellowBar.value = newHealth;
			if (yellowLerpCoroutine != null)
				StopCoroutine(yellowLerpCoroutine);

			yellowLerpCoroutine = StartCoroutine(LerpRedBar(newHealth, redLerpSpeed));
		}

		UpdateUI();
	}

	/// <summary>
	/// Coroutine for smooth yellow bar lerping when taking damage
	/// </summary>
	/// <param name="targetFill">The end result of the health bar</param>
	/// <param name="lerpSpeed">How fast the lerp from value to value should be</param>
	/// <returns></returns>
	private IEnumerator LerpYellowBar(float targetFill, float lerpSpeed)
	{
		float startFill = yellowBar.value;
		float time = 0;

		while (yellowBar.value > targetFill)
		{
			time += Time.deltaTime * lerpSpeed;
			yellowBar.value = Mathf.Lerp(startFill, targetFill, time);
			yield return null;
		}
	}

	/// <summary>
	/// Coroutine for smooth red bar lerping when healing
	/// </summary>
	/// <param name="targetFill">The end result of the health bar</param>
	/// <param name="lerpSpeed">How fast the lerp from value to value should be</param>
	/// <returns></returns>
	private IEnumerator LerpRedBar(float targetFill, float lerpSpeed)
	{
		float startFill = redBar.value;
		float time = 0;

		while (redBar.value < targetFill)
		{
			time += Time.deltaTime * lerpSpeed;
			redBar.value = Mathf.Lerp(startFill, targetFill, time);
			yield return null;
		}
	}

	/// <summary>
	/// Updates the text label with the current health value
	/// </summary>
	public void UpdateUI()
	{
		healthText.text = $"{lilGuy.Health}/{lilGuy.MaxHealth}";
		levelText.text = $"Lv. {lilGuy.Level}";
	}

}
