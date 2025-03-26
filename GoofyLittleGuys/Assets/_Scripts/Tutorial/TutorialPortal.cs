using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialPortal : InteractableBase
{
	#region Public Variables & Serialize Fields
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private TutorialPortal targetTeleporter;
	[ColoredGroup][SerializeField] private Transform endTeleportLocation;

	[Header("Portal Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float cooldown;
	#endregion

	#region Private Variables
	private bool onCooldown;
	private TutorialStateMachine tsm;
	private BoxCollider teleporterCollider;
	#endregion

	#region Getters
	public Transform EndTeleportLocation => endTeleportLocation;
	public TutorialStateMachine Tsm { get { return tsm; } set { tsm = value; } }
	public bool OnCooldown
	{
		get => onCooldown;
		set => onCooldown = value;
	}
	#endregion

	#region Unity Events
	private void Start()
	{
		teleporterCollider = GetComponent<BoxCollider>();
		teleporterCollider.isTrigger = true;
	}

	protected override void OnTriggerStay(Collider other)
	{
		LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
		if (lilGuy == null || lilGuy.PlayerOwner == null) return;

		PlayerBody body = lilGuy.PlayerOwner;
		if (body.IsDead) return;

		if (!playersInRange.Contains(body))
			playersInRange.Add(body);

		body.ClosestInteractable = this;

		// Only show canvas if not on cooldown
		if (!onCooldown)
			UpdateInteractCanvases();
	}

	protected override void OnTriggerExit(Collider other)
	{
		LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
		if (lilGuy == null || lilGuy.PlayerOwner == null) return;

		PlayerBody body = lilGuy.PlayerOwner;

		if (playersInRange.Contains(body))
			playersInRange.Remove(body);

		body.ClosestInteractable = null;

		UpdateInteractCanvases(); // Will hide canvases when no players are in range
	}
	#endregion

	#region Interaction Overrides
	public override void StartInteraction(PlayerBody body)
	{
		if (body.IsDead || onCooldown) return;
		base.StartInteraction(body);
	}

	public override void CancelInteraction(PlayerBody body)
	{
		if (body.IsDead) return;
		base.CancelInteraction(body);
	}

	protected override void CompleteInteraction(PlayerBody body)
	{
		if (body.IsDead || onCooldown) return;

		base.CompleteInteraction(body);

		body.GetComponent<Rigidbody>().MovePosition(targetTeleporter.EndTeleportLocation.position);
		Managers.DebugManager.Log("TELEPORTED " + body.name + "TO " + targetTeleporter.EndTeleportLocation.position, Managers.DebugManager.DebugCategory.ENVIRONMENT);

		// exit condition for portal state in the tutorial
		if (tsm.IslandNumber != null)
		{
			TutorialManager.Instance.IslandsComplete[tsm.IslandNumber] = true;
			TutorialManager.Instance.EnableCheckmark(tsm.IslandNumber);
		}
		TutorialManager.Instance.CheckComplete();

	}

	private IEnumerator WaitForCooldown()
	{
		yield return new WaitForEndOfFrame();
		onCooldown = true;
		targetTeleporter.OnCooldown = true;

		yield return new WaitForSeconds(cooldown);

		onCooldown = false;
		targetTeleporter.OnCooldown = false;

		// Refresh canvases if anyone is still in range
		UpdateInteractCanvases();
	}
	#endregion

	#region Canvas Logic
	protected override void UpdateInteractCanvases()
	{
		if (canvasController == null) return;

		if (onCooldown)
		{
			canvasController.SetCanvasStates(new bool[canvasController.Canvases.Length]); // disable all
			return;
		}

		base.UpdateInteractCanvases(); // Enables only correct players
	}
	#endregion
}
