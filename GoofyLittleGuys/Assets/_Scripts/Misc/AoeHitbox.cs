using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class AoeHitbox : Hitbox
{
	// private List<Collider> lilGuysInRadius = new List<Collider>();

	/// <summary>
	/// Begins expanding the AoE blast zone by provided speed value, until it reaches max size.
	/// </summary>
	/// <param name="maxSize">The maximum size this blast zone reaches</param>
	/// <param name="expansionSpeed">The speed of which the blast zone expands</param>
	/// <param name="owner">The Lil Guy who initiated this AoE attack</param>
	public void InitializeExpansion(float maxSize, float expansionSpeed, LilGuyBase owner)
	{
		Init(owner.gameObject);
		StartCoroutine(Expand(maxSize, expansionSpeed));
	}

	/// <summary>
	/// Method called when the hitbox is first spawned in the world
	/// </summary>
	/// <param name="hitboxOwner">The lil guy who created this hitbox.</param>
	public override void Init(GameObject hitboxOwner)
	{
		this.hitboxOwner = hitboxOwner;
		gameObject.layer = hitboxOwner.layer;
		Damage = hitboxOwner.GetComponent<StrengthType>().aoeDamage + hitboxOwner.GetComponent<LilGuyBase>().Strength;
	}

	/// <summary>
	/// Coroutine that handles the expansion of the AoE, by expanding it over time.
	/// </summary>
	/// <param name="maxSize">The maximum size the AoE will get.</param>
	/// <param name="expansionSpeed">How fast the AoE reaches the max size in seconds.</param>
	/// <returns></returns>
	private IEnumerator Expand(float maxSize, float expansionSpeed)
	{
		Vector3 initialScale = Vector3.zero;
		Vector3 targScale = new Vector3(maxSize, maxSize, maxSize);

		float elapsedTime = 0;
		while (elapsedTime < expansionSpeed)
		{
			// Expanding the hitbox zone
			transform.localScale = Vector3.Lerp(initialScale, targScale, elapsedTime / expansionSpeed);
			elapsedTime += Time.deltaTime;

			yield return null;
		}

		transform.localScale = targScale;

		yield return new WaitForSeconds(1);
		Destroy(gameObject);
	}


	/*
	/// <summary>
	/// Checks if the lil guy who entered the zone was already tagged as hit.
	/// </summary>
	/// <param name="other">The collider to check</param>
	/// <returns>True if the Lil Guy was already tagged as hit, otherwise false.</returns>
	private bool AlreadyAdded(Collider other)
	{
		if (lilGuysInRadius.Count <= 0) return false;
		foreach(Collider c in lilGuysInRadius)
		{
			if (c == other) return true;
		}
		return false;
	}*/
}
