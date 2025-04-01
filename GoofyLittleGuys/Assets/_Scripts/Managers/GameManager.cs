using System.Collections;
using System.Collections.Generic;
using _Scripts.Misc.ShaderCode;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using Util;
using Unity.VisualScripting;
using System.Linq;

namespace Managers
{
	public class GameManager : SingletonBase<GameManager>
	{		
		private enum TimerState { LegendaryOneApproaching, LegendaryTwoApproaching, LegendaryThreeApproaching, StormApproaching, NextStorm }

		#region Public Variables & Serialize Fields
		[Header("References")]
		[HorizontalRule]
		[SerializeField] private List<PlayerSpawnPoint> spawnPoints;
		[SerializeField] private List<HapticEvent> hapticEvents;
		[SerializeField] private List<LilGuyBase> lilGuys;
		[SerializeField] private List<GameObject> stormSets = new List<GameObject>();
		[SerializeField] private AudioSource[] phaseAudioSources; // how long between spawning new storms in phase 2
		[ColoredGroup][SerializeField] private AudioSource alertAudioSource; // a central audio source for sounds such as alerts, events etc.
		[ColoredGroup][SerializeField] private WaterChangeContainer waterChangeContainer;
		[ColoredGroup][SerializeField] private Animator phase2CloudAnim;
		[ColoredGroup][SerializeField] private Transform fountainSpawnPoint;          // The spawn point that players are respawned to in the main game, set by the HealingFountain.cs
		[ColoredGroup][SerializeField] private Material regularLilGuySpriteMat;
		[ColoredGroup][SerializeField] private Material outlinedLilGuySpriteMat;

		[Header("UI")]
		[HorizontalRule]
		[ColoredGroup][SerializeField] private GameObject timerCanvas;                // The Timer UI displayed in between all the split screeens
		[ColoredGroup][SerializeField] private TMP_Text gameTimer;             // The Timer textbox itself
		[ColoredGroup][SerializeField] private TMP_Text timerContext;             // The Timer textbox itself

		[Header("Phase Settings")]
		[HorizontalRule]
		[ColoredGroup][SerializeField] private TimerState currentTimerState = TimerState.LegendaryOneApproaching;
		[ColoredGroup][SerializeField] private Color waterColour;
		[ColoredGroup][SerializeField] private Color foamColour;
		[ColoredGroup][SerializeField] private Color darkFoamColour;
		[ColoredGroup][SerializeField, DebugOnly] private float currentGameTime = 0;             // Current game time in seconds.
		[ColoredGroup][SerializeField, Tooltip("Time in minutes that the first phase should end at.")] private float phaseOneStartTime = 7f;   // Length of Phase 1 in minutes.

		[Header("Storm Settings")]
		[HorizontalRule]
		[ColoredGroup][SerializeField] private float stormTimer = 20.0f; // how long between spawning new storms in phase 2
		[ColoredGroup][Tooltip("How much a storms damage should increase each time another storm is spawned"), SerializeField] private float stormDmgIncrease = 2f;       // How much a storm 

		[Header("Player Settings")]
		[HorizontalRule]
		[SerializeField] private Color[] playerColours;
		[SerializeField] private Color[] protanopia;
		[SerializeField] private Color[] deuteranopia;
		[SerializeField] private Color[] tritanopia;

		[ColoredGroup][SerializeField] private float activeLilGuyScaleFactor = 1.1f;
		[ColoredGroup][SerializeField] private float nonActiveLilGuyScaleFactor = 0.9f;
		[ColoredGroup][SerializeField] private float teamWipeBonusXpPercentage = 0.2f;

		[Header("Global AI Settings")]
		[HorizontalRule]
		[ColoredGroup][SerializeField] private float wildLilGuyLevelUpdateTick = 5;

		[Header("Legendary Settings")]
		[HorizontalRule]
		[ColoredGroup][SerializeField, Tooltip("Time in minutes that the legendary spawns.")] private float[] legendarySpawnTimes = { 2f, 3f, 4f };   // Legendary spawn time in minutes. 
		[ColoredGroup][SerializeField] private float[] legendaryMaxScales = { 2f, 2.5f, 3f };   // Legendary spawn time in minutes. 
		[ColoredGroup][SerializeField] private int[] legendaryLevels = { 10, 15, 20 };   // Legendary spawn time in minutes. 
		[ColoredGroup][SerializeField, Tooltip("Number of levels subtracted from legendary level to scale legendary XP amount.")] private int legendaryLevelSubtractor = 2;
		[ColoredGroup][SerializeField, Tooltip("Multiplier value to scale legendary XP amount by (leave as 0 if you want no multiplier scaling).")][Min(0)] private float legendaryXpPercentageMultiplier = 0;

