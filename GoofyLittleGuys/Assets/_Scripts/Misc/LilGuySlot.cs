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
	public bool LockState => lockState;	// Getter for lock state of this lil guy slot.

	/// <summary>
	/// Method that sees if the lil guy contained within this slot is dead or not.
	/// Sets the lock state to true if the lil guy is dead, otherwise, lockstate is false.
	/// </summary>
	public void CheckIfLiving(LilGuyBase lilGuy)
	{
		LilGuyBase lilGuyInSlot = lilGuy;
		if (lilGuyInSlot == null) return;
		lockState = lilGuyInSlot.Health <= 0 ? true : false;
	}
}
