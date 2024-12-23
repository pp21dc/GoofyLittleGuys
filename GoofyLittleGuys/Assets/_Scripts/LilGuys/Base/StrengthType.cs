using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StrengthType : LilGuyBase
{
	[SerializeField] protected GameObject aoeShape;  // Only visible in editor and only used when aoeType is set to "Custom". 
	[SerializeField] public int aoeDamage = 1;
	[SerializeField] private float frameToDestroyAoe = 15;
	[SerializeField] private float specialTotalFrames = 17;

	protected float aoeDestroyTime;

	protected override void Start()
	{
		base.Start();

		AnimationClip clip = anim.runtimeAnimatorController.animationClips.First(clip => clip.name == "Special");
		if (clip != null)
		{
			aoeDestroyTime = (float)(frameToDestroyAoe / specialTotalFrames) * clip.length;
		}
		else aoeDestroyTime = 1;
	}

	public override void StartChargingSpecial()
	{

		base.StartChargingSpecial();

	}

	public override void StopChargingSpecial()
	{
		base.StopChargingSpecial();
	}
	public override void Special()
	{
		base.Special();
	}
}
