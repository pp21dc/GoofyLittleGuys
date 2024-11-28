using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
	private bool playerSpawnedHere = false;
	public bool PlayerSpawnedHere {  get { return playerSpawnedHere; } set { playerSpawnedHere = value; } }
}
