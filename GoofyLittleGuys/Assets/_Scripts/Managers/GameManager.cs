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

		[SerializeField] private List<PlayerSpawnPoint> spawnPoints;

		[SerializeField] private const float phaseOneDuration = 420f;   // Length of Phase 1 in seconds. (This amounts to 7 minutes)420f
		[SerializeField] private const float phaseTwoDuration = 180f;   // Length of Phase 2 in seconds. (This amounts to 3 minutes)180f
		[SerializeField] private float currentGameTime = 0;             // Current game time in seconds.
		[SerializeField] private Transform fountainSpawnPoint;          // The spawn point that players are respawned to in the main game, set by the HealingFountain.cs
		[SerializeField] private GameObject timerCanvas;                // The Timer UI displayed in between all the split screeens
		[SerializeField] private TextMeshProUGUI gameTimer;             // The Timer textbox itself

		private System.TimeSpan gameTime;                                       // To convert from total seconds time to a time in the format mm:ss
		private bool isPaused = false;
		private bool legendarySpawned = false;

		private int currentPhase = 0;
		private float respawnTimer = 5.0f;

		private List<PlayerBody> players = new List<PlayerBody>(); // for the list of REMAINING players in phase 2
		private List<PlayerBody> rankings = new List<PlayerBody>(); // the phase 2 rankings list, ordered from last place -> first place
		public int CurrentPhase => currentPhase; // Getter for current phase
		public float RespawnTimer => respawnTimer; // Getter for respawn timer

		[Header("Hitbox LayerMasks")]
		[SerializeField] private LayerMask phase1LayerMask;
		[SerializeField] private LayerMask phase2LayerMask;
		private LayerMask currentLayerMask;

		public bool IsPaused { get { return isPaused; } set { isPaused = value; } }
		public Transform FountainSpawnPoint { get { return fountainSpawnPoint; } set { fountainSpawnPoint = value; } }
		public LayerMask CurrentLayerMask { get { return currentLayerMask; } }
		public List<PlayerBody> Players { get { return players; } set { players = value; } }


		public override void Awake()
		{
			base.Awake();
		}

		private void Start()
		{
			Time.timeScale = 0;
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
			else if (currentPhase == 2)
			{
				if (currentGameTime >= (phaseOneDuration + phaseTwoDuration))
				{
					BrawlTimeEnd();
				}

				if (players != null)
				{
					//MonitorPlayerDefeats();
				}
				else if (rankings.Count == players.Count - 1)
				{
					BrawlKnockoutEnd();
				}
			}
		}

		public void QuitGame()
		{
			currentPhase = 0;
			currentGameTime = 0;
			timerCanvas.SetActive(false);
			foreach (PlayerSpawnPoint point in spawnPoints)
			{
				point.PlayerSpawnedHere = false;
			}
		}

		/// <summary>
		/// Method that is called when the game transitions from Character Select to Main Game.
		/// </summary>
		public bool GameStarted()
		{
			foreach (PlayerBody body in players)
			{
				// We don't want the players all spawning in the same exact spot, so shift their x and z positions randomly.
				int randomPos = Random.Range(0, spawnPoints.Count);
				while (spawnPoints[randomPos].PlayerSpawnedHere)
				{
					randomPos = Random.Range(0, spawnPoints.Count);
				}
				if (!spawnPoints[randomPos].PlayerSpawnedHere)
				{
					Debug.Log(spawnPoints[randomPos].transform.position);
					body.GetComponent<Rigidbody>().MovePosition(spawnPoints[randomPos].transform.position + (new Vector3(1, 0, 1) * Random.Range(-1f, 1f)) + Vector3.up);
					spawnPoints[randomPos].PlayerSpawnedHere = true;
				}
			}

			// Unpause time, and begin phase one!
			Time.timeScale = 1;
			StartPhaseOne();
			if (timerCanvas != null) timerCanvas.SetActive(true);   // Show the timer canvas if one exists.

			return true;
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


			for (int i = 0; i < players.Count; i++)
			{
				PlayerBody thisPlayer = players[i];
				if (thisPlayer != null)
				{
					thisPlayer.CanRespawn = false;
				}
			}

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


		/// <summary>
		/// Takes a player that got defeated, and adds them to the end of the
		/// rankings list, meaning rankings is ordered from worst to best.
		/// </summary>
		/// <param name="defeatedPlayer"></param>
		public void PlayerDefeat(PlayerBody defeatedPlayer)
		{
			rankings.Add(defeatedPlayer);
			Debug.Log("A player has been defeated, and placed into the rankings list!");
		}

		/// <summary>
		/// When the game ends by timer, we want to determine each remaining player's total team health, and 
		/// add them to Rankings based on that.
		/// </summary>
		public void BrawlTimeEnd()
		{

			Debug.Log("Brawl Phase has ended, DING DING DING!!!");
		}

		

		/// <summary>
		/// Just checks if the given player has any Lil Guys left
		/// </summary>
		/// <param name="thePlayer"></param>
		/// <returns></returns>
		private bool IsAlive(PlayerBody thePlayer)
		{
			return thePlayer.CheckTeamHealth();

		}

		/// <summary>
		/// If only one player is left standing, we can simply add them to the rankings list, since they will then be
		/// at the end, making them ranked the best.
		/// </summary>
		public void BrawlKnockoutEnd()
		{
			rankings.Add(players[0]);
			Debug.Log("Brawl Phase has ended by knockout!");
		}

	}
}