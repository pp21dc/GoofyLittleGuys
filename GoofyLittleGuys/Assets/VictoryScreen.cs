using Managers;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
	[SerializeField] private StatscreenReferences screenReferences;
	[SerializeField] private AudioSource introSource;
	[SerializeField] private AudioSource musicSource;

	private bool introPlaying = false;

	private void Awake()
	{
		ShowVictoryScreen();

		double startTime = AudioSettings.dspTime + 0.2f; // Slight buffer to ensure sync
		introSource.clip = AudioManager.Instance.GetClip("GLGVictoryIntro");
		musicSource.clip = AudioManager.Instance.GetClip("GLGVictoryScreen");

		introSource.PlayScheduled(startTime);
		musicSource.PlayScheduled(startTime + introSource.clip.length);
		musicSource.loop = true;

		introPlaying = true;
	}


	private void Update()
	{
		if (!introSource.isPlaying && introPlaying)
		{
			AudioManager.Instance.PlayMusic("GLGVictoryScreen", musicSource);
			introPlaying = false;
		}
	}

	public void StopMusic()
	{
		introSource.Stop();
		musicSource.Stop();
	}

	private void ShowVictoryScreen()
	{
		List<PlayerBody> players = GameManager.Instance.Players;
		List<StatMetrics> metrics = new List<StatMetrics>();
		List<PlayerInput> inputs = new List<PlayerInput>();

		for (int i = 0; i < players.Count; i++)
		{
			PlayerBody body = players[i];
			metrics.Add(body.GameplayStats);

			// Find their ranking from GameManager.Instance.rankings
			int rank = GameManager.Instance.Rankings.IndexOf(body); // Lower index = worse rank, higher = better

			int playerIndex = body.Controller.PlayerNumber - 1;
			StatCard card = screenReferences.playerStatObjects[playerIndex];

			card.Initialize(body.Controller.GetComponent<PlayerInput>());
			inputs.Add(body.Controller.GetComponent<PlayerInput>());

			body.GameplayStats.ShowMetrics(metrics, card, playerIndex, players.Count - 1 - rank); // Reverse for proper "1st" logic
		}

		VictoryScreenManager.Instance.RegisterPlayers(inputs);
	}

}
