using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetDeviceBindings : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputAction;

    public void ResetAllBindings()
    {
        foreach (var map in _inputAction.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
    }
}
