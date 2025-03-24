using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BuffHandler
{
	#region Public Variables & Serialize Fields
	#endregion

	#region Private Variables
	private List<TimedBuff> activeBuffs = new List<TimedBuff>();
	#endregion

	#region Getters & Setters
	#endregion

	#region Events & Delegates
	// Callback when a buff expires
	public delegate void BuffExpiredDelegate(BuffType type, object source);
	public event BuffExpiredDelegate OnBuffExpired;
	#endregion

	public void AddBuff(BuffType type, float amount, float duration, object source)
	{
		activeBuffs.RemoveAll(b => b.Type == type && b.Source == source); // Prevent stacking same source/type
		activeBuffs.Add(new TimedBuff(type, amount, duration, source));
	}

	public void RemoveBuffFromSource(BuffType type, object source)
	{
		activeBuffs.RemoveAll(b => b.Type == type && b.Source == source);
	}

	public float GetTotalValue(BuffType type)
	{
		return activeBuffs.Where(b => b.Type == type && Time.time < b.EndTime).Sum(b => b.Amount);
	}

	public bool HasBuffFromSource(BuffType type, object source)
	{
		return activeBuffs.Any(b => b.Type == type && b.Source == source && Time.time < b.EndTime);
	}

	public void Update()
	{
		// Find expired buffs
		var expiredBuffs = activeBuffs.Where(b => Time.time >= b.EndTime).ToList();

		// Remove them and invoke the event
		foreach (var buff in expiredBuffs)
		{
			activeBuffs.Remove(buff);
			OnBuffExpired?.Invoke(buff.Type, buff.Source);
		}
	}

}
