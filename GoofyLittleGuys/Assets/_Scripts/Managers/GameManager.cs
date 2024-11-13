using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

namespace Managers
{
	public class GameManager : SingletonBase<GameManager>
	{
		[SerializeField] private bool gameStartTest = true;             // TEST BOOL FOR DESIGNERS TO PLAY THE GAME WITHOUT GOING INTO PERSISTENT ALL THE TIME

		[SerializeField] private Vector3 spawnPosition;

		[SerializeField] private const float phaseOneDuration = 420f;   // Length of Phase 1 in seconds. (This amounts to 7 minutes)
		[SerializeField] private float currentGameTime = 0;             // Current game time in seconds.
		[SerializeField] private Transform fountainSpawnPoint;          // The spawn point that players are respawned to in the main game, set by the HealingFountain.cs
		[SerializeField] private GameObject timerCanvas;                // The Timer UI displayed in between all the split screeens
		[SerializeField] private TextMeshProUGUI gameTimer;             // The Timer textbox itself

		private System.TimeSpan gameTime;                                       // To convert from total seconds time to a time in the format mm:ss
		private bool isPaused = false;
		private bool legendarySpawned = false;

		private int currentPhase = 0;
		public int CurrentPhase => currentPhase;                        // Getter for current phase

		[Header("Hitbox LayerMasks")]
		[SerializeField] private LayerMask phase1LayerMask;
		[SerializeField] private LayerMask phase2LayerMask;
		private LayerMask currentLayerMask;

		public bool IsPaused { get { return isPaused; } set { isPaused = value; } }
		public Transform FountainSpawnPoint { get { return fountainSpawnPoint; } set { fountainSpawnPoint = value; } }
		public LayerMask CurrentLayerMask { get { return currentLayerMask; } }

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

		private void Update()
		{
			if (currentPhase == 1)
			{
				// We are in phase 1, so update the timer accordingly.
				currentGameTime += Time.deltaTime;
				gameTime = System.TimeSpan.FromSeconds(currentGameTime);
				if (gameTimer != null) gameTimer.text = gameTime.ToString("mm':'ss");

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

		/// <summary>
		/// Method that is called when the game transitions from Character Select to Main Game.
		/// </summary>
		private void GameStarted()
		{
			foreach (PlayerInput input in PlayerInput.all)
			{
				// We don't want the players all spawning in the same exact spot, so shift their x and z positions randomly.
				input.gameObject.transform.position = spawnPosition + (new Vector3(1, 0, 1) * Random.Range(-1f, 1f)) + Vector3.up;
			}

			// Unpause time, and begin phase one!
			Time.timeScale = 1;
			StartPhaseOne();
			if (timerCanvas != null) timerCanvas.SetActive(true);	// Show the timer canvas if one exists.
		}

		/// <summary>
		/// Method that starts phase one.
		/// </summary>
		public void StartPhaseOne()
		{
			currentPhase++;
			currentLayerMask = phase1LayerMask;
		}

		/// <summary>
		/// Method that starts phase two.
		/// </summary>
		public void StartPhaseTwo()
		{
			currentLayerMask = phase2LayerMask;

			// Start grand brawl challenge
			LevelLoadManager.Instance.LoadNewLevel("03_PhaseTwo");
		}

		/// <summary>
		/// Method to call when it's time to spawn a legendary lil guy
		/// </summary>
		public void SpawnLegendary()
		{
			if (SpawnManager.Instance != null)
			{
				SpawnManager.Instance.SpawnLegendaryGuy();
			}
		}
	}
}