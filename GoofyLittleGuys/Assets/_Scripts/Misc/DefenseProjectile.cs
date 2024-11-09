using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseProjectile : MonoBehaviour
{
	[SerializeField] private float despawnTime = 3f;	// Time in seconds before this projectile despawns
	private GameObject player;							// The player who's currently playing the defense minigame
	private DefenseMinigame minigameController;			// The Defense minigame instance that this projectile should communicate to

	private void Awake()
	{
		StartCoroutine(Despawn(despawnTime));
	}

	private void OnTriggerEnter(Collider collision)
	{
		if (collision.gameObject == player)
		{
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Coroutine that handles despawn behaviour of the projectiles.
	/// </summary>
	/// <param name="despawnTime">The time in seconds before this projectile despawns.</param>
	/// <returns></returns>
	private IEnumerator Despawn(float despawnTime)
	{
		yield return new WaitForSeconds(despawnTime);

		// If it despawns, then increment miss count.
		minigameController.MissCount++;
		Destroy(gameObject);
	}

	/// <summary>
	/// Method called when this projectile is spawned.
	/// </summary>
	/// <param name="p">The player this projectile has to hit to count towards the minigame.</param>
	/// <param name="minigame">The minigame instance.</param>
	public void Init(GameObject p, DefenseMinigame minigame)
	{
		player = p;
		minigameController = minigame;
	}
}
