using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EventManager
{
	public delegate void LilGuyLastHitDelegate(PlayerBody body);
	public event LilGuyLastHitDelegate NotifyLastHit;

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

	public delegate void LilGuyLockedInDelegated();
	public event LilGuyLockedInDelegated NotifyLilGuyLockedIn;

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

	public void CallGamePaused(PlayerInput playerWhoPaused)
	{
		NotifyGamePaused?.Invoke(playerWhoPaused);
	}

	public void CallLilGuyLockedInEvent()
	{
		LevelLoadManager.Instance.LoadNewLevel("02_MainGame");
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


}
