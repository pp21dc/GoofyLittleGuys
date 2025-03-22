using UnityEngine;

public class TimedBuff
{
	public BuffType Type;
	public float Amount;
	public float EndTime;
	public object Source;

	public TimedBuff(BuffType type, float amount, float duration, object source)
	{
		Type = type;
		Amount = amount;
		EndTime = Time.time + duration;
		Source = source;
	}
}
