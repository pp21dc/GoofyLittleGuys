using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using Managers;
using UnityEngine.EventSystems;
using UnityEditor.Rendering.Universal.ShaderGUI;

public class TeamFullMenu : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	[SerializeField] private List<Button> buttons;                        // The first button to be selected on default.
	[SerializeField] private List<Animator> icons;
	[ColoredGroup][SerializeField] private PlayerInput player;                         // Reference to the player owner of this menu.
	[ColoredGroup][SerializeField] private MultiplayerEventSystem playerEventSystem;  // Reference to the player's event system.
	[ColoredGroup][SerializeField] private AnimatorOverrideController aocTemplate;


	private LilGuyBase lilGuyBeingCaught;                                 // Thi lil guy we are trying to capture... or not.
	private PlayerBody body;

	private void OnEnable()
	{
		player.SwitchCurrentActionMap("UI");             // Switch to UI

		// Set the first selected button to "Yes"
		playerEventSystem.gameObject.SetActive(true);
		playerEventSystem.firstSelectedGameObject = buttons[0].gameObject;
		playerEventSystem.SetSelectedGameObject(buttons[0].gameObject);

		player.GetComponent<PlayerController>().InTeamFullMenu = true;

		body = player.GetComponent<PlayerController>().Body;
		body.SetInvincible(-1);

		for (int i = 0; i < buttons.Count - 1; i++)
		{
			buttons[i].interactable = true;

			Animator anim = buttons[i].GetComponent<Animator>();
			if (anim != null)
			{
				anim.Rebind(); // Resets to default values from the AnimatorController
				anim.Update(0f); // Forces the update to happen immediately
			}

			TextMeshProUGUI label = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
			if (label != null)
			{
				label.text = "Lvl: " + body.LilGuyTeam[i].Level + "\n HP: " + body.LilGuyTeam[i].Health;
				AnimatorOverrideController aoc = new AnimatorOverrideController(aocTemplate);
				aoc["Idle"] = body.LilGuyTeam[i].UiAnimation;
				icons[i].runtimeAnimatorController = aoc;
			}
		}
	}

	private void OnDisable()
	{
		body = player.GetComponentInChildren<PlayerBody>();
		player.GetComponent<PlayerController>().InTeamFullMenu = false;
		player.SwitchCurrentActionMap("World");             // Switch back to world action map
															// Set the first selected button to "Yes"
		playerEventSystem.gameObject.SetActive(false);
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
		EventSystem.current.SetSelectedGameObject(null);
		foreach (Button button in buttons)
		{
			button.interactable = false;
			Animator anim = button.GetComponent<Animator>();
			if (anim != null)
			{
				anim.Rebind(); // Resets to default values from the AnimatorController
				anim.Update(0f); // Forces the update to happen immediately
			}
		}

		if (choice == 3)
		{
			buttons[3].interactable = false;
			Destroy(lilGuyBeingCaught.gameObject);
			body.GameplayStats.LilGuysTamedTotal--;
			body.PlayerUI.SetBerryCount(body.BerryCount);
			gameObject.SetActive(false);
			buttons[3].interactable = true;
			return;
		}
		LilGuyBase lilGuyBeingReleased = body.LilGuyTeam[choice];

		body.LilGuyTeam[choice] = lilGuyBeingCaught;

		lilGuyBeingCaught.PlayerOwner = body;
		lilGuyBeingCaught.Health = lilGuyBeingCaught.MaxHealth;
		lilGuyBeingCaught.PlaySound("Tamed_Lil_Guy");

		// Setting layer to Player Lil Guys, and putting the lil guy into the first empty slot available.
		lilGuyBeingCaught.gameObject.transform.SetParent(body.transform, true);
		lilGuyBeingCaught.gameObject.GetComponent<Rigidbody>().isKinematic = (choice == 0);
		lilGuyBeingCaught.SetLayer((choice == 0) ? LayerMask.NameToLayer("PlayerLilGuys") : LayerMask.NameToLayer("Player"));
		lilGuyBeingCaught.SetMaterial((choice != 0) ? GameManager.Instance.RegularLilGuySpriteMat : GameManager.Instance.OutlinedLilGuySpriteMat);
		lilGuyBeingCaught.gameObject.transform.localPosition = Vector3.zero;
		lilGuyBeingCaught.SetFollowGoal(body.LilGuyTeamSlots[choice].transform);

		if (choice == 0)
		{
			body.SetActiveLilGuy(lilGuyBeingCaught);
			body.PlayerUI.SetPersistentHealthBarValue(lilGuyBeingCaught.Health, lilGuyBeingCaught.MaxHealth);
			body.PlayerUI.ResetCDTimer();
		}

		EventManager.Instance.RefreshUi(body.PlayerUI, 0);
		// Remove the lil guy being released.
		Destroy(lilGuyBeingReleased.gameObject);

		// Leave the menu.
		gameObject.SetActive(false);
	}
}
