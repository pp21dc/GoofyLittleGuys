using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackHitbox : MonoBehaviour
{
	[SerializeField] private float knockbackForce = 10f; // Strength of knockback
	[SerializeField] private bool relativeToHitbox = true; // Direction relative to hitbox center
	[SerializeField] private float knockbackDuration = 1f; 

	private HashSet<LilGuyBase> wildLilGuys = new HashSet<LilGuyBase>(); // Track wild lil guys
	private HashSet<LilGuyBase> playerLilGuys = new HashSet<LilGuyBase>(); // Track player-owned lil guys

	public HashSet<LilGuyBase> PlayerLilGuys => playerLilGuys;
	public HashSet<LilGuyBase> WildLilGuys => wildLilGuys;
	public float KnockbackForce { set { knockbackForce = value; } }
	public float KnockbackDuration { set { knockbackDuration = value; } }


	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			HandleKnockback(other, wildLilGuys);
		}
		else if (other.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
		{
			HandleKnockback(other, playerLilGuys, true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			StartCoroutine(ResetKnockback(other, wildLilGuys));
		}
		else if (other.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
		{
			StartCoroutine(ResetKnockback(other, playerLilGuys, true));
		}
	}

	public void HandleKnockback(Collider other, HashSet<LilGuyBase> lilGuySet, bool isPlayerOwned = false)
	{
		LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
		if (lilGuy != null)
		{
			// Set KnockedBack state
			if (isPlayerOwned)
			{
				lilGuy.PlayerOwner.KnockedBack = true;
			}
			else
			{
				lilGuy.KnockedBack = true;
			}

			// Calculate knockback direction
			Vector3 direction = relativeToHitbox
				? (other.transform.position - transform.position).normalized
				: Vector3.up;

			// Apply knockback force
			Rigidbody rb = isPlayerOwned ? lilGuy.PlayerOwner.GetComponent<Rigidbody>() : lilGuy.RB;
			if (rb != null)
			{
				rb.AddForce(direction * knockbackForce, ForceMode.VelocityChange);
			}

			// Add to the tracking set if not already tracked
			if (!lilGuySet.Contains(lilGuy))
			{
				lilGuySet.Add(lilGuy);
			}
		}
	}

	public IEnumerator ResetKnockback(Collider other, HashSet<LilGuyBase> lilGuySet, bool isPlayerOwned = false)
	{
		yield return new WaitForSeconds(knockbackDuration);
		LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
		if (lilGuy != null && lilGuySet.Contains(lilGuy))
		{
			// Reset KnockedBack state
			if (isPlayerOwned)
			{
				lilGuy.PlayerOwner.KnockedBack = false;
			}
			else
			{
				lilGuy.KnockedBack = false;
			}

			// Remove from the tracking set
			lilGuySet.Remove(lilGuy);
		}
	}

	private void OnDestroy()
	{
		ResetAllKnockbackStates();
	}
	private void ResetAllKnockbackStates()
	{
		if (wildLilGuys.Count > 0)
		{
			// Reset all wild lil guys
			foreach (LilGuyBase lilGuy in wildLilGuys)
			{
				if (lilGuy != null)
				{
					lilGuy.KnockedBack = false;
				}
			}
			wildLilGuys.Clear();
		}
		if (playerLilGuys.Count > 0)
		{
			// Reset all player-owned lil guys
			foreach (LilGuyBase lilGuy in playerLilGuys)
			{
				if (lilGuy != null && lilGuy.PlayerOwner != null)
				{
					lilGuy.PlayerOwner.KnockedBack = false;
				}
			}
			playerLilGuys.Clear();
		}
	}
}
