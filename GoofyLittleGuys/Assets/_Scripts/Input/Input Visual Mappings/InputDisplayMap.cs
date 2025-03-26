using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputDisplayInfo
{
	public string actionName;             // e.g., "Interact"
	public Sprite gamepadIcon;            // E.g., A button
	public Sprite keyboardIcon;            // E.g., A button
	public string keyboardKeyName;        // E.g., "E"
}

[CreateAssetMenu(menuName = "UI/Input Display Map")]
public class InputDisplayMap : ScriptableObject
{
	public List<InputDisplayInfo> entries;

	public InputDisplayInfo GetInfo(string actionName)
	{
		return entries.Find(x => x.actionName == actionName);
	}
}
