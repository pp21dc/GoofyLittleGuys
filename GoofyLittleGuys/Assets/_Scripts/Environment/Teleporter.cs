using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Teleporter : InteractableBase
{
	#region Public Variables & Serialize Fields
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private Teleporter targetTeleporter;
	[ColoredGroup][SerializeField] private Transform endTeleportLocation;

	[Header("Portal Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private float cooldown;
	#endregion

	#region Private Variables
	private bool onCooldown;
	private BoxCollider teleporterCollider;
	private List<GameObject> inRange = new List<GameObject>();
	#endregion

	#region Getters & Setters
	public Transform EndTeleportLocation { get { return endTeleportLocation; } }
	public bool OnCooldown { get { return onCooldown; } set { onCooldown = value; } }
	#endregion

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
			LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
			if (lilGuy == null) return;
			inRange.Add(other.gameObject);         
           lilGuy.PlayerOwner.ClosestInteractable = this;
        }
    }

    // Using this as a cheaper update loop
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys"))
		{
            if (inRange.Count > 0 && !onCooldown)
            {
                interactableCanvas.SetActive(true);
            }
            else
            {
                interactableCanvas.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerLilGuys") && inRange.Contains(other.gameObject))
        {
			LilGuyBase lilGuy = other.GetComponent<LilGuyBase>();
			if (lilGuy == null) return;
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
		if (!onCooldown)
		{
			body.GetComponent<Rigidbody>().MovePosition(targetTeleporter.EndTeleportLocation.position);
			Managers.DebugManager.Log("TELEPORTED " + body.name + "TO " + targetTeleporter.EndTeleportLocation.position, Managers.DebugManager.DebugCategory.ENVIRONMENT);
			StartCoroutine(nameof(WaitForCooldown));
		}
	}
    private IEnumerator WaitForCooldown()
    {
        yield return new WaitForEndOfFrame();
        onCooldown = true;
        targetTeleporter.OnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
        targetTeleporter.OnCooldown = false;
    }
}
