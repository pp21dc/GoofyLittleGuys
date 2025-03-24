using UnityEditor;
using UnityEngine;

public class SortGameObjects : EditorWindow
{
	private string baseName = "Object";

	[MenuItem("Tools/Sort Selected GameObjects")]
	private static void ShowWindow()
	{
		GetWindow<SortGameObjects>("Sort GameObjects");
	}

	private void OnGUI()
	{
		GUILayout.Label("Sort and Rename Selected GameObjects", EditorStyles.boldLabel);

		// Input field for base object name
		baseName = EditorGUILayout.TextField("Base Name", baseName);

		GUILayout.Space(10);

		// Sorting Buttons
		GUILayout.Label("Sorting Options", EditorStyles.boldLabel);
		if (GUILayout.Button("Sort Ascending"))
		{
			SortSelectedGameObjects(false);
		}

		if (GUILayout.Button("Sort Descending"))
		{
			SortSelectedGameObjects(true);
		}

		GUILayout.Space(10);

		// Renaming Buttons
		GUILayout.Label("Renaming Options", EditorStyles.boldLabel);
		if (GUILayout.Button("Rename Ascending"))
		{
			RenameSelectedGameObjectsAscending(baseName);
		}

		if (GUILayout.Button("Rename Descending"))
		{
			RenameSelectedGameObjectsDescending(baseName);
		}
	}

	private static void SortSelectedGameObjects(bool descending)
	{
		// Get all selected GameObjects in the scene
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0)
		{
			Managers.DebugManager.Log("No GameObjects selected to sort.", Managers.DebugManager.DebugCategory.EDITOR_TOOL, Managers.DebugManager.LogLevel.WARNING);
			return;
		}

		// Sort the selected GameObjects by name, either alphabetically or numerically (by suffix if it exists)
		System.Array.Sort(selectedObjects, (a, b) =>
		{
			int suffixA = ExtractSuffix(a.name);
			int suffixB = ExtractSuffix(b.name);

			// If both GameObjects don't have suffixes, sort alphabetically
			if (suffixA == 0 && suffixB == 0)
			{
				return descending ? b.name.CompareTo(a.name) : a.name.CompareTo(b.name);
			}

			return descending ? suffixB.CompareTo(suffixA) : suffixA.CompareTo(suffixB);
		});

		// Reorder in the hierarchy
		for (int i = 0; i < selectedObjects.Length; i++)
		{
			selectedObjects[i].transform.SetSiblingIndex(i);
		}

		Managers.DebugManager.Log("Selected GameObjects have been sorted.", Managers.DebugManager.DebugCategory.EDITOR_TOOL);
	}

	private static void RenameSelectedGameObjectsDescending(string baseName)
	{
		// Get all selected GameObjects in the scene
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0)
		{
			Managers.DebugManager.Log("No GameObjects selected to rename.", Managers.DebugManager.DebugCategory.EDITOR_TOOL, Managers.DebugManager.LogLevel.WARNING);
			return;
		}

		// Rename the GameObjects with the specified base name in decremental order
		for (int i = 0; i < selectedObjects.Length; i++)
		{
			selectedObjects[i].name = $"{baseName} ({selectedObjects.Length - i})";
		}

		Managers.DebugManager.Log("Selected GameObjects have been renamed in descending order.", Managers.DebugManager.DebugCategory.EDITOR_TOOL);
	}
	private static void RenameSelectedGameObjectsAscending(string baseName)
	{
		// Get all selected GameObjects in the scene
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0)
		{
			Managers.DebugManager.Log("No GameObjects selected to rename.", Managers.DebugManager.DebugCategory.EDITOR_TOOL, Managers.DebugManager.LogLevel.WARNING);
			return;
		}

		// Rename the GameObjects with the specified base name in decremental order
		for (int i = 0; i < selectedObjects.Length; i++)
		{
			selectedObjects[i].name = $"{baseName} ({i + 1})";
		}

		Managers.DebugManager.Log("Selected GameObjects have been renamed in descending order.", Managers.DebugManager.DebugCategory.EDITOR_TOOL);
	}

	private static int ExtractSuffix(string name)
	{
		// Extract the numeric suffix, if any
		int suffixIndex = name.LastIndexOf('(');
		if (suffixIndex >= 0)
		{
			string suffixString = name.Substring(suffixIndex + 1, name.Length - suffixIndex - 2); // Remove the closing parenthesis
			if (int.TryParse(suffixString, out int suffix))
			{
				return suffix;
			}
		}
		return 0; // Default to 0 if no suffix is found
	}
}
