using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealingFountain : InteractableBase
{
	[SerializeField] private Transform spawnPoint;				// The position players should respawn at, should all their lil guys be defeated.
	List<GameObject> playersInRange = new List<GameObject>();
	private void OnTriggerStay(Collider other)
	{
		PlayerBody playerInRange = other.GetComponent<PlayerBody>();
		if (playerInRange == null) return;
		interactableCanvas.SetActive(GameManager.Instance.CurrentPhase != 2);

		if (!playersInRange.Contains(other.gameObject))
		{
			// Add any players in range to the in range list.
			playersInRange.Add(other.gameObject);
		}

		playerInRange.ClosestInteractable = this;
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBody playerInRange = other.GetComponent<PlayerBody>();
		if (playerInRange == null) return;
		// Removing players from in range list if they go outside of the fountain's range/
		if (playersInRange.Contains(other.gameObject))
		{
			playersInRange.Remove(other.gameObject);
		}
		if (playersInRange.Count <= 0 || GameManager.Instance.CurrentPhase == 2)
		{
			interactableCanvas.SetActive(false);
		}

		playerInRange.ClosestInteractable = null;
	}
	private void Awake()
	{
		GameManager.Instance.FountainSpawnPoint = spawnPoint;
	}

	public override void StartInteraction(PlayerBody body)
	{
		if (GameManager.Instance.CurrentPhase == 2) return;
		if (body.IsDead) return;
		base.StartInteraction(body);
	}

	/// <summary>
	/// Called when a player stops interacting (releasing the button).
	/// </summary>
	public override void CancelInteraction(PlayerBody body)
	{
		if (GameManager.Instance.CurrentPhase == 2) return;
		if (body.IsDead) return;
		base.CancelInteraction(body);
	}

	protected override void CompleteInteraction(PlayerBody body)
	{
		base.CompleteInteraction(body);
		if (GameManager.Instance.CurrentPhase == 2) return;
		if (body.IsDead) return;
		body.GameplayStats.FountainUses++;

		HapticEvent fountainHaptic = GameManager.Instance.GetHapticEvent("Fountain Used");
		if (fountainHaptic != null)
		{
			HapticFeedback.PlayHapticFeedback(body.Controller.GetComponent<PlayerInput>(), fountainHaptic.lowFrequency, fountainHaptic.highFrequency, fountainHaptic.duration);
		}

		foreach (LilGuyBase lilGuy in body.LilGuyTeam)
		{
			if (lilGuy.Health < lilGuy.MaxHealth)
			{
				// Heal up every lil guy in the player's team to full.
				// Reset their visuals.
				EventManager.Instance.HealLilGuy(lilGuy, (int)lilGuy.MaxHealth);
				lilGuy.IsDying = false;
				lilGuy.gameObject.SetActive(true);
				lilGuy.GetComponentInChildren<SpriteRenderer>().color = Color.white;
			}
		}
		EventManager.Instance.UpdatePlayerHealthUI(body);
	}
}
