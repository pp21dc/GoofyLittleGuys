using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class PolygonCollider3D : MonoBehaviour
{
	public Transform[] points; // Array of points defining the polygon's base
	public float height = 1.0f; // Height of the collider along the Y-axis

	private Mesh _mesh;

	protected void OnValidate()
	{
		UpdateCollider();
	}

	private void OnDrawGizmos()
	{
		if (points == null || points.Length < 3) return;

		Gizmos.color = Color.green;

		// Draw base polygon
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 current = points[i].position;
			Vector3 next = points[(i + 1) % points.Length].position;

			Gizmos.DrawLine(current, next); // Line between points
			Gizmos.DrawLine(current, current + Vector3.up * height); // Vertical lines
		}

		// Draw top polygon
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 current = points[i].position + Vector3.up * height;
			Vector3 next = points[(i + 1) % points.Length].position + Vector3.up * height;

			Gizmos.DrawLine(current, next);
		}
	}

	public int fanCenterIndex = 0; // Index of the vertex to fan from

	private void UpdateCollider()
	{
		if (points == null || points.Length < 3)
		{
			Managers.DebugManager.Log("PolygonCollider3D requires at least 3 points to form a shape.", Managers.DebugManager.DebugCategory.EDITOR_TOOL, Managers.DebugManager.LogLevel.WARNING);
			return;
		}

		if (fanCenterIndex < 0 || fanCenterIndex >= points.Length)
		{
			Managers.DebugManager.Log("Invalid fanCenterIndex. It must be between 0 and the number of points - 1.", Managers.DebugManager.DebugCategory.EDITOR_TOOL, Managers.DebugManager.LogLevel.WARNING);
			return;
		}

		// Generate the mesh for the collider
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[points.Length * 2];
		int[] triangles = new int[(points.Length - 2) * 6 + points.Length * 12];

		// Set vertices for the base and top polygons
		for (int i = 0; i < points.Length; i++)
		{
			vertices[i] = transform.InverseTransformPoint(points[i].position); // Base vertices
			vertices[i + points.Length] = transform.InverseTransformPoint(points[i].position + Vector3.up * height); // Top vertices
		}

		int triIndex = 0;

		// Triangles for the base polygon
		for (int i = 0; i < points.Length - 1; i++)
		{
			int current = (fanCenterIndex + i) % points.Length;
			int next = (fanCenterIndex + i + 1) % points.Length;

			triangles[triIndex++] = fanCenterIndex;
			triangles[triIndex++] = current;
			triangles[triIndex++] = next;
		}

		// Triangles for the top polygon
		for (int i = 0; i < points.Length - 1; i++)
		{
			int current = (fanCenterIndex + i) % points.Length;
			int next = (fanCenterIndex + i + 1) % points.Length;

			triangles[triIndex++] = fanCenterIndex + points.Length;
			triangles[triIndex++] = next + points.Length;
			triangles[triIndex++] = current + points.Length;
		}

		// Triangles for the side walls
		for (int i = 0; i < points.Length; i++)
		{
			int next = (i + 1) % points.Length;

			// First triangle for the side wall
			triangles[triIndex++] = i;
			triangles[triIndex++] = points.Length + i;
			triangles[triIndex++] = next;

			// Second triangle for the side wall
			triangles[triIndex++] = next;
			triangles[triIndex++] = points.Length + i;
			triangles[triIndex++] = points.Length + next;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();

		// Assign the mesh to a MeshFilter for visualization
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = mesh;

		// Assign the mesh to a MeshCollider for collision
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		if (!meshCollider) meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;

		// Cache the mesh
		_mesh = mesh;
	}


}
