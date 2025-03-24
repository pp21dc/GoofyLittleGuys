using UnityEngine;

public class TimedBuff
{
	#region Public Variables
	public BuffType Type;
	public object Source;
	public float Amount;
	public float EndTime;
	#endregion

	public TimedBuff(BuffType type, float amount, float duration, object source)
	{
		Type = type;
		Amount = amount;
		EndTime = Time.time + duration;
		Source = source;
	}
}
