using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Teleporter : InteractableBase
{
    // -- Variables --
    [SerializeField] private Teleporter targetTeleporter;
    [SerializeField] private Transform endTeleportLocation;
    [SerializeField] private float cooldown;
    private bool onCooldown;
    private BoxCollider teleporterCollider;
    private List<GameObject> inRange = new List<GameObject>();

    // -- Getters --
    public Transform EndTeleportLocation { get { return endTeleportLocation; } }
    public bool OnCooldown { get { return onCooldown; } set { onCooldown = value; } }

    #region Event Functions
    private void Start()
    {
        teleporterCollider = GetComponent<BoxCollider>();
        teleporterCollider.isTrigger = true;
        interactableCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7 && !inRange.Contains(other.gameObject))
        {
            inRange.Add(other.gameObject);
            other.GetComponent<PlayerBody>().ClosestInteractable = this;
        }
    }

    // Using this as a cheaper update loop
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 7)
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
        if (other.gameObject.layer == 7 && inRange.Contains(other.gameObject))
        {
            inRange.Remove(other.gameObject);
            other.GetComponent<PlayerBody>().ClosestInteractable = null;
        }
    }
    #endregion

    public override void OnInteracted(PlayerBody body)
    {
        base.OnInteracted(body);

        body.gameObject.transform.position = targetTeleporter.EndTeleportLocation.position;
    }

    private IEnumerator WaitForCooldown()
    {
        onCooldown = true;
        targetTeleporter.OnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
        targetTeleporter.OnCooldown = false;
    }
}
