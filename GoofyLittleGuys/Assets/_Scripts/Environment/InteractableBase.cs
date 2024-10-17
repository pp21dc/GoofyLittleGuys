using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableBase : MonoBehaviour
{
    [SerializeField] protected GameObject interactableCanvas;

    /// <summary>
    /// Called when a player has interacted within range of the interactable.
    /// </summary>
    /// <param name="body">The player that interacted with this interactable.</param>
    public virtual void OnInteracted(PlayerBody body)
    {

    }
}
