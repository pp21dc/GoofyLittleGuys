using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseProjectile : MonoBehaviour
{
	[SerializeField] private float despawnTime = 3f;

	private GameObject player;
	private DefenseMinigame minigameController;

	private void Awake()
	{
		StartCoroutine(Despawn(despawnTime));
	}

	private IEnumerator Despawn(float despawnTime)
	{
		yield return new WaitForSeconds(despawnTime);

		minigameController.MissCount++;
		Destroy(gameObject);
	}
	private void OnTriggerEnter(Collider collision)
	{
		if (collision.gameObject == player)
		{
			Destroy(gameObject);
		}
	}

	public void Init(GameObject p, DefenseMinigame minigame)
	{
		player = p;
		minigameController = minigame;
	}
}
