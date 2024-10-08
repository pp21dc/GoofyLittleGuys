using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerUi : MonoBehaviour
{
    List<LilGuyPopout> popouts;

    Image persistentHealthBar;
    Image persistentIcon;
    Image persistentAbilityIcon;
    
    
    LilGuyPopout GetLilGuyPopout(int index)
    {
        throw new NotImplementedException();
    }

    //setters
    public void SetLilGuyStats(int index, int str, int spd, int def)
    {
        popouts[index].SetStr(str);
        popouts[index].SetSpd(spd);
        popouts[index].SetDef(def);
    }
    public void SetPersistentHealthBarValue(int value)
    {
        throw new NotImplementedException();
    }

    public void SetPersistentIcon(Sprite newIcon)
    {
        persistentIcon.sprite = newIcon;
    }

    public void SetPersistentAbilityIcon(Sprite newAbilityIcon)
    {
        persistentAbilityIcon.sprite = newAbilityIcon;
    }


}



