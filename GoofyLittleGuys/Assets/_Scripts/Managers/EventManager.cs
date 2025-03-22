using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

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

	public delegate void StormSpawnedDelegate(float dmgToAdd, int numStorms);
	public event StormSpawnedDelegate NotifyStormSpawned;

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

	public void ShowRespawnTimer(PlayerBody body)
	{
		body.PlayerUI.ShowRespawnScreen();
	}
	public void CallGamePaused(PlayerInput playerWhoPaused)
	{
		NotifyGamePaused?.Invoke(playerWhoPaused);
	}
	public void CallLilGuyLockedInEvent()
	{
		if (GameManager.Instance.StartGame) return;
		GameManager.Instance.StartGame = true;
		LevelLoadManager.Instance.LoadNewLevel("TerrainWhitebox");
		MultiplayerManager.Instance.AdjustCameraRects();
		LevelLoadManager.Instance.StartCoroutine(WaitForLevelToLoad());
	}
	
	public void CallTutorialEvent()
	{
		if (GameManager.Instance.StartGame) return;
		LevelLoadManager.Instance.LoadNewLevel("TutorialScene");
		MultiplayerManager.Instance.AdjustCameraRects();

	}
	private IEnumerator WaitForLevelToLoad()
	{
		yield return new WaitUntil(() => SceneManager.GetSceneByName("TerrainWhitebox").isLoaded);
		yield return new WaitUntil(() => GameManager.Instance.GameStarted());
		// Once loading is complete, invoke the GameStarted event
		GameStartedEvent();
	}

	public void CallStormSpawnedEvent(float dmgToAdd, int numStorms)
	{
		NotifyStormSpawned?.Invoke(dmgToAdd, numStorms);
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

	public void ApplyDebuff(GameObject affectedEntity, float debuffAmount, float debuffDuration, BuffType type, object source, float damageApplicationInterval = 0)
	{
		switch (type)
		{
			case BuffType.Slow:
				Slow slow = affectedEntity.GetComponent<Slow>();
				if (slow == null)
					slow = affectedEntity.AddComponent<Slow>();

				slow.ApplySlow(debuffAmount, debuffDuration, source);
				break;

			case BuffType.Poison:
				Poison poisoned = affectedEntity.GetComponent<Poison>();
				if (poisoned == null)
					poisoned = affectedEntity.AddComponent<Poison>();

				poisoned.ApplyPoison(debuffAmount, debuffDuration, damageApplicationInterval, source);
				break;
		}
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
