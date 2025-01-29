using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Debuff : MonoBehaviour
{
	protected float damage;
	protected float duration;
	protected float currentDuration;
	protected float damageApplicationInterval;


	public float Damage { set { damage = value; } }
	public float Duration { set { duration = value; } }
	public float CurrentDuration { set { currentDuration = value; } }
	public float DamageApplicationInterval { set { damageApplicationInterval = value; } }

	public abstract void Init(float damage, float duration, float interval);
}
