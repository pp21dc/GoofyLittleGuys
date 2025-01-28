using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "newInputProfile", menuName = "InputProfile")]
public class InputProfile : ScriptableObject
{
    public string ProfileName;
    public List<InputBinding> inputBindings = new List<InputBinding>();
}
