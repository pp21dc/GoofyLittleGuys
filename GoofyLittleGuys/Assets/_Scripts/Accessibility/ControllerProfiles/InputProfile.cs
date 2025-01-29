using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//AUTH: thomas berner

/// <summary>
/// Scriptable for holding input bindings for individual players to overwrite the default action map
/// </summary>

[CreateAssetMenu(fileName = "newInputProfile", menuName = "InputProfile")]
[Serializable]
public class InputProfile : ScriptableObject
{
    public string ProfileName;
    public List<InputBinding> inputBindings = new List<InputBinding>();
    
    /// <summary>
    /// Save bindings from input asset to this input profile list
    /// </summary>
    /// <param name="inputActionAsset"></param>
    public void SaveProfile(InputActionAsset inputActionAsset)
    {
        inputBindings.Clear();

        foreach (var action in inputActionAsset)
        {
            foreach (var binding in action.bindings)
            {
                inputBindings.Add(binding);
            }
        }
    }
}