		[Header("Leader Settings")]
		[HorizontalRule]
		[ColoredGroup][SerializeField, Tooltip("Game time in seconds that the leader system should start running.")][Min(0)] private float leaderCheckStartTime = 60f; // Time in seconds before checking leader
		[ColoredGroup][SerializeField, Tooltip("The minimum team level lead before someone is considered 'in the lead'.")][Min(0)] private int leaderLevelThreshold = 2; // Minimum lead in team level
		[ColoredGroup][SerializeField, Tooltip("Percentage of bonus xp to award to someone who defeats the leader.")][Min(0)] private float leaderBonusXpPercentage = 0.5f;
		#endregion

		#region Private Variables
		// UI
		private System.TimeSpan gameTime;                                       // To convert from total seconds time to a time in the format mm:ss

		// PLAYER
		private PlayerBody currentLeader;
		private float respawnTimer = 5.0f;

		// STORM
		private int activeStorms = 0;
		private float timeUntilNextStorm = 0.0f;

		// PHASE
		private int currentPhase = 0;

		// GAME
		private Dictionary<string, HapticEvent> hapticsDictionary = new Dictionary<string, HapticEvent>();
		private List<PlayerBody> players = new List<PlayerBody>(); // for the list of REMAINING players in phase 2
		private List<PlayerBody> rankings = new List<PlayerBody>(); // the phase 2 rankings list, ordered from last place -> first place
		private bool gameOver = false;
		private bool startGame = false;
		private bool isPaused = false;

		// LEGENDARY
		private bool[] legendarySpawned = { false, false };
		#endregion

		#region Getters & Setters
		public PlayerBody CurrentLeader { get => currentLeader; set => currentLeader = value; }
		public List<LilGuyBase> LilGuys => lilGuys;
		public List<PlayerBody> Rankings => rankings;
		public List<PlayerBody> Players { get { return players; } set { players = value; } }
		public List<HapticEvent> HapticEvents { get { return hapticEvents; } set { hapticEvents = value; } }
		public Color[] PlayerColours => playerColours;
		public WaterChangeContainer WaterChangeContainer => waterChangeContainer;
		public Transform FountainSpawnPoint { get { return fountainSpawnPoint; } set { fountainSpawnPoint = value; } }
		public Material RegularLilGuySpriteMat => regularLilGuySpriteMat;
		public Material OutlinedLilGuySpriteMat => outlinedLilGuySpriteMat;
		public float ActiveLilGuyScaleFactor => activeLilGuyScaleFactor;
		public float NonActiveLilGuyScaleFactor => nonActiveLilGuyScaleFactor;
		
		public int CurrentPhase => currentPhase; // Getter for current phase
		public float RespawnTimer => respawnTimer; // Getter for respawn timer
		public float CurrentGameTime => currentGameTime;
		public float WildLilGuyLevelUpdateTick => wildLilGuyLevelUpdateTick;
		public float LeaderBonusXpPercentage => leaderBonusXpPercentage;
		public float TeamWipeBonusXpPercentage => teamWipeBonusXpPercentage;
		public bool IsPaused { get { return isPaused; } set { isPaused = value; } }
		public bool StartGame { get => startGame; set => startGame = value; }
		public int LegendaryLevelSubtractor => legendaryLevelSubtractor;
		public float LegendaryXpPercentageMultiplier => legendaryXpPercentageMultiplier;
		public Animator Phase2CloudAnim { set => phase2CloudAnim = value; }
		#endregion

		public override void Awake()
		{
			base.Awake();
			foreach (HapticEvent haptic in hapticEvents)
			{
				if (hapticsDictionary.ContainsKey(haptic.eventName))
				{
					Managers.DebugManager.Log($"{haptic.eventName} already exists in Haptics dictionary. Detected for {haptic.name}. Changing key now", DebugManager.DebugCategory.GENERAL, DebugManager.LogLevel.ERROR);
					haptic.eventName += $"NAME_ERROR";
				}
				hapticsDictionary.Add(haptic.eventName, haptic);
			}
		}

