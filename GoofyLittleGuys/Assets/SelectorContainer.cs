using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorContainer : MonoBehaviour
{
    [SerializeField] private Transform[] selectorContainers;

    public void SetSelectorContainer(GameObject selector, int playerNum)
    {
        selector.transform.parent = selectorContainers[playerNum].transform;
        selector.transform.localPosition = Vector3.zero;
    }
}
