using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;

public class PlayerUi : MonoBehaviour
{
    List<LilGuyPopout> popouts;

    [SerializeField] GameObject panel;
    [SerializeField] Slider persistentHealthBar;
    [SerializeField] Image persistentIcon;
    [SerializeField] Image persistentAbilityIcon;
    [SerializeField] TMP_Text berryCountText;
    [SerializeField] TMP_Text berryCooldownTime;
    [SerializeField] Image berryCooldownSlider;

    [SerializeField] PlayerBody pb;

    TMP_Text STR_Txt;
    TMP_Text SPD_Txt;
    TMP_Text DEF_Txt;
    [SerializeField] TMP_Text LVL_Txt;
    TMP_Text HP_Txt;

    [SerializeField] Image CurrentCharacter;
    [SerializeField] Image LBCharacter;
    [SerializeField] Image RBCharacter;

    [SerializeField] Image AbilityIcon;
    [SerializeField] Image abilityCooldownTimer;
    [SerializeField] TMP_Text abilityCooldownText;

    [SerializeField] Slider XP_Slider;
    TMP_Text levelHPBar;

    [SerializeField] List<Sprite> iconSprites;
    [SerializeField] List<Sprite> abilitySprites;

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

    private void Start()
    {
        EventManager.Instance.NotifyStartAbilityCooldown += SetCooldownIndicator;
        EventManager.Instance.NotifyUiSwap += RefreshIcons;

        if (pb.Equals(GameManager.Instance.Players[1]) || pb.Equals(GameManager.Instance.Players[3]))
        {
            panel.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Update()
    {
        XP_Slider.maxValue = pb.LilGuyTeam[0].MaxXp;
        XP_Slider.value = pb.LilGuyTeam[0].Xp;
        LVL_Txt.text = pb.LilGuyTeam[0].Level.ToString();

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

        
    }
    private void RefreshIcons(PlayerUi playerUi, float swapDirection)
    {
        Debug.Log("Swap Direction: " + swapDirection);
        if(playerUi == this)
        {
            SetIcons();
        }
    }

    private void SetIcons()
    {
        CurrentCharacter.sprite = pb.LilGuyTeam[0].Icon;
        AbilityIcon.sprite = pb.LilGuyTeam[0].AbilityIcon;
        if (pb.LilGuyTeam.Count > 1)
            RBCharacter.sprite = pb.LilGuyTeam[1].Icon;
        if (pb.LilGuyTeam.Count > 2)
            LBCharacter.sprite = pb.LilGuyTeam[2].Icon;
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

    private void SetCooldownIndicator(PlayerUi playerUi, float cooldownLength)
    {
        if(playerUi == this)
        {
            StartCoroutine(AbilityCooldownTimer(cooldownLength));
        }
    }

    private IEnumerator AbilityCooldownTimer(float cooldownLength)
    {
        float timer = cooldownLength;

        while(timer > 0)
        {
            timer -= Time.deltaTime;
            abilityCooldownTimer.fillAmount = timer / cooldownLength;
            abilityCooldownText.text = ((int)timer+1).ToString();
            if (timer <= 0)
                abilityCooldownText.text = " ";
            yield return null;
        }
            
    }
}



