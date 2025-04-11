using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    public class RebindMapAssigner : MonoBehaviour
    {
        [SerializeField] private List<RebindActionUI> rebinders;

        private void OnEnable()
        {
            var pP = UiManager.Instance.PlayerWhoPaused;
            foreach (var rebinder in rebinders)
            {
                rebinder.m_PlayerInput = pP;
            }
        }
    }
}
