using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// AUTH: thomas berner

/// <summary>
/// Place this on the player controller prefab to give functionality to write custom binds for each unique player without affecting all other players
/// </summary>
public class InputProfileManager : MonoBehaviour
{
    public InputProfile playerProfile;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            Debug.LogError("INPUT PROFILE MANAGER IS ON WRONG OBJECT! PLEASE PUT IT ON A GAMEOBJECT WITH THE PLAYERINPUT COMPONENT");
        }
        ApplyProfileBinds();
    }

    /// <summary>
    /// this goes through all the saved binds in order of the actionmap binds and overrides them with the selected profile
    /// </summary>
    private void ApplyProfileBinds()
    {
        if (playerProfile == null) return;

        var actions = playerInput.actions;
        foreach (var action in actions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (i < playerProfile.inputBindings.Count)
                    action.ApplyBindingOverride(i, playerProfile.inputBindings[i].effectivePath);
            }
        }
    }

    /// <summary>
    /// Call when player choses to change to a new profile.
    /// Replaces old with new InputProfile object
    /// </summary>
    /// <param name="newProfile"></param>
    public void SwitchProfile(InputProfile newProfile)
    {
        playerProfile = newProfile;
        ApplyProfileBinds();
    }
}
