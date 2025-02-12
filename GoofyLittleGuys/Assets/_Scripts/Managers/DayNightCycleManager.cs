using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
	[SerializeField] private float cycleDuration = 60f; // Duration of a full day in seconds
	private float timeElapsed = 0f;

	void Update()
	{
		timeElapsed += Time.deltaTime;
		float rotationX = (timeElapsed / cycleDuration) * 360f;
		transform.rotation = Quaternion.Euler(rotationX, 0, 0f);

		if (timeElapsed >= cycleDuration) timeElapsed = 0f; // Reset cycle
	}
}
