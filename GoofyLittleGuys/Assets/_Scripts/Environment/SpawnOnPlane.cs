using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnPlane : MonoBehaviour
{
	public GameObject prefab; // The prefab to spawn
	public int spawnCount = 10; // Number of prefabs to spawn
	public Collider spawnArea; // The plane's collider
	public Vector2 spawnInterval = new Vector2(0.5f, 1.5f);

	private Coroutine lightningCoroutine = null;
	public void StartLightning()
	{
		lightningCoroutine = StartCoroutine(SpawnLightning());
	}
	public void StopLightning()
	{
		if (lightningCoroutine != null) StopCoroutine(lightningCoroutine);
	}

	private IEnumerator SpawnLightning()
	{
		while (true)
		{

			SpawnPrefab();
			yield return new WaitForSecondsRealtime(Random.Range(spawnInterval.x, spawnInterval.y));

		}
	}
	void SpawnPrefab()
	{
		if (spawnArea == null)
		{
			Managers.DebugManager.Log($"{name}: No spawn area assigned!", Managers.DebugManager.DebugCategory.ENVIRONMENT, Managers.DebugManager.LogLevel.ERROR);
			return;
		}

		// Get the bounds of the collider
		Bounds bounds = spawnArea.bounds;

		// Pick a random position within the collider’s bounds
		float x = Random.Range(bounds.min.x, bounds.max.x);
		float z = Random.Range(bounds.min.z, bounds.max.z);
		float y = bounds.max.y; // Assume the top of the plane

		Vector3 spawnPosition = new Vector3(x, y, z);

		// Instantiate the prefab at the position
		Instantiate(prefab, spawnPosition, Quaternion.identity);
	}
}