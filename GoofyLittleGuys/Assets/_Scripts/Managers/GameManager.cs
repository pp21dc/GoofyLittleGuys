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

		[SerializeField, Tooltip("Time in minutes that the first phase should end at.")] private float phaseOneStartTime = 7f;   // Length of Phase 1 in seconds. (This amounts to 7 minutes)420f
		[SerializeField] private const float phaseTwoDuration = 180f;   // Length of Phase 2 in seconds. (This amounts to 3 minutes)180f
		[SerializeField] private float currentGameTime = 0;             // Current game time in seconds.
		[SerializeField] private Transform fountainSpawnPoint;          // The spawn point that players are respawned to in the main game, set by the HealingFountain.cs
		[SerializeField] private GameObject timerCanvas;                // The Timer UI displayed in between all the split screeens
		[SerializeField] private TextMeshProUGUI gameTimer;             // The Timer textbox itself
		[SerializeField] private float stormTimer = 20.0f; // how long between spawning new storms in phase 2
		[SerializeField] private AudioSource[] phaseAudioSources; // how long between spawning new storms in phase 2

		[SerializeField] private Material regularLilGuySpriteMat;
		[SerializeField] private Material outlinedLilGuySpriteMat;
		[SerializeField] private Color[] playerColours;
		[SerializeField] private float activeLilGuyScaleFactor = 1.1f;

		private System.TimeSpan gameTime;                                       // To convert from total seconds time to a time in the format mm:ss
		private bool isPaused = false;
		private bool legendarySpawned = false;

		private int currentPhase = 0;
		private bool gameOver = false;
		private float respawnTimer = 5.0f;
		private int activeStorms = 0;

		private List<PlayerBody> players = new List<PlayerBody>(); // for the list of REMAINING players in phase 2
		private List<PlayerBody> rankings = new List<PlayerBody>(); // the phase 2 rankings list, ordered from last place -> first place
		[SerializeField] private List<GameObject> stormSets = new List<GameObject>();
		public int CurrentPhase => currentPhase; // Getter for current phase
		public float RespawnTimer => respawnTimer; // Getter for respawn timer
		public float CurrentGameTime => currentGameTime;

		[Header("Hitbox LayerMasks")]
		[SerializeField] private LayerMask phase1LayerMask;
		[SerializeField] private LayerMask phase2LayerMask;
		private LayerMask currentLayerMask;

		public bool IsPaused { get { return isPaused; } set { isPaused = value; } }
		public Transform FountainSpawnPoint { get { return fountainSpawnPoint; } set { fountainSpawnPoint = value; } }
		public LayerMask CurrentLayerMask { get { return currentLayerMask; } }
		public List<PlayerBody> Players { get { return players; } set { players = value; } }

		public Material RegularLilGuySpriteMat => regularLilGuySpriteMat;
		public Material OutlinedLilGuySpriteMat => outlinedLilGuySpriteMat;
		public float ActiveLilGuyScaleFactor => activeLilGuyScaleFactor;

		public override void Awake()
		{
			base.Awake();
		}

		private void Start()
		{
			Time.timeScale = 0;
			AudioManager.Instance.PlayMusic("GLGMainMenu", phaseAudioSources[0]);
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

				if (currentGameTime >= phaseOneStartTime * 60)
				{
					// Starting Phase 2
					currentPhase++;
					StartPhaseTwo();
				}
				if (currentGameTime >= 300 && !legendarySpawned)
				{
					legendarySpawned = true;
					SpawnLegendary();
				}

			}
			else if (currentPhase == 2)
			{
				if (currentGameTime >= ((phaseOneStartTime * 60f) + phaseTwoDuration))
				{
					//BrawlTimeEnd();
				}

				if (rankings.Count == (players.Count - 1) && !gameOver)
				{
					gameOver = true;
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
			currentGameTime = 0;
			currentPhase = 0;
			for (int i = 0; i < players.Count; i++)
			{
				// We don't want the players all spawning in the same exact spot, so shift their x and z positions randomly.
				int randomPos = Random.Range(0, spawnPoints.Count);
				while (spawnPoints[randomPos].PlayerSpawnedHere)
				{
					randomPos = Random.Range(0, spawnPoints.Count);
				}
				if (!spawnPoints[randomPos].PlayerSpawnedHere)
				{
					players[i].GetComponent<Rigidbody>().MovePosition(spawnPoints[randomPos].transform.position + (new Vector3(1, 0, 1) * Random.Range(-1f, 1f)) + Vector3.up);
					spawnPoints[randomPos].PlayerSpawnedHere = true;
				}
				players[i].InMenu = false;
				players[i].PlayerColour = playerColours[i];
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
			AudioManager.Instance.PlayMusic("GLGPhase1", "GLGPhase2", phaseAudioSources[1], phaseAudioSources[0]);
		}

		/// <summary>
		/// Method that starts phase two.
		/// </summary>
		public void StartPhaseTwo()
		{
			currentLayerMask = phase2LayerMask;

			// Start grand brawl challenge
			GetStormSets();
			StartCoroutine(SpawnStorms());
			StartCoroutine(Crossfade(phaseAudioSources[0], phaseAudioSources[1], 0.5f));

		}
		private IEnumerator Crossfade(AudioSource fromSource, AudioSource toSource, float duration)
		{
			float elapsed = 0.0f;

			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				float t = elapsed / duration;

				// Linearly interpolate volumes
				fromSource.volume = Mathf.Lerp(1.0f, 0.0f, t);
				toSource.volume = Mathf.Lerp(0.0f, 1.0f, t);

				yield return null;
			}

			// Ensure final values
			fromSource.volume = 0.0f;
			fromSource.Stop(); // Stop the previous clip
			toSource.volume = 1.0f;
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
		/// If only one player is left standing, we can simply add them to the rankings list, since they will then be
		/// at the end, making them ranked the best.
		/// </summary>
		public void BrawlKnockoutEnd()
		{
			rankings.Add(players[0]);
			StartCoroutine(nameof(endGame));

			foreach (PlayerBody player in players)
			{
				if (!player.IsDead)
				{
					player.PlayerUI.TempWinText.SetActive(true);
				}
			}

			Debug.Log("Brawl Phase has ended by knockout!");
		}


		//TODO: DELETE THIS AFTER WHITEBOX AS THIS IS A TEMP SOLVE FOR ENDING GAME
		private IEnumerator endGame()
		{
			yield return new WaitForSeconds(6);
			LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
			QuitGame();
			for (int i = Players.Count - 1; i >= 0; i--)
			{
				Players[i].InMenu = true;
				Destroy(Players[i].Controller.gameObject);
			}
			players.Clear();
			currentPhase = 0;
			currentGameTime = 0;
			Time.timeScale = 0;
			gameOver = false;
			StopAllCoroutines(); // this stop the game from fucking destroying itself when restarting TODO: FIND THE FUCKING LEAK
		}

		/// <summary>
		/// Finds all storm sets in the level, and writes them into the list of StormSets
		/// </summary>
		private void GetStormSets()
		{
			GameObject stormParent = GameObject.Find("Storms");
			if (stormParent != null)
			{
				foreach (Transform child in stormParent.transform)
				{
					stormSets.Add(child.gameObject);
				}
				Debug.Log("Storm sets grabbed: " + stormSets.Count);
			}
		}

		/// <summary>
		/// Randomly picks storm sets (sets of storm objects to activate as a group) and 
		/// activates them one by one, until all in the StormSets list have been activated.
		/// </summary>
		/// <returns></returns>
		private IEnumerator SpawnStorms()
		{
			bool stormThisLoop = false;
			while (true)
			{
				bool allStormsActive = true;
				foreach (GameObject storm in stormSets)
				{
					if (!storm.activeSelf)
					{
						allStormsActive = false;
						break;
					}
				}
				if (allStormsActive) break;
				GameObject stormToActivate = RandFromList(stormSets);
				if (stormToActivate != null && !stormToActivate.activeSelf) // activate a new storm
				{
					stormToActivate.SetActive(true);
					stormThisLoop = true;
				}
				if (stormThisLoop) // if there has been a storm in this loop, wait for timer
				{
					stormThisLoop = false;
					yield return new WaitForSeconds(stormTimer);
				}

			}
			yield return new WaitForEndOfFrame();
		}

		/// <summary>
		/// This method accepts a list of objects and returns a random one from that list.
		/// </summary>
		/// <param name="theList"></param>
		/// <returns> GameObject </returns>
		private GameObject RandFromList(List<GameObject> theList)
		{
			return theList[Random.Range(0, theList.Count)];

		}

		public float PhaseOneDurationSeconds()
		{
			return phaseOneStartTime * 60f;
		}
	}

}