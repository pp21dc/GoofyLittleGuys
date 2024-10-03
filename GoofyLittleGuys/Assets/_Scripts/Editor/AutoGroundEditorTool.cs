using UnityEditor;
using UnityEngine;

/// <summary>
/// Provides the autogrounding editor tool in the Unity toolbar (Under Tools) and applies autogrounding to the currently
/// selected GameObject
/// 
/// Author: Xavier Triska
/// </summary>
[InitializeOnLoad]
public class AutoGroundEditorTool : EditorWindow
{
	private static bool autoGroundEnabled = false;

	[MenuItem("Tools/Auto Ground Tool")]
	public static void ShowWindow()
	{
		GetWindow<AutoGroundEditorTool>("Auto Ground Tool");
	}

	public static void EnableAutoGround()
	{
		EditorApplication.update += OnEditorUpdate;
		SceneView.duringSceneGui += OnSceneGUI;
	}

	public static void DisableAutoGround()
	{
		EditorApplication.update -= OnEditorUpdate;
		SceneView.duringSceneGui -= OnSceneGUI;
	}
	/// <summary>
	/// Unity method called when the Unity Editor GUI is loaded
	/// </summary>
	private void OnGUI()
	{
		autoGroundEnabled = EditorGUILayout.Toggle("Enable Auto Grounding", autoGroundEnabled);

		if (autoGroundEnabled)
		{
			EnableAutoGround();
		}
		else
		{
			DisableAutoGround();
		}
	}
	
	/// <summary>
	/// Handles user input on 'autogrounding' objects in the scene view
	/// </summary>
	/// <param name="sceneView"></param>
	private static void OnSceneGUI(SceneView sceneView)
	{
		if (!autoGroundEnabled) return;
		Event currEvent = Event.current;

		if (currEvent.type == EventType.MouseUp && currEvent.button == 0)
		{
			CheckAndGroundSelectedObjects();
		}
	}

	private static void OnEditorUpdate()
	{
		if (!autoGroundEnabled) return;
		if (Selection.transforms.Length > 0)
		{
			CheckAndGroundSelectedObjects();
		}
	}

	private static void CheckAndGroundSelectedObjects()
	{
		foreach (var selectedObject in Selection.transforms)
		{
			GroundedObject groundScript = selectedObject.GetComponent<GroundedObject>();
			if (groundScript != null)
			{
				groundScript.GroundToSurface();
			}
		}
	}
}
