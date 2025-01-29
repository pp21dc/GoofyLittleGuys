using UnityEngine;
using UnityEngine.InputSystem;

// AUTH: thomas berner

/// <summary>
/// Place this on the player controller prefab,
/// uses InputProfiles to overwrite input bindings.
/// allows each player to have unique bindings without multiple input action maps
/// </summary>
public class PlayerProfileHandler : MonoBehaviour
{
    public InputProfile playerProfile;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            Debug.LogError($"INPUT PROFILE MANAGER IS ON WRONG OBJECT! CURRENTLY ON {gameObject.name}." +
                           $" PLEASE PUT IT ON A GAME OBJECT WITH THE PLAYER INPUT COMPONENT");
        }
        ApplyProfileBinds();
    }

    /// <summary>
    /// this goes through all the saved binds in order of the action map binds and overrides them with the selected profile
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
