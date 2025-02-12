using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatMetrics : MonoBehaviour
{
	// General metrics
	private float damageDealt = 0;
	private float damageTaken = 0;
	private float damageReduced = 0;
	private int specialsUsed = 0;
	private int teamWipes = 0;
	private int wildLilGuysDefeated = 0;
	private int deathCount = 0;
	private int swapCount = 0;
	private int berriesEaten = 0;
	private float distanceTraveled = 0;
	private int lilGuysTamedTotal = 0;
	private bool killedLegendary = false;

	private float lowestHPDuringBattle;
	private bool isInCombat = false;
	private bool survivedWithLowHP = false;

	public float DamageDealt { get => damageDealt; set => damageDealt = value; }
	public float DamageTaken { get => damageTaken; set => damageTaken = value; }
	public float DamageReduced { get => damageReduced; set => damageReduced = value; }
	public int SpecialsUsed { get => specialsUsed; set => specialsUsed = value; }
	public int TeamWipes { get => teamWipes; set => teamWipes = value; }
	public int WildLilGuysDefeated { get => wildLilGuysDefeated; set => wildLilGuysDefeated = value; }
	public int DeathCount { get => deathCount; set => deathCount = value; }
	public int SwapCount { get => swapCount; set => swapCount = value; }
	public int BerriesEaten { get => berriesEaten; set => berriesEaten = value; }
	public float DistanceTraveled { get => distanceTraveled; set => distanceTraveled = value; }
	public int LilGuysTamedTotal { get => lilGuysTamedTotal; set => lilGuysTamedTotal = value; }

	public bool SurvivedWithLowHP { get => survivedWithLowHP; set => survivedWithLowHP = value; }
	public bool KilledLegendary { get => killedLegendary; set => killedLegendary = value; }
	// Time spent per character in seconds
	private Dictionary<string, float> characterUsage = new Dictionary<string, float>();

	private string currentCharacter;
	public string CurrentCharacter { get => currentCharacter; set => currentCharacter = value; }

	private float characterStartTime;

	// Most visited location tracking
	private Dictionary<string, int> locationVisits = new Dictionary<string, int>();
	private string mostVisitedLocation;

	private PlayerBody body;

	public List<string> GetTitles(List<StatMetrics> allPlayers)
	{
		List<string> titles = new List<string>();

		if (distanceTraveled >= GetMax(allPlayers, p => p.distanceTraveled))
			titles.Add("True Explorer");

		if (killedLegendary)
			titles.Add("Legendary Slayer");

		if (lilGuysTamedTotal > 0 && lilGuysTamedTotal >= GetMax(allPlayers, p => p.lilGuysTamedTotal, true))
			titles.Add("The Befriender");

		if (deathCount == GetMin(allPlayers, p => p.deathCount))
			titles.Add("Not Even Close");

		if (survivedWithLowHP)
			titles.Add("Tis But a Scratch");

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

	public void ShowMetrics(List<StatMetrics> allPlayers)
	{
		string outputMessage = "Titles\n";
		List<string> titles = GetTitles(allPlayers);
		foreach (string title in titles) outputMessage += title + "\n";

		outputMessage += $"\nStats\nDamage Dealt: {damageDealt}\nDamage Taken: {damageTaken}\nDamage Reduced: {damageReduced}\nSpecials Used: {specialsUsed}\nTeam Wipes: {teamWipes}\nWild Lil Guys Defeated: {wildLilGuysDefeated}\nDeath Count: {deathCount}\nSwap Count: {swapCount}\nBerries Eaten: {berriesEaten}\nDistance Traveled: {distanceTraveled}m\nLil Guys tamed: {lilGuysTamedTotal}" +
			$"\nFavourite Lil Guy: {GetFavoriteCharacter()}\nMost Visited Location: {GetMostVisitedLocation()}";
		Debug.Log(outputMessage);
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
