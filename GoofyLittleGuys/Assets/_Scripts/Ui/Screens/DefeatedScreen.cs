using Managers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DefeatedScreen : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private PlayerBody player;
	[ColoredGroup][SerializeField] private TMP_Text context;

	private void OnEnable()
	{
		context.text = $"YOU HAVE BEEN DEFEATED!\nYOU PLACED: {GetRankString()}";
	}

	private string GetRankString()
	{
		int rank = GameManager.Instance.Rankings.IndexOf(player); // Lower index = worse rank, higher = better
		rank = GameManager.Instance.Players.Count - 1 - rank;

		switch (rank)
		{
			case 0: return "1st";
			case 1: return "2nd";
			case 2: return "3rd";
			case 3: return "4th";
			default: return (rank + 1) + "th";
		}
	}
}
