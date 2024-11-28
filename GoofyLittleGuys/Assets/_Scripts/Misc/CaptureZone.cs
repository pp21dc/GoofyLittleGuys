using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureZone : MonoBehaviour
{
    private List<PlayerBody> playersInRange = new List<PlayerBody>();
    private LilGuyBase lilGuyToCapture;
    private AiController controller;

    public void Init(LilGuyBase lilGuy)
    {
        lilGuyToCapture = lilGuy;
        controller = lilGuyToCapture.GetComponent<AiController>();
    }

	private void OnTriggerStay(Collider other)
	{
        PlayerBody playerInRange = other.GetComponent<PlayerBody>();
        if (playerInRange == null) return;
        if (playersInRange.Count > 0 && playersInRange.Contains(playerInRange)) return;    

        playersInRange.Add(playerInRange);
        playerInRange.ClosestWildLilGuy = lilGuyToCapture;
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBody playerInRange = other.GetComponent<PlayerBody>();
		if (playerInRange == null) return;
		if (playersInRange.Count <= 0 || !playersInRange.Contains(playerInRange)) return;

		playersInRange.Remove(playerInRange);
		playerInRange.ClosestWildLilGuy = null;
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

	private void OnDestroy()
	{
		foreach(PlayerBody body in playersInRange)
		{
			if (body.ClosestWildLilGuy == lilGuyToCapture) body.ClosestWildLilGuy = null;
		}
	}
	// Update is called once per frame
	void Update()
    {
        controller.ToggleInteractCanvas(playersInRange.Count > 0);
    }
}
