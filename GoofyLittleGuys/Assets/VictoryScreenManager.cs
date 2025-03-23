using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VictoryScreenManager : MonoBehaviour
{
	public static VictoryScreenManager Instance;

	private HashSet<PlayerInput> readyPlayers = new HashSet<PlayerInput>();
	private List<PlayerInput> allPlayers = new List<PlayerInput>();

	[SerializeField] private VictoryScreen screen;

	private void Awake()
	{
		Instance = this;
	}
	public void UnmarkPlayerReady(PlayerInput input)
	{
		if (readyPlayers.Contains(input))
		{
			readyPlayers.Remove(input);
		}
	}

	public void RegisterPlayers(List<PlayerInput> players)
	{
		allPlayers = players;
	}

	public void MarkPlayerReady(PlayerInput input)
	{
		if (!readyPlayers.Contains(input))
		{
			readyPlayers.Add(input);
		}

		if (readyPlayers.Count >= allPlayers.Count)
		{
			StartCoroutine(FinishVictoryScreen());
		}
	}

	private IEnumerator FinishVictoryScreen()
	{
		yield return new WaitForSeconds(1f); // small delay for polish

		// DESTROY PLAYERS & LOAD MAIN MENU
		for (int i = GameManager.Instance.Players.Count - 1; i >= 0; i--)
		{
			GameManager.Instance.Players[i].InMenu = true;
			Destroy(GameManager.Instance.Players[i].Controller.gameObject);
		}
		GameManager.Instance.Players.Clear();
		GameManager.Instance.Rankings.Clear();
		Time.timeScale = 0;
		GameManager.Instance.IsPaused = false;
		LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
		screen.StopMusic();
		GameManager.Instance.PlayMainMenuMusic();
	}
}
