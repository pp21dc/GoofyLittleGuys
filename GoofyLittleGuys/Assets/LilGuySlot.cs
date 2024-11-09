using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container-type script that just adds a lockstate to the gameobject, making it easier to swap between
/// Living lil guys
/// </summary>
public class LilGuySlot : MonoBehaviour
{
	bool lockState = false;
	public bool LockState => lockState;

	public void CheckIfLiving()
	{
		LilGuyBase lilGuyInSlot = GetComponentInChildren<LilGuyBase>();
		if (lilGuyInSlot == null) return;
		lockState = lilGuyInSlot.health <= 0 ? true : false;
	}
}
