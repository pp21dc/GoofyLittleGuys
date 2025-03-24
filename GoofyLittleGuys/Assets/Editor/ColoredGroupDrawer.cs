using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ColoredGroupAttribute))]
public class ColoredGroupDrawer : PropertyDrawer
{
	private const float Padding = 4f;
	private const float InnerPadding = 2f;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ColoredGroupAttribute attr = (ColoredGroupAttribute)attribute;
		Color boxColor = attr.BoxColor;

		float totalHeight = EditorGUI.GetPropertyHeight(property, label, true);
		Rect backgroundRect = new Rect(position.x, position.y, position.width, totalHeight + Padding);

		// Draw background box
		EditorGUI.DrawRect(backgroundRect, boxColor);

		// Adjust for inner padding
		Rect fieldRect = new Rect(position.x + InnerPadding, position.y + InnerPadding, position.width - (InnerPadding * 2), totalHeight);
		EditorGUI.PropertyField(fieldRect, property, label, true);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUI.GetPropertyHeight(property, label, true);
		return height + Padding;
	}
}
