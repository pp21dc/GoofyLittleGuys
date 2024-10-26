using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterSelectMenu : MonoBehaviour
{
	bool lockedIn = false;
	public bool LockedIn { get { return lockedIn; } }

	private void Start()
	{
		EventManager.Instance.GameStarted += GameInit;
	}
	private void OnDestroy()
	{
		EventManager.Instance.GameStarted -= GameInit;
	}
	void GameInit()
	{
		gameObject.SetActive(false);
	}
	public void OnLockedIn(int choice)
	{
		lockedIn = true;
		EventManager.Instance.CallLilGuyLockedInEvent();

	}	
	public void OnPlayGameButtonPressed()
	{
		EventManager.Instance.GameStartedEvent();
	}

}
