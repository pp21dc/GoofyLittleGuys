using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TurteriamWall : MonoBehaviour
{
	[SerializeField] private Transform innerSortingGroupPoint;
	[SerializeField] private Transform outerSortingGroupPoint;

	[SerializeField] private SortingGroup innerSortingGroup;
	[SerializeField] private SortingGroup outerSortingGroup;
	[SerializeField] private Collider outerWallCollider;
	[SerializeField] private Collider innerWallCollider;

	private float maxSize = 10;
	private float expansionTime = 2f;
	private float wallLifetime = 6f;

	private Vector3 initialScale;
	private Vector3 targetScale;
	public void Init(float maxSize, float expansionTime, float wallLifetime)
	{
		this.maxSize = maxSize;
		this.expansionTime = expansionTime;
		this.wallLifetime = wallLifetime;
	}
	private void Awake()
	{
		innerSortingGroup.sortingOrder = (int)-innerSortingGroupPoint.position.z;
		outerSortingGroup.sortingOrder = (int)-outerSortingGroupPoint.position.z;
		outerWallCollider.enabled = false;
		innerWallCollider.enabled = false;
		initialScale = transform.localScale;
		targetScale = new Vector3(maxSize, maxSize, maxSize);
		StartCoroutine(Expand());
	}

	private IEnumerator Expand()
	{
		float elapsedTime = 0;
		while (elapsedTime < expansionTime)
		{
			// Expanding the hitbox zone
			transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / expansionTime);
			innerSortingGroup.sortingOrder = (int)-innerSortingGroupPoint.position.z;
			outerSortingGroup.sortingOrder = (int)-outerSortingGroupPoint.position.z;
			elapsedTime += Time.deltaTime;

			yield return null;
		}
		outerWallCollider.enabled = true;
		innerWallCollider.enabled = true;
		Destroy(gameObject, wallLifetime);
	}
}
