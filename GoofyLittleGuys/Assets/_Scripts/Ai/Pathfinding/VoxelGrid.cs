using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{
	public static VoxelGrid Instance { get; private set; }
	[Header("Debug Options")]
	[SerializeField] private bool debugDrawGrid = false;

	public int gridSizeX, gridSizeY, gridSizeZ;
	public float voxelSize;
	public LayerMask obstacleMask;
	public Voxel[,,] grid;

	private void Awake()
	{
		Instance = this;
		GenerateGrid();
	}

	private void OnValidate()
	{

		GenerateGrid();
	}
	private void OnDrawGizmos()
	{
		if (!debugDrawGrid || grid == null) return;

		foreach (Voxel voxel in grid)
		{
			Gizmos.color = voxel.isWalkable ? Color.green : Color.red;
			Gizmos.DrawWireCube(voxel.position, Vector3.one * voxelSize);
		}
	}

	void GenerateGrid()
	{
		grid = new Voxel[gridSizeX, gridSizeY, gridSizeZ];

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				for (int z = 0; z < gridSizeZ; z++)
				{
					Vector3 worldPoint = new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);
					bool isWalkable = !Physics.CheckBox(worldPoint, Vector3.one * voxelSize * 0.5f, Quaternion.identity, obstacleMask);
					grid[x, y, z] = new Voxel(worldPoint, isWalkable);
				}
			}
		}
	}

	public Voxel GetNearestVoxel(Vector3 position)
	{
		int x = Mathf.RoundToInt(position.x / voxelSize);
		int y = Mathf.RoundToInt(position.y / voxelSize);
		int z = Mathf.RoundToInt(position.z / voxelSize);
		return grid[x, y, z];
	}

	public List<Voxel> GetVoxelNeighbors(Voxel voxel)
	{
		List<Voxel> neighbors = new List<Voxel>();

		// 3D directions (Up, Down, Left, Right, Forward, Backward & Diagonals if needed)
		int[] directions = { -1, 0, 1 };

		foreach (int x in directions)
		{
			foreach (int y in directions)
			{
				foreach (int z in directions)
				{
					// Skip the center voxel (0,0,0)
					if (x == 0 && y == 0 && z == 0) continue;

					int neighborX = Mathf.RoundToInt(voxel.position.x / voxelSize) + x;
					int neighborY = Mathf.RoundToInt(voxel.position.y / voxelSize) + y;
					int neighborZ = Mathf.RoundToInt(voxel.position.z / voxelSize) + z;

					// Ensure the neighbor is within grid bounds
					if (neighborX >= 0 && neighborX < gridSizeX &&
						neighborY >= 0 && neighborY < gridSizeY &&
						neighborZ >= 0 && neighborZ < gridSizeZ)
					{
						Voxel neighbor = grid[neighborX, neighborY, neighborZ];
						if (neighbor.isWalkable)
						{
							neighbors.Add(neighbor);
						}
					}
				}
			}
		}

		return neighbors;
	}

}
