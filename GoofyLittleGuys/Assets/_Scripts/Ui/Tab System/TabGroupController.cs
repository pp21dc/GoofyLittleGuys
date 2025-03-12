using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroupController : MonoBehaviour
{
    [SerializeField] private List<TabObject> tabs;

    private int currentTab = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            OnLB();
        else if (Input.GetKeyDown(KeyCode.E))
            OnRB();
    }
    private void OnEnable()
    {
        currentTab = 0;
    }
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
