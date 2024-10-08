using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class LilGuyPopout : PlayerUi
{
    TMP_Text str;
    TMP_Text spd;
    TMP_Text def;
    TMP_Text lilGuyName;

    Sprite healthBar; //May switch to slider
    Image icon;

    public void SetStr(int value)
    {
        str.text = "STR: " + value;
    }

    public void SetSpd(int value)
    {
        spd.text = "SPD: " + value;
    }
    public void SetDef(int value)
    {
        def.text = "DEF: " + value;
    }

    public void SetLilGuyName(string name)
    {
        lilGuyName.text = name;
    }

    public void SetHealthBar(int value) //Not 100% how the health bars are going to work yet
    {
        throw new NotImplementedException();
    }
    public void SetIcon(Sprite newIcon)
    {
        icon.sprite = newIcon;
    }
}
