using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using UnityEngine.SceneManagement;

namespace Managers
{
	public class GameManager : SingletonBase<GameManager>
	{
		[SerializeField] private const float phaseOneDuration = 420f;
		[SerializeField] private float currentGameTime = 0;
		[SerializeField] private Transform fountainSpawnPoint;
		[SerializeField] private GameObject timerCanvas;

		private TimeSpan gameTime;

		[SerializeField] private TextMeshProUGUI gameTimer;

		private bool isPaused = false;
		private bool legendarySpawned = false;

		[SerializeField] private bool gameStartTest = true;
		private int currentPhase = 0;

		public int CurrentPhase => currentPhase;

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

		private void GameStarted()
		{
			foreach(PlayerInput input in PlayerInput.all)
			{
				input.gameObject.transform.position += (new Vector3(1, 0, 1) * UnityEngine.Random.Range(-1f, 1f)) + Vector3.up;
			}
			Time.timeScale = 1;
			StartPhaseOne();
			if (SceneManager.GetActiveScene().name == "02_MainGame")
				fountainSpawnPoint = FindFirstObjectByType<HealingFountain>().transform;

			if (timerCanvas != null) timerCanvas.SetActive(true);
		}
		public void StartPhaseOne()
		{
			currentPhase++;
			currentLayerMask = phase1LayerMask;
		}

		public void StartPhaseTwo()
		{
			currentLayerMask = phase2LayerMask;
			// Start grand brawl challenge
			LevelLoadManager.Instance.LoadNewLevel("03_PhaseTwo");
		}

		public void SpawnLegendary()
		{
			if (SpawnManager.Instance != null)
            {
				SpawnManager.Instance.SpawnLegendaryGuy();
			}
		}

		private void Update()
		{
			if (currentPhase == 1)
			{
				currentGameTime += Time.deltaTime;
				gameTime = TimeSpan.FromSeconds(currentGameTime);
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

	}

}