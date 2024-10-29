using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
	public class GameManager : SingletonBase<GameManager>
	{
		[SerializeField] private const float phaseOneDuration = 7f;
		[SerializeField] private float currentGameTime = 0;

		private bool isPaused = false;
		private bool legendarySpawned = false;

		[SerializeField] private bool gameStartTest = true;
		private int currentPhase = 0;
		public bool IsPaused { get { return isPaused; } set { isPaused = value; } }

		public override void Awake()
		{
			base.Awake();
		}

		private void Start()
		{
			Time.timeScale = 0;
			EventManager.Instance.GameStarted += GameStarted;

			if (gameStartTest) EventManager.Instance.GameStartedEvent();

        }

		private void GameStarted()
		{
			Time.timeScale = 1;
			StartPhaseOne();
		}
		public void StartPhaseOne()
		{
			currentPhase++;
		}

		public void StartPhaseTwo()
		{
			// Start grand brawl challenge
		}

		public void SpawnLegendary()
		{
			Managers.SpawnManager.Instance.SpawnLegendaryGuy();
			legendarySpawned = true;

		}

		private void Update()
		{
			if (currentPhase == 1)
			{
				currentGameTime += Time.deltaTime;

				if (currentGameTime >= phaseOneDuration)
				{
					// Starting Phase 2
					currentPhase++;
					StartPhaseTwo();
				}
				if (currentGameTime >= (phaseOneDuration * 0.5f) && !legendarySpawned)
				{
					// Spawning Legendary
					legendarySpawned = true;
					SpawnLegendary();
				}

			}
		}

	}

}