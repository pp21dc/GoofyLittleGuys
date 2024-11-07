using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterSelectMenu : MonoBehaviour
{
	[SerializeField] private List<Button> buttons;
	public List<LilGuyBase> starters;
	[SerializeField] private PlayerBody body;
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
		foreach (Button button in buttons)
		{
			button.interactable = false;
		}
		GameObject starter = Instantiate(starters[choice].gameObject);
		starter.GetComponent<LilGuyBase>().Init(LayerMask.NameToLayer("PlayerLilGuys"));

		starter.transform.SetParent(body.LilGuyTeamSlots[0].transform, false);  // false keeps local position/rotation
		starter.transform.localPosition = Vector3.zero;  // Ensure it’s positioned at the parent
		starter.GetComponent<Rigidbody>().isKinematic = true;
		body.LilGuyTeam.Add(starter.GetComponent<LilGuyBase>());
		body.LilGuyTeam[0].playerOwner = body.gameObject;
		EventManager.Instance.CallLilGuyLockedInEvent();

	}	

	public void OnPlayGameButtonPressed()
	{
		EventManager.Instance.GameStartedEvent();
	}

}
