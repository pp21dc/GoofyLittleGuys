using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterSelectHandler : MonoBehaviour
{
	[SerializeField] private GameObject characterSelectUnit;
	[SerializeField] private GridLayoutGroup gridLayout;

	[SerializeField] private List<GameObject> charSelectUnits;
	[SerializeField] private GameObject tutorialUi;

	private void Awake()
	{
		MultiplayerManager.Instance.CharacterSelectScreen = this;
	}

	public void OnPlayerJoin(PlayerInput input)
	{
		tutorialUi.SetActive(false);
		GameObject menu = Instantiate(characterSelectUnit, gridLayout.transform);
		menu.GetComponent<CharacterSelectMenu>().SetPlayer(input);
		charSelectUnits.Add(menu);

		int playerIndex = GameManager.Instance.Players.Find(b => b == input.GetComponentInChildren<PlayerBody>()).Controller.PlayerNumber;
		DebugManager.Log(playerIndex.ToString());
		// Send haptic feedback only to the new player
		HapticFeedback.PlayJoinHaptics(input, playerIndex);
		UpdateGridLayout();
	}

	public void OnPlayerLeft(PlayerInput input, bool faultyJoin = false)
	{
		for (int i = charSelectUnits.Count - 1; i >= 0; i--)
		{
			if (charSelectUnits[i].GetComponent<CharacterSelectMenu>().Player == input)
			{
				GameObject menuToRemove = charSelectUnits[i];
				GameManager.Instance.Players.RemoveAt(i);
				for (int j = i; j < GameManager.Instance.Players.Count; j++)
				{
					GameManager.Instance.Players[j].Controller.PlayerNumber -= 1;
				}
				charSelectUnits.RemoveAt(i);
				Destroy(menuToRemove);
				Destroy(input.gameObject);
				break;
			}
		}
		UpdateGridLayout();

		if (faultyJoin) return;
		// Send haptic feedback to ALL remaining players since positions shift
		for (int i = 0; i < charSelectUnits.Count; i++)
		{
			PlayerInput player = charSelectUnits[i].GetComponent<CharacterSelectMenu>().Player;
			int playerIndex = GameManager.Instance.Players.Find(b => b == player.GetComponentInChildren<PlayerBody>()).Controller.PlayerNumber;
			DebugManager.Log(playerIndex.ToString());
			HapticFeedback.PlayJoinHaptics(player, playerIndex);
		}
	}

	public void LeaveAllPlayers()
	{
		for(int i = GameManager.Instance.Players.Count - 1; i >= 0; i--)
		{
			//Destroy(charSelectUnits[i].gameObject);
			//Destroy(GameManager.Instance.Players[i].Controller.gameObject);
		}
		//LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
	}

	public bool AllPlayersLockedIn()
	{
		foreach (GameObject menu in charSelectUnits)
		{
			if (!menu.GetComponent<CharacterSelectMenu>().LockedIn) return false;
		}
		return true;
	}
	private void UpdateGridLayout()
	{
		switch (charSelectUnits.Count)
		{
			case 1:
				gridLayout.cellSize = new Vector2(1920, 1080);
				break;
			case 2:
				gridLayout.cellSize = new Vector2(960, 1080);
				break;
			case 3:
				gridLayout.cellSize = new Vector2(960, 540);
				break;
		}
	}
}
