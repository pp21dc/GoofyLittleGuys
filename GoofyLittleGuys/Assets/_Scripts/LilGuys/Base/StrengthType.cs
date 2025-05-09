using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StrengthType : LilGuyBase
{
	[Header("Strength Type Specific")]
	[HorizontalRule]
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] protected GameObject aoeShape;  // Only visible in editor and only used when aoeType is set to "Custom". 
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] public float aoeDamageMultiplier = 1;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private float frameToDestroyAoe = 15;
	[ColoredGroup(0.363f, 0.0904f, 0.1455f)][SerializeField] private float specialTotalFrames = 17;

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
	protected override void Special()
	{
		base.Special();
	}
}
