using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMoveBase : MonoBehaviour 
{
	protected float cooldownTimer = 0;
	protected float cooldownDuration = 1;
	protected virtual void OnSpecialUsed() { }
}
