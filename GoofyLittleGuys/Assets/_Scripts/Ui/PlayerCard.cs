using Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
	private CharacterSelectState currentState = CharacterSelectState.Disconnected;
	[SerializeField] private GameObject[] playerCardStates;


	[SerializeField] private Image[] playerColourIndicators;
	[SerializeField] private Image[] playerShapeIndicators;

	private void Awake()
	{
		UpdateState(currentState);
	}
	public void UpdateState(CharacterSelectState state, int playerNum = -1)
	{
		currentState = state;
		GetComponent<Image>().color = currentState == CharacterSelectState.Disconnected ? Color.gray : GameManager.Instance.PlayerColours[playerNum];
		if (currentState != CharacterSelectState.Disconnected)
		{
			foreach (Image image in playerColourIndicators)
			{
				image.color = GameManager.Instance.PlayerColours[playerNum];
			}
			foreach (Image image in playerShapeIndicators)
			{
				image.color = GameManager.Instance.PlayerColours[playerNum];
				image.sprite = UiManager.Instance.shapes[playerNum];
			}
		}
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		for (int i = 0; i < playerCardStates.Length; i++)
		{
			playerCardStates[i].SetActive(i == (int)currentState ? true : false);
		}

	}
}
