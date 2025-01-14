using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionShower : MonoBehaviour
{
    private void Awake()
    {
        if (TryGetComponent(out TMP_Text output))
        {
            output.text = Application.version;
        }
    }
}
