using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slow : Debuff
{
	private GameObject affectedGuy;
	private GameObject instantiatedFX;
	public override void Init(float damage, float duration, float interval)
	{
		this.damage = damage;
		this.duration = duration;
		currentDuration = 0;
		damageApplicationInterval = interval;

		affectedGuy = gameObject;
		StartCoroutine(Slowed(() => currentDuration, (value) => currentDuration = value));
	}
	private IEnumerator Slowed(Func<float> getCurrentDuration, Action<float> setCurrentDuration)
	{
		PlayerBody body = GetComponent<PlayerBody>();
		LilGuyBase lilGuy = GetComponent<LilGuyBase>();
		if (body != null)
		{
			body.TeamSpeedBoost -= damage;
		}
		else
		{
			
			lilGuy.Speed -= damage;
		}

		while (getCurrentDuration() < duration)
		{
			float prevDuration = getCurrentDuration();
			setCurrentDuration(prevDuration + Time.deltaTime);
			yield return null;
		}

		if (body != null)
		{
			body.TeamSpeedBoost += damage;
		}
		else
		{
			lilGuy.Speed += damage;
		}


		Destroy(this); // Destroy the component when the effect ends
	}

}
