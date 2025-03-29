using Managers;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;

public class VictoryScreen : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private StatscreenReferences screenReferences;
	[ColoredGroup][SerializeField] private AudioSource introSource;
	[ColoredGroup][SerializeField] private AudioSource musicSource;

	private bool introPlaying = false;

	private void Awake()
	{
		ShowVictoryScreen();
		StartCoroutine(AnimateStatCards());

		double startTime = AudioSettings.dspTime + 0.2f; // Slight buffer to ensure sync
		introSource.clip = AudioManager.Instance.GetClip("GLGVictoryIntro");
		musicSource.clip = AudioManager.Instance.GetClip("GLGVictoryScreen");

		introSource.PlayScheduled(startTime);
		musicSource.PlayScheduled(startTime + introSource.clip.length);
		musicSource.loop = true;

		introPlaying = true;
	}

	private IEnumerator AnimateStatCards()
	{
		List<PlayerBody> rankedPlayers = GameManager.Instance.Rankings; // Rankings[0] = last place, Rankings[^1] = winner
		List<StatCardAnimator> animators = new List<StatCardAnimator>();

		for (int i = 0; i < rankedPlayers.Count; i++) // Animate from last to first
		{
			PlayerBody body = rankedPlayers[i];
			int playerIndex = body.Controller.PlayerNumber - 1;
			StatCard card = screenReferences.playerStatObjects[playerIndex];
			bool isWinner = (i == rankedPlayers.Count - 1); // Last in the list = winner

			StatCardAnimator animator = card.GetComponent<StatCardAnimator>();
			if (animator != null)
			{
				animators.Add(animator);
				animator.BeginScaleIn(i * 0.5f, isWinner);
				yield return new WaitForSeconds(0.5f);
			}
		}

		yield return new WaitForSeconds(0.5f); // Final buffer before post-effects

		foreach (var anim in animators)
		{
			anim.TriggerPostScaleEffects();
		}
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
			players[i].Controller.PlayerCam.gameObject.SetActive(false);
			PlayerBody body = players[i];
			inputs.Add(body.Controller.GetComponent<PlayerInput>());
			metrics.Add(body.GameplayStats);

		}

		for (int i = 0; i < players.Count; i++)
		{
			PlayerBody body = players[i];
			// Find their ranking from GameManager.Instance.rankings
			int rank = GameManager.Instance.Rankings.IndexOf(body); // Lower index = worse rank, higher = better

			int playerIndex = body.Controller.PlayerNumber - 1;
			StatCard card = screenReferences.playerStatObjects[playerIndex];

			card.Initialize(body.Controller.GetComponent<PlayerInput>());
			body.GameplayStats.ShowMetrics(metrics, card, playerIndex, players.Count - 1 - rank); // Reverse for proper "1st" logic
		}

		VictoryScreenManager.Instance.RegisterPlayers(inputs);
	}

}
