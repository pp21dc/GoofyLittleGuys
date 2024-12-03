using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUi : MonoBehaviour
{
    List<LilGuyPopout> popouts;

    [SerializeField] Image persistentHealthBar;
    [SerializeField] Image persistentIcon;
    [SerializeField] Image persistentAbilityIcon;
    [SerializeField] TMP_Text berryCountText;

    [SerializeField] PlayerBody pb;

    [SerializeField] TMP_Text STR_Txt;
    [SerializeField] TMP_Text SPD_Txt;
    [SerializeField] TMP_Text DEF_Txt;
    [SerializeField] TMP_Text LVL_Txt;
    [SerializeField] TMP_Text HP_Txt;
    private void Update()
    {
        STR_Txt.text = "STR: " + pb.LilGuyTeam[0].Strength.ToString();
        SPD_Txt.text = "SPD: " + pb.LilGuyTeam[0].Speed.ToString();
        DEF_Txt.text = "DEF: " + pb.LilGuyTeam[0].Defense.ToString();
        LVL_Txt.text = "LVL: " + pb.LilGuyTeam[0].Level.ToString();
        HP_Txt.text =  "HP: "  + pb.LilGuyTeam[0].Health.ToString() + " / " + pb.LilGuyTeam[0].MaxHealth.ToString();
    }
    public void SetBerryCount(int count)
    {
        berryCountText.text = $"{count}/3";
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



