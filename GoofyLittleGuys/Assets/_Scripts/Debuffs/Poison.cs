using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poison : Debuff
{
	private LilGuyBase affectedGuy;
	private GameObject instantiatedFX;
	public override void Init(float damage, float duration, float interval)
	{
		this.damage = damage;
		this.duration = duration;
		currentDuration = 0;
		damageApplicationInterval = interval;

		affectedGuy = GetComponent<LilGuyBase>();

		instantiatedFX = Instantiate(FXManager.Instance.GetEffect("Poisoned"), affectedGuy.transform.position, Quaternion.identity, affectedGuy.transform);
		StartCoroutine(Poisoned());
	}

	private IEnumerator Poisoned()
	{
		Hurtbox h = affectedGuy.GetComponent<Hurtbox>();
		if (h != null)
		{
			while (currentDuration < duration)
			{
				h.TakeDamage(damage, false);

				// Wait for the interval but allow currentDuration to be reset during this period
				float elapsed = 0;
				while (elapsed < damageApplicationInterval)
				{
					if (currentDuration >= duration)
					{
						// Break out if the duration is exceeded during the wait
						yield break;
					}
					elapsed += Time.deltaTime;
					yield return null; // Wait for the next frame
				}

				currentDuration += damageApplicationInterval;
			}
		}

		Destroy(this); // Destroy the component when the effect ends
	}

	private void OnDestroy()
	{
		Destroy(instantiatedFX);
	}
}
