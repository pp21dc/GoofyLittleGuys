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
    [SerializeField] TMP_Text berryCooldownTime;
    [SerializeField] Image berryCooldownSlider;

    [SerializeField] PlayerBody pb;

    [SerializeField] TMP_Text STR_Txt;
    [SerializeField] TMP_Text SPD_Txt;
    [SerializeField] TMP_Text DEF_Txt;
    [SerializeField] TMP_Text LVL_Txt;
    [SerializeField] TMP_Text HP_Txt;

    [SerializeField] Image CurrentCharacter;
    [SerializeField] Image LBCharacter;
    [SerializeField] Image RBCharacter;

    [SerializeField] Slider XP_Slider;
    TMP_Text levelHPBar;

    [SerializeField] List<Sprite> iconSprites;

    [SerializeField] GameObject tempWinText;

    enum LilGuys
    {
        Teddy,
        Spricket,
        Armordillo,
        Phant,
        Fishbowl,
        Turterium,
        Mousecar,
        Toadstool,
        Tricerabox
    }
    public GameObject TempWinText { get => tempWinText; set => tempWinText = value; }
    public RectTransform mirroredXUi; // Assign this in the inspector

	private void Update()
    {
        if (pb.NextBerryUseTime > 0)
        {
			berryCooldownTime.text = pb.NextBerryUseTime.ToString("0.0");
			berryCooldownSlider.fillAmount = pb.NextBerryUseTime / pb.BerryCooldown;
            berryCooldownTime.enabled = true;
		}
        else
        {
            berryCooldownSlider.fillAmount = 0;
            berryCooldownTime.enabled = false;
        }
        if (pb.LilGuyTeam.Count <= 0) return;
        STR_Txt.text = "STR: " + pb.LilGuyTeam[0].Strength.ToString();
        SPD_Txt.text = "SPD: " + pb.LilGuyTeam[0].Speed.ToString();
        DEF_Txt.text = "DEF: " + pb.LilGuyTeam[0].Defense.ToString();
        LVL_Txt.text = pb.LilGuyTeam[0].Level.ToString();
        HP_Txt.text = "HP: " + pb.LilGuyTeam[0].Health.ToString() + " / " + pb.LilGuyTeam[0].MaxHealth.ToString();
        XP_Slider.maxValue = pb.LilGuyTeam[0].MaxXp;
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

        //Setting the UI Images - needs to be redone for efficency


        //Current
        if (pb.LilGuyTeam[0].GuyName == "Teddy")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Teddy];
        else if (pb.LilGuyTeam[0].GuyName == "Spricket")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Spricket];
        else if (pb.LilGuyTeam[0].GuyName == "Armordillo")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Armordillo];
        else if (pb.LilGuyTeam[0].GuyName == "Phant-a-phant")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Phant];
        else if (pb.LilGuyTeam[0].GuyName == "Fishbowl")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Fishbowl];
        else if (pb.LilGuyTeam[0].GuyName == "Turteriam")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Turterium];
        else if (pb.LilGuyTeam[0].GuyName == "Mousecar")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Mousecar];
        else if (pb.LilGuyTeam[0].GuyName == "Toadstool")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Toadstool];
        else if (pb.LilGuyTeam[0].GuyName == "Tricera-box")
            CurrentCharacter.sprite = iconSprites[(int)LilGuys.Tricerabox];
        //LB
        if (pb.LilGuyTeam.Count <= 1) return;

        if (pb.LilGuyTeam[1].GuyName == "Teddy")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Teddy];
        else if (pb.LilGuyTeam[1].GuyName == "Spricket")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Spricket];
        else if (pb.LilGuyTeam[1].GuyName == "Armordillo")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Armordillo];
        else if (pb.LilGuyTeam[1].GuyName == "Phant-a-phant")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Phant];
        else if (pb.LilGuyTeam[1].GuyName == "Fishbowl")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Fishbowl];
        else if (pb.LilGuyTeam[1].GuyName == "Turteriam")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Turterium];
        else if (pb.LilGuyTeam[1].GuyName == "Mousecar")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Mousecar];
        else if (pb.LilGuyTeam[1].GuyName == "Toadstool")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Toadstool];
        else if (pb.LilGuyTeam[1].GuyName == "Tricera-box")
            LBCharacter.sprite = iconSprites[(int)LilGuys.Tricerabox];
		//RB
		if (pb.LilGuyTeam.Count <= 2) return;
		if (pb.LilGuyTeam[2].GuyName == "Teddy")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Teddy];
        else if (pb.LilGuyTeam[2].GuyName == "Spricket")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Spricket];
        else if (pb.LilGuyTeam[2].GuyName == "Armordillo")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Armordillo];
        else if (pb.LilGuyTeam[2].GuyName == "Phant-a-phant")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Phant];
        else if (pb.LilGuyTeam[2].GuyName == "Fishbowl")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Fishbowl];
        else if (pb.LilGuyTeam[2].GuyName == "Turteriam")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Turterium];
        else if (pb.LilGuyTeam[2].GuyName == "Mousecar")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Mousecar];
        else if (pb.LilGuyTeam[2].GuyName == "Toadstool")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Toadstool];
        else if (pb.LilGuyTeam[2].GuyName == "Tricera-box")
            RBCharacter.sprite = iconSprites[(int)LilGuys.Tricerabox];
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
        value = value / maxHealth;                                          //sets value to the hp %                                              //Sets Value to percentage of HP Bar max

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


}



