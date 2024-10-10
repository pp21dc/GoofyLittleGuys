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
		private int currentPhase = 0;
		private int challengeIndex = -1;    // Index within a list of challenges
		public bool IsPaused { get { return isPaused; } set { isPaused = value; } }

		public override void Awake()
		{
			base.Awake();
			challengeIndex = (int)Random.Range(0, 3);
		}

		public void StartPhaseOne()
		{
			currentPhase++;
		}

		public void StartPhaseTwo()
		{
			switch (challengeIndex)
			{
				case 0:
					// Begin Attack Challenge
					break;
				case 1:
					// Begin Speed Challenge
					break;
				case 2:
					// Begin Defense Challenge
					break;
			}
		}

		public void SpawnLegendary()
		{

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