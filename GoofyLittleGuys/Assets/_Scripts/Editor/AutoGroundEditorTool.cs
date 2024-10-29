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
	private float minScale = 1f;
	private float maxScale = 1f;
	private GameObject prefabToSpawn;
	private bool randomizeScale = false;

	private static bool autoGroundEnabled = false;

	[MenuItem("Tools/Grounded Props Tool")]
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

		EditorGUILayout.LabelField("Scale Settings", EditorStyles.boldLabel);
		minScale = EditorGUILayout.FloatField("Min Scale", minScale);
		maxScale = EditorGUILayout.FloatField("Max Scale", maxScale);

		if (GUILayout.Button("Set Random Scale"))
		{
			SetRandomScaleForSelectedObjects();
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Spawn Object", EditorStyles.boldLabel);
		prefabToSpawn = (GameObject)EditorGUILayout.ObjectField("Prefab to Spawn", prefabToSpawn, typeof(GameObject), false);

		// Randomize Scale Checkbox
		randomizeScale = EditorGUILayout.Toggle("Randomize Scale on Spawn", randomizeScale);

		// Button to spawn the prefab
		if (GUILayout.Button("Spawn GameObject"))
		{
			SpawnPrefabWithOptionalRandomScale();
		}
	}

	/// <summary>
	/// Sets a random scale for the currently selected objects within the min and max scale range,
	/// rounded to the nearest 0.05 increment.
	/// </summary>
	private void SetRandomScaleForSelectedObjects()
	{
		foreach (var selectedObject in Selection.transforms)
		{
			if (selectedObject.GetComponent<GroundedObject>() != null)
			{
				float randomScale = GetRandomScaleInIncrements(minScale, maxScale, 0.05f);
				selectedObject.localScale = Vector3.one * randomScale;
			}
		}
	}

	/// <summary>
	/// Spawns a new instance of the specified prefab and optionally applies a random scale to it,
	/// rounded to the nearest 0.05 increment.
	/// </summary>
	private void SpawnPrefabWithOptionalRandomScale()
	{
		if (prefabToSpawn == null)
		{
			Debug.LogWarning("No prefab assigned to spawn.");
			return;
		}

		// Spawn prefab at the scene's origin or an appropriate position
		GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
		if (newObject == null)
		{
			Debug.LogError("Failed to instantiate prefab.");
			return;
		}

		// Position the new object at (0,0,0) or any default position
		newObject.transform.position = Vector3.zero;

		// Apply random scale if enabled
		if (randomizeScale)
		{
			float randomScale = GetRandomScaleInIncrements(minScale, maxScale, 0.05f);
			newObject.transform.localScale = Vector3.one * randomScale;
		}

		// Auto-ground the newly spawned object if it has the GroundedObject component
		GroundedObject groundScript = newObject.GetComponent<GroundedObject>();
		if (groundScript != null)
		{
			groundScript.GroundToSurface();
		}

		// Register the object creation for undo purposes
		Undo.RegisterCreatedObjectUndo(newObject, "Spawned " + newObject.name);
	}

	/// <summary>
	/// Generates a random float between min and max, rounded to the nearest specified increment.
	/// </summary>
	/// <param name="min">Minimum scale value.</param>
	/// <param name="max">Maximum scale value.</param>
	/// <param name="increment">Increment to round to, e.g., 0.05f.</param>
	/// <returns>Random float between min and max, rounded to the nearest increment.</returns>
	private float GetRandomScaleInIncrements(float min, float max, float increment)
	{
		float randomValue = Random.Range(min, max);
		return Mathf.Round(randomValue / increment) * increment;
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
