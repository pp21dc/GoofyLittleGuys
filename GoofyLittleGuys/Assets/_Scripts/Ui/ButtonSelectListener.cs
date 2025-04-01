using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSelectListener : MonoBehaviour
{
    public void OnButtonSelect() 
    {
        Managers.UiManager.Instance.PlayButtonHighlightSfx();
    }
}
