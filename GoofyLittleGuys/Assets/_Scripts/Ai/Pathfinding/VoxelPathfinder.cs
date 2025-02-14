using System.Collections.Generic;
using UnityEngine;

public class VoxelPathfinder
{
	public static List<Voxel> FindPath(Vector3 start, Vector3 target)
	{
		Voxel startNode = VoxelGrid.Instance.GetNearestVoxel(start);
		Voxel targetNode = VoxelGrid.Instance.GetNearestVoxel(target);

		if (!startNode.isWalkable || !targetNode.isWalkable)
		{
			return null;
		}

		List<Voxel> openSet = new List<Voxel> { startNode };
		HashSet<Voxel> closedSet = new HashSet<Voxel>();

		while (openSet.Count > 0)
		{
			Voxel currentNode = openSet[0];

			for (int i = 1; i < openSet.Count; i++)
			{
				if (openSet[i].FCost < currentNode.FCost || openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
				{
					currentNode = openSet[i];
				}
			}

			openSet.Remove(currentNode);
			closedSet.Add(currentNode);

			if (currentNode == targetNode)
			{
				return RetracePath(startNode, targetNode);
			}

			foreach (Voxel neighbor in VoxelGrid.Instance.GetVoxelNeighbors(currentNode))
			{
				if (!neighbor.isWalkable || closedSet.Contains(neighbor))
				{
					continue;
				}

				int newMovementCost = currentNode.GCost + GetDistance(currentNode, neighbor);
				if (newMovementCost < neighbor.GCost || !openSet.Contains(neighbor))
				{
					neighbor.GCost = newMovementCost;
					neighbor.HCost = GetDistance(neighbor, targetNode);
					neighbor.parent = currentNode;

					if (!openSet.Contains(neighbor))
					{
						openSet.Add(neighbor);
					}
				}
			}
		}

		return null;
	}

	private static List<Voxel> RetracePath(Voxel startNode, Voxel endNode)
	{
		List<Voxel> path = new List<Voxel>();
		Voxel currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		path.Reverse();
		return path;
	}

	private static int GetDistance(Voxel a, Voxel b)
	{
		return Mathf.Abs(Mathf.RoundToInt(a.position.x - b.position.x)) +
			   Mathf.Abs(Mathf.RoundToInt(a.position.y - b.position.y)) +
			   Mathf.Abs(Mathf.RoundToInt(a.position.z - b.position.z));
	}
}
