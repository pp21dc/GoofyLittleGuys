using Managers;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DebugCategorySettings))]
public class DebugCategorySettingsDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty categoryProp = property.FindPropertyRelative("category");
		SerializedProperty enabledProp = property.FindPropertyRelative("enabled");
		SerializedProperty logLevelProp = property.FindPropertyRelative("logLevelFilter");

		EditorGUI.BeginProperty(position, label, property);

		// Create a foldout labeled "Debug Category"
		property.isExpanded = EditorGUI.Foldout(
			new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
			property.isExpanded,
			categoryProp.enumDisplayNames[categoryProp.enumValueIndex],
			true
		);

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;

			float yOffset = position.y + 20;

			EditorGUI.PropertyField(
				new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
				categoryProp
			);

			yOffset += 20;

			EditorGUI.PropertyField(
				new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
				enabledProp
			);

			yOffset += 20;

			EditorGUI.PropertyField(
				new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
				logLevelProp
			);

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return property.isExpanded ? EditorGUIUtility.singleLineHeight * 4 : EditorGUIUtility.singleLineHeight;
	}
}
