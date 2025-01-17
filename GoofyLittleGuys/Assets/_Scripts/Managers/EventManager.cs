using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
		LevelLoadManager.Instance.LoadNewLevel("ForestWhitebox");
		MultiplayerManager.Instance.AdjustCameraRects();
		LevelLoadManager.Instance.StartCoroutine(WaitForLevelToLoad());
	}
	private IEnumerator WaitForLevelToLoad()
	{
		yield return new WaitUntil(() => SceneManager.GetSceneByName("ForestWhitebox").isLoaded);
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


	public void ApplyDebuff(GameObject affectedEntity, float debuffAmount, float debuffDuration, DebuffType type, float damageApplicationInterval = 0)
	{
		switch (type)
		{
			case DebuffType.Slow:
				GameManager.Instance.StartCoroutine(Slow(affectedEntity, debuffAmount, debuffDuration));
				break;
			case DebuffType.Poison:
				// Apply Debuff effect to lil guy as well
				GameManager.Instance.StartCoroutine(Poison(affectedEntity, debuffAmount, debuffDuration, damageApplicationInterval));
				break;
		}

	}
	private IEnumerator Poison(GameObject affectedEntity, float amount, float duration, float damageApplicationInterval)
	{
		float elapsedTime = 0;
		Hurtbox h = affectedEntity.GetComponent<Hurtbox>();
		if (h != null)
		{
			while (elapsedTime <= duration)
			{
				h.TakeDamage(amount);
				yield return new WaitForSeconds(damageApplicationInterval);
				elapsedTime += damageApplicationInterval;
			}
		}
		else yield break;
	}
	private IEnumerator Slow(GameObject affectedEntity, float debuffAmount, float debuffDuration)
	{

		PlayerBody body = affectedEntity.GetComponent<PlayerBody>();
		if (body != null)
		{
			body.MaxSpeed -= debuffAmount;
			yield return new WaitForSeconds(debuffDuration);

			body.MaxSpeed += debuffAmount;
		}
		else
		{
			LilGuyBase lilGuy = affectedEntity.GetComponent<LilGuyBase>();
			lilGuy.Speed -= debuffAmount;
			yield return new WaitForSeconds(debuffDuration);
			lilGuy.Speed += debuffAmount;
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

}
