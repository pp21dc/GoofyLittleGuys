using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slow : MonoBehaviour
{
	private class SlowInstance
	{
		public float amount;
		public float duration;
		public float elapsed;
		public object source;
	}

	private List<SlowInstance> activeSlows = new List<SlowInstance>();
	private LilGuyBase lilGuy;
	private PlayerBody body;
	private Coroutine tickCoroutine;

	public void ApplySlow(float amount, float duration, object source)
	{
		if (lilGuy == null)
		{
			body = GetComponent<PlayerBody>();
			lilGuy = body != null ? body.LilGuyTeam[0] : GetComponent<LilGuyBase>();
		}

		activeSlows.Add(new SlowInstance
		{
			amount = amount,
			duration = duration,
			elapsed = 0,
			source = source
		});

		if (tickCoroutine == null)
			tickCoroutine = StartCoroutine(HandleSlows());
	}

	private IEnumerator HandleSlows()
	{
		while (activeSlows.Count > 0)
		{
			// Remove expired slows
			for (int i = activeSlows.Count - 1; i >= 0; i--)
			{
				activeSlows[i].elapsed += Time.deltaTime;
				if (activeSlows[i].elapsed >= activeSlows[i].duration)
					activeSlows.RemoveAt(i);
			}

			if (activeSlows.Count > 0)
			{
				// Apply strongest slow
				float lowestSlow = 0;
				foreach (var s in activeSlows)
					if (s.amount < lowestSlow) lowestSlow = s.amount;

				lilGuy.MoveSpeedModifier = Mathf.Clamp(1f + lowestSlow, 0.1f, 1f);
			}
			else
			{
				lilGuy.MoveSpeedModifier = 1f;
				Destroy(this); // Clean up when no slows remain
				yield break;
			}

			yield return null;
		}
	}

	private void OnDestroy()
	{
		if (lilGuy != null)
			lilGuy.MoveSpeedModifier = 1f; // Reset on forced removal
	}
}