		public void PlayMainMenuMusic()
		{
			AudioManager.Instance.PlayMusic("GLGMainMenu", phaseAudioSources[0]);
		}
		private void LoadSettings()
		{
			SettingsManager.Instance.LoadSettings();
			GameSettings settings = SettingsManager.Instance.GetSettings();
			AudioManager.Instance.SetupMixerVolumes(settings);
		}

		private void Start()
		{
			LoadSettings();
			waterChangeContainer.SwapColors(waterColour, foamColour, darkFoamColour);
			PlayMainMenuMusic();
			Time.timeScale = 0;

			EventManager.Instance.NotifyGameOver += QuitGame;
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnDestroy()
		{
			EventManager.Instance.NotifyGameOver -= QuitGame;
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "00_MainMenu" && players.Count > 0)
			{
				Debug.Log("Clearing players...");
				for (int i = players.Count - 1; i >= 0; i--)
				{
					PlayerBody body = players[i];
					players.RemoveAt(i);
					Destroy(body.transform.parent.gameObject);
				}

				MainMenu menu = FindObjectOfType<MainMenu>();
				if (menu != null)
				{
					menu.MenuEventSystem.SetActive(false);
					CoroutineRunner.Instance.StartCoroutine(EnableMenu(menu));
				}
			}
		}

		private IEnumerator EnableMenu(MainMenu menu)
		{
			yield return new WaitForSecondsRealtime(0.5f);
			menu.MenuEventSystem.SetActive(true);

		}

		public HapticEvent GetHapticEvent(string eventName)
		{
			if (hapticsDictionary.TryGetValue(eventName, out HapticEvent haptic))
			{
				return haptic;
			}

			Managers.DebugManager.Log($"Haptic Event: '{eventName}' not found in the dictionary.", DebugManager.DebugCategory.GENERAL, DebugManager.LogLevel.ERROR);
			return null;
		}
		private void UpdateTimer()
		{
			if (gameTimer == null) return;
			switch (currentTimerState)
			{
				case TimerState.LegendaryOneApproaching:
					timerContext.color = Color.white;
					timerContext.text = "Legendary Approaching";
					gameTimer.color = Color.white;
					gameTime = System.TimeSpan.FromSeconds((legendarySpawnTimes[0] * 60) - currentGameTime);
					gameTimer.text = gameTime.ToString("mm':'ss");
					break;
				case TimerState.LegendaryTwoApproaching:
					timerContext.color = Color.white;
					timerContext.text = "Legendary Approaching";
					gameTimer.color = Color.white;
					gameTime = System.TimeSpan.FromSeconds((legendarySpawnTimes[1] * 60) - currentGameTime);
					gameTimer.text = gameTime.ToString("mm':'ss");
					break;
				case TimerState.StormApproaching:
					timerContext.color = Color.white;
					timerContext.text = "Storm Approaching";
					gameTimer.color = Color.white;
					gameTime = System.TimeSpan.FromSeconds((phaseOneStartTime * 60) - currentGameTime);
					gameTimer.text = gameTime.ToString("mm':'ss");
					break;
				case TimerState.NextStorm:
					timerContext.color = Color.red;
					timerContext.text = "Next Storm Approaching";
					gameTimer.color = Color.red;
					gameTime = System.TimeSpan.FromSeconds((legendarySpawnTimes[1] * 60) - currentGameTime);
					gameTimer.text = $"0:{timeUntilNextStorm.ToString("00")}";
					break;

			}
		}

