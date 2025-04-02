using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

//AUTH: thomas berner

/// <summary>
/// Scriptable for holding input bindings for individual players to overwrite the default action map
/// </summary>

[CreateAssetMenu(fileName = "newInputProfile", menuName = "InputProfile")]
[Serializable]
public class InputProfile : ScriptableObject
{
    public string ProfileName;
    public InputActionAsset asset;
    public Color profileColor = new  Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    public List<InputBinding> inputBindings = new List<InputBinding>();
    
    /// <summary>
    /// Save bindings from input asset to this input profile list
    /// </summary>
    /// <param name="inputActionAsset"></param>
    public void SaveProfile()
    {
        inputBindings.Clear();

        foreach (var action in asset)
        {
            foreach (var binding in action.bindings)
            {
                inputBindings.Add(binding);
            }
        }
    }
}
