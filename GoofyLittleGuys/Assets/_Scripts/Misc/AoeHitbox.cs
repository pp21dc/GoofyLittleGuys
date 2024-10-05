using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class AoeHitbox : MonoBehaviour
{
	private List<Collider> lilGuysInRadius;
	LilGuyBase hitboxOwner;	// The Lil Guy who used the AoE attack
	/// <summary>
	/// Begins expanding the AoE blast zone by provided speed value, until it reaches max size.
	/// </summary>
	/// <param name="maxSize">The maximum size this blast zone reaches</param>
	/// <param name="expansionSpeed">The speed of which the blast zone expands</param>
	/// <param name="owner">The Lil Guy who initiated this AoE attack</param>
	public void InitializeExpansion(float maxSize, float expansionSpeed, LilGuyBase owner)
	{
		hitboxOwner = owner;
		StartCoroutine(Expand(maxSize, expansionSpeed));
	}

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
		hitboxOwner.GetComponent<AoeSpecialMove>().HitColliders = lilGuysInRadius.ToArray();	// Give the owner all the lil guys this hitbox hit.
	}
	private void OnTriggerEnter(Collider other)
	{
		LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
        if (lilGuy != null && lilGuy != hitboxOwner && !AlreadyAdded(other))
        {
			lilGuysInRadius.Add(other);
        }
    }
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
	}
}
