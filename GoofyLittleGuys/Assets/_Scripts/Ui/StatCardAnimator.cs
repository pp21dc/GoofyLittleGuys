using System.Collections;
using UnityEngine;
using TMPro;
using Managers;
using UnityEngine.InputSystem;

public class StatCardAnimator : MonoBehaviour
{
	public enum StatRevealMode { None, FadeIn, ScrollDown }

	[Header("Scale Animation")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private Vector3 targetScaleWinner = Vector3.one;
	[ColoredGroup][SerializeField] private Vector3 targetScaleLoser = Vector3.one * 0.9f;
	[ColoredGroup][SerializeField] private float scaleDuration = 0.4f;
	[ColoredGroup][SerializeField] private bool isWinner = false;

	[Header("Pulsing")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private bool enablePulse = true;
	[ColoredGroup][SerializeField] private float pulseSpeed = 1.5f;
	[ColoredGroup][SerializeField] private float pulseAmount = 0.02f;

	[Header("Title Bounce")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private bool animateTitles = true;
	[ColoredGroup][SerializeField] private float titleDelay = 0.3f;
	[ColoredGroup][SerializeField] private float titlePopScale = 1.2f;
	[ColoredGroup][SerializeField] private float titlePopDuration = 0.2f;
	[ColoredGroup][SerializeField] private TMP_Text[] titleTexts;

	[Header("Stats Reveal")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private StatRevealMode statMode = StatRevealMode.FadeIn;
	[ColoredGroup][SerializeField] private float statRevealDuration = 0.75f;
	[ColoredGroup][SerializeField] private CanvasGroup statsGroup;
	[ColoredGroup][SerializeField] private ParticleSystem confettiEffect;
	[ColoredGroup][SerializeField] private RectTransform statNames;
	[ColoredGroup][SerializeField] private RectTransform statValues;
	[ColoredGroup][SerializeField] private float statsScaleDuration = 0.25f;

	private RectTransform rect;
	private Vector3 initialScale = Vector3.zero;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
		rect.localScale = initialScale;

		if (statMode == StatRevealMode.FadeIn && statsGroup != null)
			statsGroup.alpha = 0;

		if (statMode == StatRevealMode.ScrollDown)
		{
			if (statNames != null) statNames.sizeDelta = new Vector2(statNames.sizeDelta.x, 0);
			if (statValues != null) statValues.sizeDelta = new Vector2(statValues.sizeDelta.x, 0);
		}
	}

	public void BeginScaleIn(float delay, bool isWinner)
	{
		this.isWinner = isWinner;
		StartCoroutine(ScaleInCoroutine(delay));
	}

	public void TriggerPostScaleEffects()
	{
		if (isWinner && confettiEffect != null)
		{
			confettiEffect.Play();
		}
		StartCoroutine(PostScaleEffectsCoroutine());
	}

	private IEnumerator ScaleInCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);

		Vector3 target = isWinner ? targetScaleWinner : targetScaleLoser;
		yield return StartCoroutine(ScaleIn(target));
	}

	private IEnumerator PostScaleEffectsCoroutine()
	{
		// Start Pulse
		if (enablePulse)
			StartCoroutine(PulseLoop(isWinner ? targetScaleWinner : targetScaleLoser));

		// Animate Titles
		if (animateTitles && titleTexts != null)
		{
			for (int i = 0; i < titleTexts.Length; i++)
			{
				TMP_Text t = titleTexts[i];
				if (t == null) continue;
				t.transform.localScale = Vector3.zero;
				yield return StartCoroutine(ScaleUIElement(t.transform, Vector3.one * titlePopScale, Vector3.one, titlePopDuration));
				yield return new WaitForSeconds(titleDelay);
			}
		}

		// Stats Reveal
		if (statMode == StatRevealMode.FadeIn && statsGroup != null)
		{
			float timer = 0;
			while (timer < statRevealDuration)
			{
				statsGroup.alpha = Mathf.Lerp(0, 1, timer / statRevealDuration);
				timer += Time.deltaTime;
				yield return null;
			}
			statsGroup.alpha = 1;
		}
		else if (statMode == StatRevealMode.ScrollDown)
		{
			if (statNames != null && statValues != null)
			{
				yield return StartCoroutine(AnimateStatHeight(statNames, statValues, 0, 370.21f, statsScaleDuration));
			}
		}
	}

	private IEnumerator ScaleIn(Vector3 target)
	{
		float timer = 0f;
		while (timer < scaleDuration)
		{
			rect.localScale = Vector3.Lerp(initialScale, target, timer / scaleDuration);
			timer += Time.deltaTime;
			yield return null;
		}
		rect.localScale = target;
	}

	private IEnumerator AnimateStatHeight(RectTransform rect, RectTransform rect2, float startHeight, float endHeight, float duration)
	{
		float elapsed = 0f;
		Vector2 size = rect.sizeDelta;
		Vector2 size2 = rect2.sizeDelta;
		while (elapsed < duration)
		{
			float t = elapsed / duration;
			size.y = Mathf.Lerp(startHeight, endHeight, t);
			size2.y = Mathf.Lerp(startHeight, endHeight, t);
			rect.sizeDelta = size;
			rect2.sizeDelta = size2;
			elapsed += Time.deltaTime;
			yield return null;
		}
		size.y = endHeight;
		size2.y = endHeight;
		rect.sizeDelta = size;
		rect2.sizeDelta = size2;
	}

	private IEnumerator PulseLoop(Vector3 baseScale)
	{
		float t = 0;
		float actualPulse = isWinner ? pulseAmount : pulseAmount * 0.5f;
		while (true)
		{
			float scaleFactor = 1 + Mathf.Sin(t * pulseSpeed) * actualPulse;
			rect.localScale = baseScale * scaleFactor;
			t += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator ScaleUIElement(Transform t, Vector3 start, Vector3 end, float duration)
	{
		float time = 0f;
		t.localScale = start;
		while (time < duration)
		{
			t.localScale = Vector3.Lerp(start, end, time / duration);
			time += Time.deltaTime;
			yield return null;
		}
		t.localScale = end;
		HapticEvent hapticEvent = GameManager.Instance.GetHapticEvent("Title Reveal");
		if (hapticEvent != null) HapticFeedback.PlayHapticFeedback(GetComponent<StatCard>().PlayerInput, hapticEvent.lowFrequency, hapticEvent.highFrequency, hapticEvent.duration);
	}
}
