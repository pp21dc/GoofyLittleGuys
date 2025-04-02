using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindButton : MonoBehaviour
{
    [SerializeField] private InputAction inputAction;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

    private void StartInteractiveRebind()
    {
        _rebindingOperation = inputAction.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Gamepad>/Start")
            .WithControlsExcluding("<Gamepad>/LeftStick")
            .WithControlsExcluding("<Keyboard>/escape")
            .WithControlsExcluding("<Keyboard>/tab")
            .OnComplete(operation => RebindComplete());
        
        _rebindingOperation.Start();
    }

    private void RebindComplete()
    {
        _rebindingOperation.Dispose();
    }
}
