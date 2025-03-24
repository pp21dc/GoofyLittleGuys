using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainCollisionHandler : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	public ParticleSystem rainParticles;
	public ParticleSystem splashParticlesPrefab;

	[Header("Rain Settings")]
	[HorizontalRule]
	[SerializeField] private LayerMask splashCollisionMask; // Restrict splashes to specific layers
	[SerializeField] private int maxSplashes = 50; // Max number of splashes allowed at a time

	private int currentSplashCount = 0; // Tracks active splashes
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private void Start()
	{
		if (!rainParticles) rainParticles = GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject other)
	{
		if (((1 << other.layer) & splashCollisionMask) == 0) return; // Skip if not in mask
		if (currentSplashCount >= maxSplashes) return; // Prevent excessive spawns

		int numCollisionEvents = rainParticles.GetCollisionEvents(other, collisionEvents);

		for (int i = 0; i < numCollisionEvents; i++)
		{
			if (currentSplashCount >= maxSplashes) break; // Stop spawning if max reached

			Vector3 hitPoint = collisionEvents[i].intersection;
			Vector3 hitNormal = collisionEvents[i].normal;
			SpawnSplash(hitPoint, hitNormal);
		}
	}

	private void SpawnSplash(Vector3 hitPoint, Vector3 hitNormal)
	{
		ParticleSystem splash = Instantiate(splashParticlesPrefab, hitPoint, Quaternion.LookRotation(hitNormal), transform);
		currentSplashCount++; // Increment active splashes

		// Auto-destroy splash and decrease count when it's gone
		//StartCoroutine(DestroySplashAfterTime(splash));
	}

	private IEnumerator DestroySplashAfterTime(ParticleSystem splash)
	{
		float lifetime = 2;
		yield return new WaitForSeconds(lifetime);
		Destroy(splash.gameObject);
		currentSplashCount--; // Decrease active splash count
	}
}
