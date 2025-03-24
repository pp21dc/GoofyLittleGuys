using UnityEngine;

public class ColoredGroupAttribute : PropertyAttribute
{
	public Color BoxColor { get; }

	public ColoredGroupAttribute(float r = 0.175f, float g = 0.175f, float b = 0.175f)
	{
		BoxColor = new Color(r, g, b);
	}
}
