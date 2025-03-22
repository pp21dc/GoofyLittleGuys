using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BuffHandler
{
	private List<TimedBuff> activeBuffs = new List<TimedBuff>();
	
	// Callback when a buff expires
	public delegate void BuffExpiredDelegate(BuffType type, object source);
	public event BuffExpiredDelegate OnBuffExpired;

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
