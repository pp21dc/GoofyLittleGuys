using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Phantaphant : SpeedType
{
	[Header("Phant-a-phant Specific")]
	[SerializeField, Tooltip("The maximum range that a target to dash to can be picked.")] private float dashTargetRange;
	private Transform targetPosition;
	private Vector3 directionToTarget;
	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}

	public override void StartChargingSpecial()
	{
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		if (!IsInSpecialAttack && !IsInBasicAttack)
		{
			LocateClosestTarget();
			StopChargingSpecial();
		}
	}

	public override void Special()
	{
		Rigidbody rb = (playerOwner == null) ? GetComponent<Rigidbody>() : playerOwner.GetComponent<Rigidbody>();
		if (rb != null && targetPosition != null)
		{
			rb.MovePosition(directionToTarget);
		}
		base.Special();		
	}

	protected override void OnEndSpecial()
	{
		base.OnEndSpecial();
		targetPosition = null;
	}
	public override void StopChargingSpecial()
	{
		if (targetPosition == null) return;
		base.StopChargingSpecial();
	}

	private void LocateClosestTarget()
	{
		if (playerOwner != null)
		{
			// Player owned target choosing
			Collider[] nearbyColliders = Physics.OverlapSphere(RB.position, dashTargetRange, LayerMask.GetMask("WildLilGuys", "PlayerLilGuys"));
			Debug.Log(nearbyColliders.Length);

			float closestDistance = float.MaxValue;
			Transform closestTarget = null;

			foreach (Collider collider in nearbyColliders)
			{
				if (collider.transform == transform) continue; // If the collider is itself, skip
				float distance = Vector3.Distance(collider.transform.position, transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestTarget = collider.transform;
				}
			}

			if (closestTarget != null)
			{
				targetPosition = closestTarget;
				directionToTarget = targetPosition.position /*- transform.position*/;
			}
		}
		else
		{
			// Enemy target choosing
			Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, dashTargetRange, LayerMask.GetMask("PlayerLilGuys"));

			float closestDistance = float.MaxValue;
			Transform closestTarget = null;

			foreach (Collider collider in nearbyColliders)
			{
				if (collider.transform == transform) continue; // If the collider is itself, skip
				float distance = Vector3.Distance(collider.transform.position, transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestTarget = collider.transform;
				}
			}

			if (closestTarget != null)
			{
				targetPosition = closestTarget;
				directionToTarget = targetPosition.position /*- transform.position*/;
			}
		}
	}
}