		private void Update()
		{
			if (currentPhase < 1 || currentPhase > 2) return;

			UpdateTimer();
			if (currentPhase == 1)
			{
				// We are in phase 1, so update the timer accordingly.
				currentGameTime += Time.deltaTime;

				if (currentGameTime >= phaseOneStartTime * 60)
				{
					// Starting Phase 2
					currentPhase++;
					phase2CloudAnim.SetTrigger("Phase2");
					StartPhaseTwo();
				}
				else if (currentGameTime >= legendarySpawnTimes[1] * 60 && !legendarySpawned[1])
				{
					legendarySpawned[1] = true;
					currentTimerState = TimerState.StormApproaching;
					SpawnLegendary(legendaryMaxScales[1], legendaryLevels[1]);
				}
				else if (currentGameTime >= legendarySpawnTimes[0] * 60 && !legendarySpawned[0])
				{
					legendarySpawned[0] = true;
					currentTimerState = TimerState.LegendaryTwoApproaching;
					SpawnLegendary(legendaryMaxScales[0], legendaryLevels[0]);
				}

			}
			else if (currentPhase == 2)
			{
				currentTimerState = TimerState.NextStorm;

				if (rankings.Count >= (players.Count - 1) && !gameOver)
				{
					gameOver = true;
					BrawlKnockoutEnd();
				}
			}

			if (CurrentGameTime >= leaderCheckStartTime)
			{
				CheckLeader();
			}
		}

		/// <summary>
		/// Checks all players and determines who the leader is, if any.
		/// </summary>
		private void CheckLeader()
		{
			PlayerBody newLeader = null;

			foreach (var candidate in players)
			{
				int candidateLevel = 0;
				int candidateXP = 0;

				foreach (var lilGuy in candidate.LilGuyTeam)
				{
					candidateLevel += lilGuy.Level;
					candidateXP += lilGuy.Xp;
				}

				bool qualifies = true;

				foreach (var other in players)
				{
					if (other == candidate) continue;

					int otherLevel = 0;

					foreach (var lilGuy in other.LilGuyTeam)
					{
						otherLevel += lilGuy.Level;
					}

					if (candidateLevel < otherLevel + leaderLevelThreshold)
					{
						qualifies = false;
						break;
					}
				}

				if (!qualifies) continue;

				if (newLeader == null)
				{
					newLeader = candidate;
				}
				else
				{
					// Tie-breaker by total XP
					if (candidateLevel > newLeader.LilGuyTeam.Sum(l => l.Level) ||
						(candidateLevel == newLeader.LilGuyTeam.Sum(l => l.Level) &&
						 candidateXP > newLeader.LilGuyTeam.Sum(l => l.Xp)))
					{
						newLeader = candidate;
					}
				}
			}

			if (newLeader != null && newLeader != currentLeader)
			{
				if (currentLeader != null)
					currentLeader.SetLeader(false); // Disable crown on previous leader

				newLeader.SetLeader(true); // Enable crown on new leader
				currentLeader = newLeader;
			}
		}


		public void QuitGame()
		{
			startGame = false;
			gameOver = false;
			if (currentPhase > 1) phase2CloudAnim.SetTrigger("Revert");
			stormSets.Clear();
			//rankings.Clear();
			for (int i = 0; i < legendarySpawned.Length; i++) { legendarySpawned[i] = false; }
			waterChangeContainer.SwapColors(waterColour, foamColour, darkFoamColour);
			phaseAudioSources[0].volume = 0;
			phaseAudioSources[2].volume = 0;
			phaseAudioSources[3].volume = 0;
			phaseAudioSources[1].volume = 0;
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
			currentTimerState = TimerState.LegendaryOneApproaching;
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
			AudioManager.Instance.PlayMusic("GLGPhase1", "GLGPhase2", phaseAudioSources[1], phaseAudioSources[0]);
			StartCoroutine(InitialUiLoad());
		}

		/// <summary>
		/// Method that starts phase two.
		/// </summary>
		public void StartPhaseTwo()
		{

			// Start grand brawl challenge
			GetStormSets();
			StartCoroutine(FadeWater(4));
			StartCoroutine(SpawnStorms());
			StartCoroutine(Crossfade(phaseAudioSources[0], phaseAudioSources[1], 1f));
			//AudioManager.Instance.PlaySfx("Phase_Change", alertAudioSource);
			AudioManager.Instance.PlaySfx("Wind", phaseAudioSources[2]);
			AudioManager.Instance.PlaySfx("Rain", phaseAudioSources[3]);

			HapticEvent hapticPhase2 = GetHapticEvent("Phase 2");
			if (hapticPhase2 != null)
			{
				for (int i = 0; i < players.Count; i++)
				{
					HapticFeedback.PlayHapticFeedback(players[i].Controller.GetComponent<PlayerInput>(), hapticPhase2.lowFrequency, hapticPhase2.highFrequency, hapticPhase2.duration);
				}
			}
		}

