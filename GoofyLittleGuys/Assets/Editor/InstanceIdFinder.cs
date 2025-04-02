using UnityEditor;
using UnityEngine;

public class InstanceIdFinder : EditorWindow
{
	private int instanceID = 0;
	private Object foundObject;

	[MenuItem("Tools/Find Object by Instance ID")]
	public static void ShowWindow()
	{
		GetWindow<InstanceIdFinder>("Instance ID Finder");
	}

	private void OnGUI()
	{
		GUILayout.Label("Find Object from Instance ID", EditorStyles.boldLabel);

		instanceID = EditorGUILayout.IntField("Instance ID", instanceID);

		if (GUILayout.Button("Find Object"))
		{
			foundObject = EditorUtility.InstanceIDToObject(instanceID);

			if (foundObject != null)
				Debug.Log($"Found: {foundObject.name} at {AssetDatabase.GetAssetPath(foundObject)}", foundObject);
			else
				Debug.LogWarning("No object found with that instance ID.");
		}

		if (foundObject != null)
		{
			EditorGUILayout.ObjectField("Result:", foundObject, typeof(Object), false);
		}
	}
}
