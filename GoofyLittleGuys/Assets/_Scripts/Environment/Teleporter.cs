using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Teleporter : MonoBehaviour
{
    // -- Variables --
    [SerializeField] private float timeToTeleport;
    [SerializeField] private Teleporter targetTeleporter;
    private float timer = 0.0f;
    private BoxCollider teleporterCollider;

    private void Start()
    {
        teleporterCollider = GetComponent<BoxCollider>();
        teleporterCollider.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            Debug.Log(timer);
            if (timer >= timeToTeleport)
            {
                if (targetTeleporter != null)
                    other.gameObject.transform.position = targetTeleporter.transform.position;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            timer = 0.0f;
    }
}
