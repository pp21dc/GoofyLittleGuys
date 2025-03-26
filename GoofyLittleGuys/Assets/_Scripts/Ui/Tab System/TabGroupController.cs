using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroupController : MonoBehaviour
{
    [SerializeField] private List<TabObject> tabs;

    [SerializeField] private GameObject ribbonButton; //Generic Button/icon for the top ribbon

    private int currentTab = 0;

    //Should convert most of this to event based

    private void Start()
    {
        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) //Convert to real input
            OnLB();
        else if (Input.GetKeyDown(KeyCode.E)) //Convert to real input
            OnRB();
    }
    private void OnEnable()
    {
        currentTab = 0;
    }


    //Swapping Active Tab
    public void OnRB()
    {
        DeactivateTab(currentTab);
        currentTab++;
        
        if(currentTab >= tabs.Count)
            currentTab = 0;

        ActivateTab(currentTab);

		Managers.DebugManager.Log(currentTab.ToString(), Managers.DebugManager.DebugCategory.UI);
    }

    public void OnLB()
    {
        DeactivateTab(currentTab);
        currentTab--;

        if (currentTab < 0)
            currentTab = tabs.Count - 1;

        ActivateTab(currentTab);

		Managers.DebugManager.Log(currentTab.ToString(), Managers.DebugManager.DebugCategory.UI);
	}

    private void ActivateTab(int currentTab)
    {
        tabs[currentTab].ActivateTab();
    }

    private void DeactivateTab(int currentTab)
    {
        tabs[currentTab].DeactivateTab();
    }
}
