using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableBase : MonoBehaviour
{
	protected List<PlayerBody> playersInRange = new List<PlayerBody>();

	[Header("References")]
	[HorizontalRule]
	[SerializeField] private List<GameObject> outlinedObjects;
	[ColoredGroup][SerializeField] protected InteractCanvasController canvasController;
	[ColoredGroup][SerializeField] protected GameObject interactableCanvas;

	[Header("Interact Settings")]
	[HorizontalRule]
	[ColoredGroup] public float requiredHoldDuration = 1f;

	protected Dictionary<PlayerBody, Coroutine> activeHolds = new Dictionary<PlayerBody, Coroutine>();


	protected virtual void OnTriggerStay(Collider other)
	{
		PlayerBody player = other.GetComponent<PlayerBody>();
		if (player == null || player.IsDead) return;

		if (!playersInRange.Contains(player))
			playersInRange.Add(player);

		player.ClosestInteractable = this;

		UpdateInteractCanvases();
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		PlayerBody player = other.GetComponent<PlayerBody>();
		if (player == null) return;

		if (playersInRange.Contains(player))
			playersInRange.Remove(player);

		player.ClosestInteractable = null;

		UpdateInteractCanvases();
	}

	protected virtual void UpdateInteractCanvases()
	{
		if (canvasController == null) return;

		bool[] activeArray = new bool[canvasController.Canvases.Length];
		foreach (var player in playersInRange)
		{
			int index = Mathf.Clamp(player.Controller.PlayerNumber - 1, 0, canvasController.Canvases.Length - 1);
			activeArray[index] = true;
		}

		canvasController.SetCanvasStates(activeArray);
	}

	protected void UpdateLayers(bool makeOutlined = false)
	{
		if (outlinedObjects.Count > 0)
		{
			foreach(GameObject obj in outlinedObjects)
			{
				obj.layer = makeOutlined ? LayerMask.NameToLayer("OutlineObjects") : LayerMask.NameToLayer("Default");
			}
		}
	}

	/// <summary>
	/// Called when a player starts interacting.
	/// </summary>
	public virtual void StartInteraction(PlayerBody body)
	{
		if (requiredHoldDuration <= 0f)
		{
			// If no hold time is required, complete immediately
			CompleteInteraction(body);
			return;
		}
		if (activeHolds.ContainsKey(body)) return; // Prevent duplicate coroutines

		Coroutine holdRoutine = StartCoroutine(HoldInteraction(body));
		activeHolds[body] = holdRoutine;
	}

	/// <summary>
	/// Called when a player stops interacting (releasing the button).
	/// </summary>
	public virtual void CancelInteraction(PlayerBody body)
	{
		if (activeHolds.TryGetValue(body, out Coroutine holdRoutine))
		{
			StopCoroutine(holdRoutine);
			activeHolds.Remove(body);
		}
	}

	protected virtual IEnumerator HoldInteraction(PlayerBody body)
	{
		float holdTime = 0f;
		while (holdTime < requiredHoldDuration)
		{
			holdTime += Time.deltaTime;
			yield return null;
		}

		CompleteInteraction(body);
		activeHolds.Remove(body);
	}

	/// <summary>
	/// Called when the hold duration has been reached.
	/// </summary>
	protected virtual void CompleteInteraction(PlayerBody body)
	{
		Managers.DebugManager.Log($"{body.name} successfully interacted with {gameObject.name}", Managers.DebugManager.DebugCategory.GENERAL);
		// Do whatever the interaction effect should be (e.g., healing, opening doors, etc.)
	}
}
