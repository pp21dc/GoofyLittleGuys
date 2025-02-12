using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slow : Debuff
{
	private GameObject affectedGuy;
	private GameObject instantiatedFX;
	PlayerBody body;
	LilGuyBase lilGuy;
	public override void Init(float damage, float duration, float interval)
	{
		this.damage = damage;
		this.duration = duration;
		currentDuration = 0;
		damageApplicationInterval = interval;
		body = GetComponent<PlayerBody>();
		lilGuy = (body == null) ? GetComponent<LilGuyBase>() : body.LilGuyTeam[0];
		affectedGuy = gameObject;
		StartCoroutine(Slowed(() => currentDuration, (value) => currentDuration = value));
	}
	private IEnumerator Slowed(Func<float> getCurrentDuration, Action<float> setCurrentDuration)
	{
		lilGuy.MoveSpeedModifier = Mathf.Max(lilGuy.MoveSpeedModifier + damage, 0.1f);

		while (getCurrentDuration() < duration)
		{
			float prevDuration = getCurrentDuration();
			setCurrentDuration(prevDuration + Time.deltaTime);
			yield return null;
		}

		Destroy(this); // Destroy the component when the effect ends
	}

	private void OnDestroy()
	{

		lilGuy.MoveSpeedModifier = Mathf.Max(lilGuy.MoveSpeedModifier - damage, 1f);

	}
}
