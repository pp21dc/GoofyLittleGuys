using UnityEngine;

public class Voxel
{
	public Vector3 position;
	public bool isWalkable;
	public int GCost, HCost;
	public Voxel parent;

	public int FCost => GCost + HCost;

	public Voxel(Vector3 pos, bool walkable)
	{
		position = pos;
		isWalkable = walkable;
	}
}
