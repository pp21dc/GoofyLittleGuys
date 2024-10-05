using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AoeSpecialMove))]
public class AoeSpecialMoveEditor : Editor
{
	SerializedProperty aoeTypeProperty;
	SerializedProperty aoeColliderProperty;
	SerializedProperty aoeMaxSizeProperty;
	SerializedProperty aoeExpansionSpeedProperty;
	SerializedProperty aoeDamageProperty;
	private void OnEnable()
	{
		aoeTypeProperty = serializedObject.FindProperty("aoeType");
		aoeColliderProperty = serializedObject.FindProperty("aoeShape");
		aoeMaxSizeProperty = serializedObject.FindProperty("aoeMaxSize");
		aoeExpansionSpeedProperty = serializedObject.FindProperty("aoeExpansionSpeed");
		aoeDamageProperty = serializedObject.FindProperty("aoeDamage");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(aoeTypeProperty);

		if ((AoeSpecialMove.AoEType)aoeTypeProperty.enumValueIndex == AoeSpecialMove.AoEType.Custom)
		{
			EditorGUILayout.PropertyField(aoeColliderProperty);
		}

		EditorGUILayout.PropertyField(aoeMaxSizeProperty);
		EditorGUILayout.PropertyField(aoeExpansionSpeedProperty);
		EditorGUILayout.PropertyField(aoeDamageProperty);

		serializedObject.ApplyModifiedProperties();
	}
}