		/// <summary>
		/// Fade between the base water color and blood color
		/// </summary>
		/// <param name="fadeTime">Duration of fade in seconds</param>
		/// <returns></returns>
		private IEnumerator FadeWater(float fadeTime)
		{
			float timeElapsed = 0;
			while (timeElapsed < fadeTime)
			{
				Color c1 = Color.Lerp(waterChangeContainer.baseWaterColor, waterChangeContainer.waterColor, timeElapsed / fadeTime);
				Color c2 = Color.Lerp(waterChangeContainer.baseLightFoamColor, waterChangeContainer.lightFoamColor, timeElapsed / fadeTime);
				Color c3 = Color.Lerp(waterChangeContainer.baseDarkFoamColor, waterChangeContainer.darkFoamColor, timeElapsed / fadeTime);

				waterChangeContainer.SwapColors(c1, c2, c3);

				timeElapsed += Time.deltaTime;
				yield return null;
			}
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
		public void SpawnLegendary(float maxScale, int level)
		{
			AudioManager.Instance.PlaySfx("LegendarySpawned", alertAudioSource);

			HapticEvent legendHaptic = GetHapticEvent("Legendary Spawned");
			if (legendHaptic != null)
			{
				for (int i = 0; i < players.Count; i++)
				{
					HapticFeedback.PlayHapticFeedback(players[i].Controller.GetComponent<PlayerInput>(), legendHaptic.lowFrequency, legendHaptic.highFrequency, legendHaptic.duration);
				}
			}

			if (SpawnManager.Instance != null)
			{
				SpawnManager.Instance.SpawnLegendaryLilGuy(maxScale, level);
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
			Managers.DebugManager.Log("A player has been defeated, and placed into the rankings list!", DebugManager.DebugCategory.GENERAL);
		}

		/// <summary>
		/// When the game ends by timer, we want to determine each remaining player's total team health, and 
		/// add them to Rankings based on that.
		/// </summary>
		public void BrawlTimeEnd()
		{

			Managers.DebugManager.Log("Brawl Phase has ended, DING DING DING!!!", DebugManager.DebugCategory.GENERAL);
		}

		/// <summary>
		/// If only one player is left standing, we can simply add them to the rankings list, since they will then be
		/// at the end, making them ranked the best.
		/// </summary>
		public void BrawlKnockoutEnd()
		{
			//rankings.Add(players[0]);
			StartCoroutine(nameof(endGame));
			List<StatMetrics> metrics = new List<StatMetrics>();
			foreach (PlayerBody player in players)
			{
				if (!rankings.Contains(player)) rankings.Add(player);
			}

			rankings[^1].PlayerUI.VictoryAnimPlay();

			Managers.DebugManager.Log("Brawl Phase has ended by knockout!", DebugManager.DebugCategory.GENERAL);
		}

		private IEnumerator endGame()
		{
			yield return new WaitForSeconds(6);

			waterChangeContainer.SwapColors(
				waterChangeContainer.baseWaterColor,
				waterChangeContainer.baseLightFoamColor,
				waterChangeContainer.baseDarkFoamColor
			);

			EventManager.Instance.CallGameOverEvent();
			LevelLoadManager.Instance.LoadNewLevel("03_VictoryScreen");
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
				Managers.DebugManager.Log("Storm sets grabbed: " + stormSets.Count, DebugManager.DebugCategory.GENERAL);
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
					AudioManager.Instance.PlaySfx("Thunder", alertAudioSource);
					stormToActivate.SetActive(true);
					activeStorms++;
					yield return new WaitForEndOfFrame();
					EventManager.Instance.CallStormSpawnedEvent(stormDmgIncrease, activeStorms);
					stormThisLoop = true;
				}
				if (stormThisLoop) // if there has been a storm in this loop, wait for timer
				{
					stormThisLoop = false;
					timeUntilNextStorm = stormTimer;
					while (timeUntilNextStorm > 0)
					{
						timeUntilNextStorm -= Time.deltaTime;
						yield return null;
					}
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

		public IEnumerator InitialUiLoad()
		{
			yield return new WaitForNextFrameUnit();

			foreach (PlayerBody pb in players)
			{
				EventManager.Instance.RefreshUi(pb.PlayerUI, 0);
			}

		}
	}

}