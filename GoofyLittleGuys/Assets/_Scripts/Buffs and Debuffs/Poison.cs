using System.Collections;
using UnityEngine;

public class Poison : MonoBehaviour
{
	private LilGuyBase affectedGuy;
	private Coroutine poisonCoroutine;
	private GameObject instantiatedFX;

	private void Awake()
	{
		affectedGuy = GetComponent<LilGuyBase>();
	}

	private void OnDestroy()
	{
		if (instantiatedFX != null) Destroy(instantiatedFX);
	}

	public void ApplyPoison(float damagePerTick, float duration, float interval, object source)
	{
		// Add or refresh the poison buff
		affectedGuy.Buffs.AddBuff(BuffType.Poison, damagePerTick, duration, source);

		if (poisonCoroutine == null)
		{
			instantiatedFX = Instantiate(FXManager.Instance.GetEffect("Poisoned"), affectedGuy.transform.position, Quaternion.identity, affectedGuy.transform);
			poisonCoroutine = StartCoroutine(ApplyPoisonDamage(interval));
		}
	}

	private IEnumerator ApplyPoisonDamage(float interval)
	{
		Hurtbox h = affectedGuy.GetComponent<Hurtbox>();
		if (h == null) yield break;

		while (affectedGuy.Buffs.GetTotalValue(BuffType.Poison) > 0)
		{
			float poisonDamage = affectedGuy.Buffs.GetTotalValue(BuffType.Poison);
			h.TakeDamage(poisonDamage, false);
			yield return new WaitForSeconds(interval);
		}

		poisonCoroutine = null;
		if (instantiatedFX != null) Destroy(instantiatedFX);
		Destroy(this);
	}
}
