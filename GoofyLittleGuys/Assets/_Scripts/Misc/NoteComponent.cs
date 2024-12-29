using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class NoteComponent : MonoBehaviour
{
	[TextArea]
	public string note = "Enter your note here...";
	[SerializeField] private int fontSize = 6;
	[SerializeField] private Color fontColor = Color.yellow;
	[SerializeField] private TextAnchor labelAnchor = TextAnchor.MiddleCenter;
	[SerializeField] private bool wordWrap = true;
	[SerializeField] private bool renderInEditor = true;
	[SerializeField] private Vector2 labelRectSize = new Vector2(100, 100);

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (!string.IsNullOrEmpty(note) && renderInEditor)
		{
			// Get the Scene View camera
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView == null) return;

			Camera sceneCamera = sceneView.camera;
			if (sceneCamera == null) return;

			// Calculate the distance from the camera to the GameObject
			float distance = Vector3.Distance(sceneCamera.transform.position, transform.position);

			// Scale font size based on distance (larger distance = smaller font)

			// Create GUIStyle for the label
			GUIStyle style = new GUIStyle();
			style.alignment = labelAnchor; // Fully center the text
			style.normal.textColor = fontColor;
			style.fontSize = fontSize; // Dynamically scaled font size
			style.wordWrap = wordWrap; // Enable word wrapping

			// Convert world position to screen position
			Vector3 screenPosition = HandleUtility.WorldToGUIPoint(transform.position);

			// Create a centered rect for the text (100x100)
			Rect textRect = new Rect(screenPosition.x - labelRectSize.x / 2, screenPosition.y - labelRectSize.y / 2, labelRectSize.x, labelRectSize.y);

			// Draw the label
			Handles.BeginGUI();
			GUI.Label(textRect, note, style);
			Handles.EndGUI();
		}
	}
#endif
}
