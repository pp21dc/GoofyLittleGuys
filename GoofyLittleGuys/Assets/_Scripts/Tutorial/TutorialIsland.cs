using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialIsland : MonoBehaviour
{
    /// <summary>
    /// this is just a ref holder for all GO's on a specific island, reference-able by each state
    /// </summary>
    [Header("References")]
    [HorizontalRule]
	public List<GameObject> enemies;
	[ColoredGroup] public GameObject lilGuyPref;
	[ColoredGroup] public Transform enemySpawnPoint;
	[ColoredGroup] public GameObject storm;
	[ColoredGroup] public GameObject fountain;
	[ColoredGroup] public GameObject exitPortal;

    private void Start()
    {
        exitPortal.SetActive(false);
        fountain.GetComponent<TutorialFountain>().interactArea.enabled = false;
    }
}