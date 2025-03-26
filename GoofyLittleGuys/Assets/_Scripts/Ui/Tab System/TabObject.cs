using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TabObject : MonoBehaviour
{
    private bool isActive = false;
    private bool isHovered = false;

    [SerializeField] private GameObject tabContainer;
    [SerializeField] private GameObject tabNavigationStart;

    [SerializeField] private GameObject selectedTabImage;
    [SerializeField] private GameObject hoveredTabImage;
    [SerializeField] private GameObject currentTabImage;


    public void DeactivateTab()
    {
        tabContainer.SetActive(false);
        this.GetComponent<Image>().color = Color.white;
    }
    public void ActivateTab()
    {
        tabContainer.SetActive(true);
        this.GetComponent<Image>().color = Color.red;
        EventSystem.current.SetSelectedGameObject(tabNavigationStart);
    }
}
