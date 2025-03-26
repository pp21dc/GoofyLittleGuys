using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabObject : MonoBehaviour
{
    private bool isActive = false;
    private bool isHovered = false;

    [SerializeField] private GameObject tabIndicator;
    [SerializeField] private GameObject tabContainer;
    [SerializeField] private GameObject tabNavigationStart;

    [SerializeField] private GameObject selectedTabImage;
    [SerializeField] private GameObject hoveredTabImage;
    [SerializeField] private GameObject currentTabImage;


    public void DeactivateTab()
    {
        tabContainer.SetActive(false);
    }
    public void ActivateTab()
    {
        tabContainer.SetActive(true);
    }
}
