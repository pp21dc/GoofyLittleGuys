using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackHitbox : MonoBehaviour
{
	[SerializeField] private float knockbackForce = 100f; // Strength of knockback
	[SerializeField] private bool relativeToHitbox = true; // Direction relative to hitbox center
	[SerializeField] private float knockbackDuration = 1f;
	[SerializeField] private float hitstunForce = 0.2f;
	[SerializeField] private float hitstunTime = 1.1f;
	private Vector3 knockbackDir = Vector3.zero;

	private HashSet<LilGuyBase> wildLilGuys = new HashSet<LilGuyBase>(); // Track wild lil guys
	private HashSet<LilGuyBase> playerLilGuys = new HashSet<LilGuyBase>(); // Track player-owned lil guys

	public HashSet<LilGuyBase> PlayerLilGuys => playerLilGuys;
	public HashSet<LilGuyBase> WildLilGuys => wildLilGuys;
	public float KnockbackForce { set { knockbackForce = value; } }
	public float KnockbackDuration { set { knockbackDuration = value; } }
	public Vector3 KnockbackDir { set { knockbackDir = value; } }


	private void OnTriggerEnter(Collider other)
	{
		LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
		if (lilGuy == null) return;

		Managers.DebugManager.Log($"Knockback Hitbox hit {other.gameObject}", Managers.DebugManager.DebugCategory.COMBAT);
		if (other.gameObject.layer == LayerMask.NameToLayer("WildLilGuys"))
		{
			lilGuy.ApplyKnockback((knockbackDir = (knockbackDir == Vector3.zero) ? (other.transform.position - transform.position).normalized : knockbackDir) * knockbackForce);
			Managers.DebugManager.Log("The knockback is: " +  knockbackForce, Managers.DebugManager.DebugCategory.COMBAT);
		}
		else if (other.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
		{
			lilGuy.PlayerOwner.ApplyKnockback((knockbackDir = (knockbackDir == Vector3.zero) ? (other.transform.position - transform.position).normalized : knockbackDir) * knockbackForce);
			//lilGuy.PlayerOwner.StartHitStun(hitstunForce, hitstunTime); comment out for now till its done :)
            Managers.DebugManager.Log("The knockback is: " + knockbackForce, Managers.DebugManager.DebugCategory.COMBAT);
        }
	}

	private void OnTriggerExit(Collider other)
	{
		
	}


	/*
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
	*/
}
