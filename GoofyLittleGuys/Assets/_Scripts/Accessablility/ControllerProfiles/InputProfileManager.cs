using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputProfileManager : MonoBehaviour
{
    public InputProfile playerProfile;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        ApplyProfileBinds();
    }

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
