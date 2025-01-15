using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUi : MonoBehaviour
{
    List<LilGuyPopout> popouts;

    [SerializeField] Slider persistentHealthBar;
    [SerializeField] Image persistentIcon;
    [SerializeField] Image persistentAbilityIcon;
    [SerializeField] TMP_Text berryCountText;

    
    [SerializeField] PlayerBody pb;

    [SerializeField] TMP_Text STR_Txt;
    [SerializeField] TMP_Text SPD_Txt;
    [SerializeField] TMP_Text DEF_Txt;
    [SerializeField] TMP_Text LVL_Txt;
    [SerializeField] TMP_Text HP_Txt;

    [SerializeField] Slider XP_Slider;
    TMP_Text levelHPBar;

    [SerializeField] GameObject tempWinText;
    public GameObject TempWinText { get => tempWinText; set => tempWinText = value; }
	public RectTransform mirroredXUi; // Assign this in the inspector

    private void Update()
    {
        if (pb.LilGuyTeam.Count <= 0) return;
        STR_Txt.text = "STR: " + pb.LilGuyTeam[0].Strength.ToString();
        SPD_Txt.text = "SPD: " + pb.LilGuyTeam[0].Speed.ToString();
        DEF_Txt.text = "DEF: " + pb.LilGuyTeam[0].Defense.ToString();
        //LVL_Txt.text = "LVL: " + pb.LilGuyTeam[0].Level.ToString();
        LVL_Txt.text = pb.LilGuyTeam[0].Level.ToString();
        HP_Txt.text =  "HP: "  + pb.LilGuyTeam[0].Health.ToString() + " / " + pb.LilGuyTeam[0].MaxHealth.ToString();
        XP_Slider.maxValue = pb.LilGuyTeam[0].MaxHealth;
        XP_Slider.value = pb.LilGuyTeam[0].Xp;

		if (pb.Controller.PlayerCam != null && mirroredXUi != null)
		{
			// Check if the camera's x position in the viewport is less than 0.5
			if (pb.Controller.PlayerCam.rect.x < 0.5f)
			{
				// Anchor UI to the upper left
				mirroredXUi.anchorMin = new Vector2(0, 1); // Upper left corner
				mirroredXUi.anchorMax = new Vector2(0, 1); // Upper left corner
				mirroredXUi.anchoredPosition = Vector2.zero; // Position at (0,0)
                mirroredXUi.pivot = new Vector2(0, 1); // Upper left corner
			}
			else
			{
				// Anchor UI to the upper right
				mirroredXUi.anchorMin = new Vector2(1, 1); // Upper right corner
				mirroredXUi.anchorMax = new Vector2(1, 1); // Upper right corner
				mirroredXUi.anchoredPosition = Vector2.zero;
				mirroredXUi.pivot = new Vector2(1, 1); // Upper right corner
			}
		}
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
        //value = value * 0.75f;                                              //Sets Value to percentage of HP Bar max

        persistentHealthBar.value = value;
    }

    public void SetPersistentIcon(Sprite newIcon)
    {
        persistentIcon.sprite = newIcon;
    }

    public void SetPersistentAbilityIcon(Sprite newAbilityIcon)
    {
        persistentAbilityIcon.sprite = newAbilityIcon;
    }

    public void SetHealthBarLevel(int level)
    {

    }


}



