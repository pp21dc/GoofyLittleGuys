using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventCaller : MonoBehaviour
{
	[SerializeField] private LilGuyBase lilGuy;
	public void SpawnHitbox()
	{
		lilGuy.SpawnHitbox();
	}

	public void DestroyHitbox()
	{
		lilGuy.DestroyHitbox();
	}
}
