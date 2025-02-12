using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
	[SerializeField] private string locationName;

	private void OnTriggerEnter(Collider other)
	{
		PlayerBody body = other.GetComponent<PlayerBody>();
		if (body != null)
		{
			body.GameplayStats.RegisterLocationVisit(locationName);
		}
	}
}
