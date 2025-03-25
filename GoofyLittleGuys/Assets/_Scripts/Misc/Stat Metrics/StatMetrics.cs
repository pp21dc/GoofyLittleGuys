using Managers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Util;

public class StatMetrics : MonoBehaviour
{
	[SerializeField] private PlayerInput player;

	#region Private Variables
	private PlayerBody body;

	private float damageDealt = 0;
	private float damageTaken = 0;
	private float damageReduced = 0;
	private int specialsUsed = 0;
	private int teamWipes = 0;
	private int wildLilGuysDefeated = 0;
	private int deathCount = 0;
	private int swapCount = 0;
	private int berriesEaten = 0;
	private int fountainUses = 0;
	private float distanceTraveled = 0;
	private int lilGuysTamedTotal = 0;
	private bool killedLegendary = false;

	private float lowestHPDuringBattle;
	private bool isInCombat = false;
	private bool survivedWithLowHP = false;

	// Time spent per character in seconds
	private Dictionary<string, float> characterUsage = new Dictionary<string, float>();
	private string currentCharacter;
	private float characterStartTime;

	// Most visited location tracking
	private Dictionary<string, int> locationVisits = new Dictionary<string, int>();
	private string mostVisitedLocation;

	#endregion

	#region Getters & Setters
	public float DamageDealt { get => damageDealt; set => damageDealt = value; }
	public float DamageTaken { get => damageTaken; set => damageTaken = value; }
	public float DamageReduced { get => damageReduced; set => damageReduced = value; }
	public int SpecialsUsed { get => specialsUsed; set => specialsUsed = value; }
	public int TeamWipes { get => teamWipes; set => teamWipes = value; }
	public int WildLilGuysDefeated { get => wildLilGuysDefeated; set => wildLilGuysDefeated = value; }
	public int DeathCount { get => deathCount; set => deathCount = value; }
	public int SwapCount { get => swapCount; set => swapCount = value; }
	public int BerriesEaten { get => berriesEaten; set => berriesEaten = value; }
	public int FountainUses { get => fountainUses; set => fountainUses = value; }
	public float DistanceTraveled { get => distanceTraveled; set => distanceTraveled = value; }
	public int LilGuysTamedTotal { get => lilGuysTamedTotal; set => lilGuysTamedTotal = value; }

	public bool SurvivedWithLowHP { get => survivedWithLowHP; set => survivedWithLowHP = value; }
	public bool KilledLegendary { get => killedLegendary; set => killedLegendary = value; }
	public string CurrentCharacter { get => currentCharacter; set => currentCharacter = value; }
	#endregion

