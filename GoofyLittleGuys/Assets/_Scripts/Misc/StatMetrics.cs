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
	private int fountainUses = 0;
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
	public int FountainUses { get => fountainUses; set => fountainUses = value; }
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

		outputMessage += $"\nStats\nDamage Dealt: {damageDealt}\nDamage Taken: {damageTaken}\nDamage Reduced: {damageReduced}\nSpecials Used: {specialsUsed}\nTeam Wipes: {teamWipes}\nWild Lil Guys Defeated: {wildLilGuysDefeated}\nDeath Count: {deathCount}\nSwap Count: {swapCount}\nBerries Eaten: {berriesEaten}\nFountain Uses: {fountainUses}\nDistance Traveled: {distanceTraveled}m\nLil Guys tamed: {lilGuysTamedTotal}" +
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
