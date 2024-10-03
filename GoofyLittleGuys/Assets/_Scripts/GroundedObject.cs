using UnityEngine;

/// <summary>
/// Gives context to the autogrounding editor tool as to which objects should be affected by autogrounding.
/// 
/// Author: Xavier Triska
/// </summary>
[ExecuteInEditMode]
public class GroundedObject : MonoBehaviour
{
	[SerializeField]
	private LayerMask groundLayer;
	[SerializeField, Range(0f, 1f)]
	[Tooltip("Controls how much the object aligns with the ground's surface. \n0: Object stays upright.\n1: Fully aligns the object's up direction with the ground's slope.")]
	private float alignmentFactor = 0f;

	public void GroundToSurface()
	{
		Ray ray = new Ray(transform.position + Vector3.up * 1000, Vector3.down);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
		{
			transform.position = hit.point;
			Vector3 surfaceNormal = hit.normal;
			Vector3 currentUp = transform.up;
			Vector3 newUp = Vector3.Slerp(currentUp, surfaceNormal, alignmentFactor);

			Quaternion rotation = Quaternion.FromToRotation(currentUp, newUp);
			transform.rotation = rotation * transform.rotation;
		}
	}
}
