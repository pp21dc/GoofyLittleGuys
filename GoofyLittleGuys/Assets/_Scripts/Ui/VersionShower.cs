using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class VersionShower : MonoBehaviour
{
    private void Awake()
    {
        string outText = ("Version: " + Application.version);

        if (TryGetComponent(out TMP_Text output))
        {
            output.text = outText;
        }
    }
}
