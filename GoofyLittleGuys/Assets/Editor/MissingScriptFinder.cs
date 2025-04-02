using UnityEngine;
using UnityEditor;

public class MissingScriptFinder
{
	[MenuItem("Tools/Find Missing Scripts in Scene")]
	public static void FindMissingScripts()
	{
		GameObject[] allGOs = GameObject.FindObjectsOfType<GameObject>();
		int count = 0;

		foreach (GameObject go in allGOs)
		{
			Component[] components = go.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] == null)
				{
					Debug.LogWarning($"Missing script found on GameObject: {go.name}", go);
					count++;
				}
			}
		}

		Debug.Log($"Finished scanning. {count} missing scripts found.");
	}

	[MenuItem("Tools/Scan All Assets for Missing References")]
	static void ScanAssets()
	{
		string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
		int missingCount = 0;

		foreach (string path in allAssetPaths)
		{
			var obj = AssetDatabase.LoadMainAssetAtPath(path);
			if (obj == null) continue;

			var so = new SerializedObject(obj);
			var prop = so.GetIterator();
			while (prop.NextVisible(true))
			{
				if (prop.propertyType == SerializedPropertyType.ObjectReference &&
					prop.objectReferenceValue == null &&
					prop.objectReferenceInstanceIDValue != 0)
				{
					Debug.LogWarning($"Missing reference in asset: {path} -> {prop.displayName}", obj);
					missingCount++;
				}
			}
		}

		// Also scan ScriptableObjects
		string[] scriptableGuids = AssetDatabase.FindAssets("t:ScriptableObject");
		foreach (string guid in scriptableGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			ScriptableObject soAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
			if (soAsset == null) continue;

			SerializedObject so = new SerializedObject(soAsset);
			SerializedProperty prop = so.GetIterator();
			while (prop.NextVisible(true))
			{
				if (prop.propertyType == SerializedPropertyType.ObjectReference &&
					prop.objectReferenceValue == null &&
					prop.objectReferenceInstanceIDValue != 0)
				{
					Debug.LogWarning($"Broken reference in ScriptableObject: {path} -> {prop.displayName}", soAsset);
				}
			}
		}


		Debug.Log($"Scan complete. Found {missingCount} missing references.");
	}
}
