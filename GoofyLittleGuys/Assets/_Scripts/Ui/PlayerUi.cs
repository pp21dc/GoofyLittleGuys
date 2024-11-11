using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerUi : MonoBehaviour
{
    List<LilGuyPopout> popouts;

    [SerializeField] Image persistentHealthBar;
    [SerializeField] Image persistentIcon;
    [SerializeField] Image persistentAbilityIcon;
    
    
    LilGuyPopout GetLilGuyPopout(int index)                                  //Not 100% when this would be needed - hence it not being implemented yet
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
    public void SetPersistentHealthBarValue(float value, float maxHealth)
    {
        value = value / maxHealth;                                          //sets value to the hp %
        value = value * 0.75f;                                              //Sets Value to percentage of HP Bar max

        persistentHealthBar.fillAmount = value;
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



