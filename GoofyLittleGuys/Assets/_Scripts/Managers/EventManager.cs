using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EventManager
{
	public delegate void LilGuyLastHitDelegate(PlayerBody body);
	public event LilGuyLastHitDelegate NotifyLastHit;

	public delegate void UpdatePlayerUIHealthDelegate(PlayerBody body);
	public event UpdatePlayerUIHealthDelegate NotifyPlayerUIHealthUpdate;

	public delegate void LegendarySpawnedDelegate();
	public event LegendarySpawnedDelegate NotifyLegendarySpawned;

	public delegate void ChangePhaseDelegate();
	public event ChangePhaseDelegate NotifyPhaseChanged;

	public delegate void StatsChangeDelegate(LilGuyBase statsToChange);
	public event StatsChangeDelegate NotifyStatsChanged;

	public delegate void GameOverStateDelegate();
	public event GameOverStateDelegate NotifyGameOver;

	public delegate void MicrogameFailedDelegate(PlayerBody body);
	public event MicrogameFailedDelegate NotifyMicrogameFailed;

	public delegate void GameStartedDelegate();
	public event GameStartedDelegate GameStarted;

	public delegate void GamePausedDelegate(PlayerInput playerWhoPaused);
	public event GamePausedDelegate NotifyGamePaused;

	public delegate void StartAbilityCooldownDelegate(PlayerUi playerWhoUsedAbility, float cdLength);
	public event StartAbilityCooldownDelegate NotifyStartAbilityCooldown;

	public delegate void UiSwapDelegate(PlayerUi playerUi, float swapDirection);
	public event UiSwapDelegate NotifyUiSwap;

	private static EventManager _instance = null;
	public static EventManager Instance
	{
		get
		{
			if (_instance == null) _instance = new EventManager();
			return _instance;
		}
	}

	public void HealLilGuy(LilGuyBase lilGuy, int amount)
	{
		if (lilGuy.Health + amount > lilGuy.MaxHealth) lilGuy.Health = lilGuy.MaxHealth;
		else lilGuy.Health += amount;
		lilGuy.PlayHealEffect();
	}
	public void UpdatePlayerHealthUI(PlayerBody body)
	{
		body.PlayerUI.SetPersistentHealthBarValue(body.LilGuyTeam[0].Health, body.LilGuyTeam[0].MaxHealth);
	}
	public void CallGamePaused(PlayerInput playerWhoPaused)
	{
		NotifyGamePaused?.Invoke(playerWhoPaused);
	}
	public void CallLilGuyLockedInEvent()
	{
		LevelLoadManager.Instance.LoadNewLevel("TerrainWhitebox");
		MultiplayerManager.Instance.AdjustCameraRects();
		LevelLoadManager.Instance.StartCoroutine(WaitForLevelToLoad());
	}
	private IEnumerator WaitForLevelToLoad()
	{
		yield return new WaitUntil(() => SceneManager.GetSceneByName("TerrainWhitebox").isLoaded);
		yield return new WaitUntil(() => GameManager.Instance.GameStarted());
		// Once loading is complete, invoke the GameStarted event
		GameStartedEvent();
	}
	public void GameStartedEvent()
	{
		GameStarted?.Invoke();
	}

	public void CallLilGuyLastHitEvent(PlayerBody body)
	{
		NotifyLastHit?.Invoke(body);
	}

	public void CallLegendarySpawnedEvent()
	{
		NotifyLegendarySpawned?.Invoke();
	}

	public void CallPhaseChangedEvent()
	{
		NotifyPhaseChanged?.Invoke();
	}

	public void CallStatsChangedEvent(LilGuyBase statsToChange)
	{
		NotifyStatsChanged?.Invoke(statsToChange);
	}

	public void CallGameOverEvent()
	{
		NotifyGameOver?.Invoke();
	}

	public void CallMicrogameFailedEvent(PlayerBody body)
	{
		NotifyMicrogameFailed?.Invoke(body);
	}

	public void HandleKnockback(Collider other, float knockbackForce, float duration, Vector3 direction, bool isPlayerOwned = false)
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

			// Apply knockback force
			Rigidbody rb = isPlayerOwned ? lilGuy.PlayerOwner.GetComponent<Rigidbody>() : lilGuy.RB;
			if (rb != null)
			{
				// Clear current velocity to prioritize knockback

				// Scale knockback force dynamically based on distance

				// Apply force as impulse
				rb.AddForce(direction * knockbackForce * rb.mass, ForceMode.Impulse);
			}

			GameManager.Instance.StartCoroutine(ResetKnockback(other, duration, isPlayerOwned));
		}
	}

	public IEnumerator ResetKnockback(Collider other, float duration, bool isPlayerOwned = false)
	{
		yield return new WaitForSeconds(duration);
		LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
		if (lilGuy != null)
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
		}
	}

	public void ApplyDebuff(GameObject affectedEntity, float debuffAmount, float debuffDuration, DebuffType type, float damageApplicationInterval = 0)
	{
		switch (type)
		{
			case DebuffType.Slow:
				Slow slowed = affectedEntity.GetComponent<Slow>();
				if (slowed == null)
				{
					slowed = affectedEntity.AddComponent<Slow>();
					slowed.Init(debuffAmount, debuffDuration, damageApplicationInterval);
				}
				else slowed.CurrentDuration = 0;
				break;
			case DebuffType.Poison:
				// Apply Debuff effect to lil guy as well
				Poison poisoned = affectedEntity.GetComponent<Poison>();
				if (poisoned == null)
				{
					poisoned = affectedEntity.AddComponent<Poison>();
					poisoned.Init(debuffAmount, debuffDuration, damageApplicationInterval);
				}					
				else
					poisoned.CurrentDuration = 0;
				break;
		}

	}

	public void ApplySpeedBoost(PlayerBody playerOwner, float speedBoostAmount, float spawnInterval, int maxAfterImages, float fadeSpeed, Color emissionColour, float speedBoostDuration)
	{
		playerOwner.TeamSpeedBoost += speedBoostAmount;
		foreach (LilGuyBase lilGuy in playerOwner.LilGuyTeam)
		{
			if (!lilGuy.isActiveAndEnabled) continue;
			lilGuy.ApplySpeedBoost(spawnInterval, maxAfterImages, fadeSpeed, emissionColour);

		}
		GameManager.Instance.StartCoroutine(StopSpeedBoost(playerOwner, speedBoostAmount, speedBoostDuration));
	}
	public void ApplySpeedBoost(LilGuyBase lilGuy, float speedBoostAmount, float spawnInterval, int maxAfterImages, float fadeSpeed, Color emissionColour, float speedBoostDuration)
	{
		lilGuy.Speed += speedBoostAmount;
		lilGuy.ApplySpeedBoost(spawnInterval, maxAfterImages, fadeSpeed, emissionColour);
		GameManager.Instance.StartCoroutine(StopSpeedBoost(lilGuy, speedBoostAmount, speedBoostDuration));
	}

	private IEnumerator StopSpeedBoost(PlayerBody body, float speedBoostAmount, float speedBoostDuration)
	{
		yield return new WaitForSeconds(speedBoostDuration);

		body.TeamSpeedBoost -= speedBoostAmount;
		foreach (LilGuyBase lilGuy in body.LilGuyTeam)
		{
			lilGuy.RemoveSpeedBoost();
		}

	}
	private IEnumerator StopSpeedBoost(LilGuyBase lilGuy, float speedBoostAmount, float speedBoostDuration)
	{
		yield return new WaitForSeconds(speedBoostDuration);

		lilGuy.Speed -= speedBoostAmount;
		lilGuy.RemoveSpeedBoost();

	}

	public void ApplyTeamDamageReduction(PlayerBody playerOwner, float teamDamageReductionDuration, Turteriam lilGuy)
	{
		playerOwner.TeamDamageReduction += lilGuy.DamageReduction;
		GameManager.Instance.StartCoroutine(StopDamageReduction(playerOwner, teamDamageReductionDuration, lilGuy));
	}

	private IEnumerator StopDamageReduction(PlayerBody playerOwner, float teamDamageReductionDuration, Turteriam lilGuy)
	{
		yield return new WaitForSeconds(teamDamageReductionDuration);
		lilGuy.IsShieldActive = false;
		playerOwner.TeamDamageReduction -= lilGuy.DamageReduction;

		lilGuy.InstantiatedDome = null;
		lilGuy.DamageReductionActive = false;
	}

	public void StartAbilityCooldown(PlayerUi playerUi, float cdLength)
	{
		NotifyStartAbilityCooldown?.Invoke(playerUi, cdLength);
	}

	public void RefreshUi(PlayerUi playerUi, float swapDirection)
	{
		NotifyUiSwap?.Invoke(playerUi, swapDirection);
	}
}
