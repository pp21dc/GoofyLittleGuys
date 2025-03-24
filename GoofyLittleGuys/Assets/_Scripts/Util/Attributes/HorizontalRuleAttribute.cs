using UnityEngine;

public class HorizontalRuleAttribute : PropertyAttribute
{
	public float Thickness { get; }
	public float Padding { get; }
	public Color Color { get; }

	public HorizontalRuleAttribute(float thickness = 1f, float padding = 6f, float r = 0.5f, float g = 0.5f, float b = 0.5f)
	{
		Thickness = thickness;
		Padding = padding;
		Color = new Color(r, g, b);
	}
}
