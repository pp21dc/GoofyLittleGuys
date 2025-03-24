using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HorizontalRuleAttribute))]
public class HorizontalRuleDrawer : DecoratorDrawer
{
	public override void OnGUI(Rect position)
	{
		HorizontalRuleAttribute line = (HorizontalRuleAttribute)attribute;

		float midY = position.y + line.Padding / 2f;
		Rect lineRect = new Rect(position.x, midY, position.width, line.Thickness);
		EditorGUI.DrawRect(lineRect, line.Color);
	}

	public override float GetHeight()
	{
		return ((HorizontalRuleAttribute)attribute).Padding;
	}
}
