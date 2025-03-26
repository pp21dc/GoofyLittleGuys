using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPortal : InteractableBase
{
	// -- Variables --
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private TutorialPortal targetTeleporter;
	[ColoredGroup][SerializeField] private Transform endTeleportLocation;

	private TutorialStateMachine tsm;
    private BoxCollider teleporterCollider;
    private List<GameObject> inRange = new List<GameObject>();
    
    public Transform EndTeleportLocation { get { return endTeleportLocation; } }
    public TutorialStateMachine Tsm { get { return tsm; } set { tsm = value; } }

    #region Event Functions
    private void Start()
    {
        teleporterCollider = GetComponent<BoxCollider>();
        teleporterCollider.isTrigger = true;
        interactableCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys") && !inRange.Contains(other.gameObject))
		{
			var lilGuy = other.GetComponent<LilGuyBase>();
			if (!lilGuy) return;
			inRange.Add(other.gameObject);
            lilGuy.PlayerOwner.ClosestInteractable = this;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
	    if (other.gameObject.layer != LayerMask.NameToLayer("PlayerLilGuys")) return;

	    interactableCanvas.SetActive(inRange.Count > 0);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys") && inRange.Contains(other.gameObject))
        {
			LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
			if (!lilGuy) return;
			if (inRange.Contains(lilGuy.gameObject)) inRange.Remove(lilGuy.gameObject);
			lilGuy.PlayerOwner.ClosestInteractable = null;
		}
        if (inRange.Count == 0)
        {
            interactableCanvas.SetActive(false);
        }
    }
	#endregion

	/// <summary>
	/// Called when a player starts interacting.
	/// </summary>
	public override void StartInteraction(PlayerBody body)
	{
		if (body.IsDead) return;
		base.StartInteraction(body);
	}

	/// <summary>
	/// Called when a player stops interacting (releasing the button).
	/// </summary>
	public override void CancelInteraction(PlayerBody body)
	{
		if (body.IsDead) return;
		base.CancelInteraction(body);
	}

	/// <summary>
	/// Called when a player interacts with this interactable object.
	/// </summary>
	/// <param name="body">PlayerBody: The player that interacted with this object.</param>
	protected override void CompleteInteraction(PlayerBody body)
	{
		if (body.IsDead) return;
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
}