	public List<string> GetTitles(List<StatMetrics> allPlayers)
	{
		List<string> titles = new List<string>();

		if (survivedWithLowHP)
			titles.Add("Tis But a Scratch");

		if (killedLegendary)
			titles.Add("Legendary Slayer");

		if (distanceTraveled >= GetMax(allPlayers, p => p.distanceTraveled))
			titles.Add("True Explorer");

		if (lilGuysTamedTotal > 0 && lilGuysTamedTotal >= GetMax(allPlayers, p => p.lilGuysTamedTotal, true))
			titles.Add("The Befriender");

		if (deathCount == GetMin(allPlayers, p => p.deathCount))
			titles.Add("Not Even Close");

		


		PlayerBody body = GetComponent<PlayerBody>();
		if (body.LilGuyTeam.Count == 1) titles.Add("Juggernaut");
		else if (body.LilGuyTeam.Count == 2) titles.Add("Besties");
		else
		{
			if (body.LilGuyTeam[0].GuyName.Equals("Armordillo") && body.LilGuyTeam[1].GuyName.Equals("Armordillo") && body.LilGuyTeam[2].GuyName.Equals("Armordillo")) titles.Add("Rollout");
			else if (body.LilGuyTeam[0].GuyName.Equals("Teddy") && body.LilGuyTeam[1].GuyName.Equals("Teddy") && body.LilGuyTeam[2].GuyName.Equals("Teddy")) titles.Add("Enguarde");
			else if (body.LilGuyTeam[0].GuyName.Equals("Spricket") && body.LilGuyTeam[1].GuyName.Equals("Spricket") && body.LilGuyTeam[2].GuyName.Equals("Spricket")) titles.Add("Springloaded");
			else if (body.LilGuyTeam[0].GuyName.Equals("Phant-a-phant") && body.LilGuyTeam[1].GuyName.Equals("Phant-a-phant") && body.LilGuyTeam[2].GuyName.Equals("Phant-a-phant")) titles.Add("Phantastic");
			else if (body.LilGuyTeam[0].GuyName.Equals("Toadstool") && body.LilGuyTeam[1].GuyName.Equals("Toadstool") && body.LilGuyTeam[2].GuyName.Equals("Toadstool")) titles.Add("Toxic");
			else if (body.LilGuyTeam[0].GuyName.Equals("Tricera-box") && body.LilGuyTeam[1].GuyName.Equals("Tricera-box") && body.LilGuyTeam[2].GuyName.Equals("Tricera-box")) titles.Add("T.K.O.");
			else if (body.LilGuyTeam[0].GuyName.Equals("Fishbowl") && body.LilGuyTeam[1].GuyName.Equals("Fishbowl") && body.LilGuyTeam[2].GuyName.Equals("Fishbowl")) titles.Add("Tsunami");
			else if (body.LilGuyTeam[0].GuyName.Equals("Turteriam") && body.LilGuyTeam[1].GuyName.Equals("Turteriam") && body.LilGuyTeam[2].GuyName.Equals("Turteriam")) titles.Add("Thunderdome");
			else if (body.LilGuyTeam[0].GuyName.Equals("Mousecar") && body.LilGuyTeam[1].GuyName.Equals("Mousecar") && body.LilGuyTeam[2].GuyName.Equals("Mousecar")) titles.Add("Street Racers");
		}

		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Teddy")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Armordillo")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Spricket"))) titles.Add("The OGs");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Teddy")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Tricera-box")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Fishbowl"))) titles.Add("The Mighty");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Spricket")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Phant-a-phant")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Mousecar"))) titles.Add("The Quick");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Toadstool")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Armordillo")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Turteriam"))) titles.Add("The Wall");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Toadstool")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Mousecar"))) titles.Add("Toxic Tires");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Tricera-box")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Mousecar"))) titles.Add("Mach Punch");

		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Fishbowl")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Toadstool"))) titles.Add("Dome Buddies");
		else if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Fishbowl")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Turteriam"))) titles.Add("Dome Buddies");
		else if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Toadstool")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Turteriam"))) titles.Add("Dome Buddies");

		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Spricket")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Tricera-box"))) titles.Add("Knockback Knockout");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Turteriam")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Tricera-box"))) titles.Add("Cage Fighter");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Phant-a-phant")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Tricera-box"))) titles.Add("Extinct");

		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Teddy")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Armordillo"))) titles.Add("Bread \'n Butter");
		else if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Teddy")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Spricket"))) titles.Add("Bread \'n Butter");

		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Fishbowl")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Armordillo"))) titles.Add("Rolling Waves");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Mousecar")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Phant-a-phant"))) titles.Add("Phantomrider");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Spricket")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Toadstool"))) titles.Add("Leaping Legends");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Teddy")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Phant-a-phant"))) titles.Add("Phantom Flurry");
		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Armordillo")) && body.LilGuyTeam.Exists(lilGuy => lilGuy.GuyName.Equals("Turteriam"))) titles.Add("Defensive Offense");

		if (body.LilGuyTeam.Exists(lilGuy => lilGuy.Type == LilGuyBase.PrimaryType.Strength) && body.LilGuyTeam.Exists(lilGuy => lilGuy.Type == LilGuyBase.PrimaryType.Defense) && body.LilGuyTeam.Exists(lilGuy => lilGuy.Type == LilGuyBase.PrimaryType.Speed)) titles.Add("All-Rounder");

		else if (body.LilGuyTeam.Count == 3) titles.Add("Dream Team");


		return titles;
	}

	// Start is called before the first frame update
	void Start()
	{
		body = GetComponent<PlayerBody>();
	}

	// Update is called once per frame
	void Update()
	{
		if (body.LilGuyTeam.Count > 0)
		{
			if (string.IsNullOrEmpty(currentCharacter)) return;
			if (!characterUsage.ContainsKey(currentCharacter))
			{
				characterUsage[currentCharacter] = 0;
			}
			characterUsage[currentCharacter] += Time.deltaTime;
		}
	}
	/// <summary>
	/// Called when combat starts.
	/// </summary>
	public void StartCombat(float currentHP)
	{
		isInCombat = true;
		lowestHPDuringBattle = currentHP; // Reset tracking
	}

	/// <summary>
	/// Called every frame or on HP change during combat.
	/// </summary>
	public void UpdateHealthDuringCombat(float currentHP)
	{
		if (isInCombat)
		{
			lowestHPDuringBattle = Mathf.Min(lowestHPDuringBattle, currentHP);
		}
	}

	/// <summary>
	/// Called when combat ends.
	/// </summary>
	public void EndCombat(float currentHP)
	{
		isInCombat = false;

		// Check if they survived AND reached <= 10 HP at any point during combat
		if (currentHP > 0 && lowestHPDuringBattle <= 10)
		{
			survivedWithLowHP = true;
		}
	}

	public void ShowMetrics(List<StatMetrics> allPlayers, StatCard card, int playerIndex, int rank)
	{
		string favourite = GetFavoriteCharacter();

		// Most Used Icon
		foreach (LilGuyBase lilguy in GameManager.Instance.LilGuys)
		{
			if (lilguy.name == favourite)
			{
				card.mostUsedIcon.;
				break;
			}
		}

		card.playerNum.text = $"Player {playerIndex + 1}";
		// Background & shape colors
		card.background.color = GameManager.Instance.PlayerColours[playerIndex];
		foreach (Image i in card.playerShapes)
		{
			i.sprite = UiManager.Instance.shapes[playerIndex];
			i.color = GameManager.Instance.PlayerColours[playerIndex];
		}

		card.gameObject.SetActive(true);

		// Titles
		List<string> titles = GetTitles(allPlayers);
		List<string> featuredTitles = new List<string>();

		// Check and add guaranteed titles
		if (titles.Contains("Tis But a Scratch")) featuredTitles.Add("Tis But a Scratch");
		if (titles.Contains("Legendary Slayer")) featuredTitles.Add("Legendary Slayer");

		// Remove guaranteed titles from the pool
		titles.RemoveAll(t => featuredTitles.Contains(t));

		// Shuffle remaining titles manually
		List<string> shuffledRemaining = new List<string>(titles);
		for (int i = 0; i < shuffledRemaining.Count; i++)
		{
			int swapIndex = Random.Range(i, shuffledRemaining.Count);
			(shuffledRemaining[i], shuffledRemaining[swapIndex]) = (shuffledRemaining[swapIndex], shuffledRemaining[i]);
		}

		// Add random titles from shuffled list
		foreach (var title in shuffledRemaining)
		{
			if (featuredTitles.Count >= 3) break;
			featuredTitles.Add(title);
		}


		// Build title display string
		string titleOutput = string.Join("\n", featuredTitles);
		card.titles.text = titleOutput;

		// Rank display
		card.ranking.text = GetRankString(rank);

		// Stats
		string outputMessage = "";
		outputMessage += $"{damageDealt}\n{damageTaken}\n{distanceTraveled}m\n{specialsUsed}\n{teamWipes}\n{wildLilGuysDefeated}\n{deathCount}\n{swapCount}\n{berriesEaten}\n{fountainUses}\n{lilGuysTamedTotal}";
		Managers.DebugManager.Log(outputMessage, Managers.DebugManager.DebugCategory.STAT_METRICS, Managers.DebugManager.LogLevel.LOG);
		card.stats.text = outputMessage;
	}

	private string GetRankString(int rank)
	{
		switch (rank)
		{
			case 0: return "1st";
			case 1: return "2nd";
			case 2: return "3rd";
			case 3: return "4th";
			default: return (rank + 1) + "th";
		}
	}


	private static float GetMax(List<StatMetrics> players, System.Func<StatMetrics, float> selector, bool excludeZero = false)
	{
		float max = float.MinValue;
		foreach (var player in players)
		{
			float value = selector(player);
			if (excludeZero && value == 0) continue; // Skip players with 0 lil guys tamed
			max = Mathf.Max(max, value);
		}
		return max;
	}

	private static float GetMin(List<StatMetrics> players, System.Func<StatMetrics, float> selector)
	{
		float min = float.MaxValue;
		foreach (var player in players)
		{
			min = Mathf.Min(min, selector(player));
		}
		return min;
	}

	public void SwitchCharacter(string newCharacter)
	{
		if (!string.IsNullOrEmpty(currentCharacter))
		{
			// Stop tracking previous character
			characterUsage[currentCharacter] += Time.time - characterStartTime;
		}

		characterStartTime = Time.time;

		if (!characterUsage.ContainsKey(newCharacter))
		{
			characterUsage[newCharacter] = 0;
		}
	}

	public string GetFavoriteCharacter()
	{
		string favorite = null;
		float maxTime = 0;

		foreach (var entry in characterUsage)
		{
			if (entry.Value > maxTime)
			{
				maxTime = entry.Value;
				favorite = entry.Key;
			}
		}

		return favorite;
	}

	public void RegisterLocationVisit(string locationName)
	{
		if (!locationVisits.ContainsKey(locationName))
		{
			locationVisits[locationName] = 0;
		}

		locationVisits[locationName]++;

		if (mostVisitedLocation == null || locationVisits[locationName] > locationVisits[mostVisitedLocation])
		{
			mostVisitedLocation = locationName;
		}
	}

	public string GetMostVisitedLocation()
	{
		return mostVisitedLocation;
	}
}
