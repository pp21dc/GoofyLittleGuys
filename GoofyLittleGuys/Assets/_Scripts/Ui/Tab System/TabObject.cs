using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabObject : MonoBehaviour
{
    private bool isActive = false;
    private bool isHovered = false;

    [SerializeField] private GameObject tabIndicator;
    [SerializeField] private GameObject tabInformation;
    [SerializeField] private GameObject tabNavigationStart;

    [SerializeField] private GameObject selectedTabImage;
    [SerializeField] private GameObject hoveredTabImage;
    [SerializeField] private GameObject currentTabImage;


    public void DeactivateTab()
    {
        tabInformation.SetActive(false);
    }
    public void ActivateTab()
    {
        tabInformation.SetActive(true);
    }
}
