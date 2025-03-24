using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableBase : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	[SerializeField] private List<GameObject> outlinedObjects;
	[ColoredGroup][SerializeField] protected GameObject interactableCanvas;

	[Header("Interact Settings")]
	[HorizontalRule]
	[ColoredGroup] public float requiredHoldDuration = 1f;

	private Dictionary<PlayerBody, Coroutine> activeHolds = new Dictionary<PlayerBody, Coroutine>();
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

	private IEnumerator HoldInteraction(PlayerBody body)
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
