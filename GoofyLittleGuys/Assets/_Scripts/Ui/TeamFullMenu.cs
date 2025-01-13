using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class TeamFullMenu : MonoBehaviour
{
	[SerializeField] private PlayerInput player;                         // Reference to the player owner of this menu.
	[SerializeField] private List<Button> buttons;                        // The first button to be selected on default.
	[SerializeField] private MultiplayerEventSystem playerEventSystem;  // Reference to the player's event system.

	private LilGuyBase lilGuyBeingCaught;                                 // Thi lil guy we are trying to capture... or not.
	private PlayerBody body;

	private void OnEnable()
	{
		player.SwitchCurrentActionMap("UI");             // Switch to UI

		// Set the first selected button to "Yes"
		playerEventSystem.gameObject.SetActive(true);
		playerEventSystem.firstSelectedGameObject = buttons[0].gameObject;
		playerEventSystem.SetSelectedGameObject(buttons[0].gameObject);

		body = player.GetComponent<PlayerController>().Body;
		body.SetInvincible(-1);

		for (int i = 0; i < buttons.Count; i++)
		{
			TextMeshProUGUI label = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
			if (label != null)
			{
				label.text = body.LilGuyTeam[i].GuyName;
			}
		}
	}

	private void OnDisable()
	{
		player.SwitchCurrentActionMap("World");             // Switch back to world action map
		body.SetInvincible(0);
	}

	/// <summary>
	/// Method called on initialization of this menu. 
	/// </summary>
	/// <param name="lilGuy">The lil guy being caught.</param>
	public void Init(LilGuyBase lilGuy)
	{
		lilGuyBeingCaught = lilGuy;
	}

	/// <summary>
	/// Method fired when one of the choices in the menu is chosen.
	/// </summary>
	/// <param name="choice">Which team index the player has chosen to remove from their team.</param>
	public void OnLilGuyChosenToRelease(int choice)
	{
		LilGuyBase lilGuyBeingReleased = body.LilGuyTeam[choice];

		body.LilGuyTeam[choice] = lilGuyBeingCaught;

		lilGuyBeingCaught.PlayerOwner = body;
		lilGuyBeingCaught.Health = lilGuyBeingCaught.MaxHealth;

		// Setting layer to Player Lil Guys, and putting the lil guy into the first empty slot available.
		lilGuyBeingCaught.gameObject.transform.SetParent(body.transform, true);
		lilGuyBeingCaught.gameObject.GetComponent<Rigidbody>().isKinematic = (choice == 0);
		lilGuyBeingCaught.SetLayer((choice == 0) ? LayerMask.NameToLayer("PlayerLilGuys") : LayerMask.NameToLayer("Player"));
		lilGuyBeingCaught.gameObject.transform.localPosition = Vector3.zero;
		lilGuyBeingCaught.SetFollowGoal(body.LilGuyTeamSlots[choice].transform);

		// Remove the lil guy being released.
		Destroy(lilGuyBeingReleased.gameObject);

		// Leave the menu.
		gameObject.SetActive(false);
	}
}
