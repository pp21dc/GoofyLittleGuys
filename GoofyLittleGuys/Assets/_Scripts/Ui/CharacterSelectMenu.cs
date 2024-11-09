using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CharacterSelectMenu : MonoBehaviour
{
	public enum CharacterSelectState
	{
		CharacterSelect,
		LockedIn,
		Confirmed
	}


	[SerializeField] private PlayerInput player;
	[SerializeField] private MultiplayerEventSystem playerEventSystem;
	[SerializeField] private List<Button> buttons;
	public List<LilGuyBase> starters;
	[SerializeField] private PlayerBody body;
	bool lockedIn = false;
	private CharacterSelectState currentState;

	public bool LockedIn { get { return lockedIn; } }


	private void Start()
	{
		player.actions["Cancel"].performed += OnCancelled;
		player.actions["Submit"].performed += OnSubmitted;
		EventManager.Instance.GameStarted += GameInit;
	}

	private void OnDestroy()
	{
		player.actions["Cancel"].performed -= OnCancelled;
		player.actions["Submit"].performed -= OnSubmitted;
		EventManager.Instance.GameStarted -= GameInit;
	}

	void GameInit()
	{
		gameObject.SetActive(false);
	}
	private void OnSubmitted(InputAction.CallbackContext ctx)
	{
		if (currentState == CharacterSelectState.LockedIn && CheckIfValidGameStart())
		{
			EventManager.Instance.CallLilGuyLockedInEvent();
		}
	}
	private void OnCancelled(InputAction.CallbackContext ctx)
	{
		switch (currentState)
		{
			case CharacterSelectState.CharacterSelect:
				foreach (PlayerInput input in PlayerInput.all)
				{
					Destroy(input.gameObject);
				}
				LevelLoadManager.Instance.LoadNewLevel("00_MainMenu");
				break;
			case CharacterSelectState.LockedIn:
				currentState = CharacterSelectState.CharacterSelect;
				lockedIn = false;
				GameObject lilGuyToRemove = body.LilGuyTeam[0].gameObject;
				body.LilGuyTeam.RemoveAt(0);
				Destroy(lilGuyToRemove);
				SetButtonLockState(false);
				playerEventSystem.SetSelectedGameObject(buttons[0].gameObject);
				break;
			case CharacterSelectState.Confirmed:
				currentState = CharacterSelectState.LockedIn;
				break;

		}
	}

	private bool CheckIfValidGameStart()
	{
		if (PlayerInput.all.Count < 2) return false;
		foreach (PlayerInput input in PlayerInput.all)
		{
			if (!input.GetComponentInChildren<CharacterSelectMenu>().LockedIn) return false;
		}
		return true;
	}

	private void SetButtonLockState(bool lockState)
	{
		foreach (Button button in buttons)
		{
			button.interactable = !lockState;
		}
	}
	public void OnLockedIn(int choice)
	{
		lockedIn = true;
		currentState = CharacterSelectState.LockedIn;
		SetButtonLockState(true);
		GameObject starter = Instantiate(starters[choice].gameObject);
		starter.GetComponent<LilGuyBase>().Init(LayerMask.NameToLayer("PlayerLilGuys"));
		starter.transform.SetParent(body.LilGuyTeamSlots[0].transform, false);  // false keeps local position/rotation
		starter.transform.localPosition = Vector3.zero;  // Ensure it’s positioned at the parent
		starter.GetComponent<Rigidbody>().isKinematic = true;
		body.LilGuyTeam.Add(starter.GetComponent<LilGuyBase>());
		body.LilGuyTeam[0].playerOwner = body.gameObject;

	}

	public void OnPlayGameButtonPressed()
	{
		EventManager.Instance.GameStartedEvent();
	}

}
