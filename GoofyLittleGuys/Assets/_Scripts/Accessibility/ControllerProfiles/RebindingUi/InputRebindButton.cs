using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindButton : MonoBehaviour
{
    [SerializeField] private InputActionReference actionReference;
    private InputAction inputAction;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private void Awake()
    {
        inputAction = actionReference.action;
    }

    private void StartInteractiveRebind()
    {
        rebindingOperation = inputAction.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Gamepad>/Start")
            .WithControlsExcluding("<Gamepad>/LeftStick")
            .WithControlsExcluding("<Keyboard>/escape")
            .WithControlsExcluding("<Keyboard>/tab")
            .OnComplete(operation => RebindComplete());
        
        rebindingOperation.Start();
    }

    private void RebindComplete()
    {
        rebindingOperation.Dispose();
    }
}
