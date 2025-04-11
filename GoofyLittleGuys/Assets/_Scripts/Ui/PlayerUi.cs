using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;

public class PlayerUi : MonoBehaviour
{
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

	[Header("General References")]
	[HorizontalRule]
	[SerializeField] List<GameObject> livingUI; // All the UI GOs that should get disabled on DEATH
	[ColoredGroup][SerializeField] PlayerBody pb;
	[ColoredGroup][SerializeField] GameObject panel;

	[Header("Player Health UI References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private Image healthBarFill;
	[ColoredGroup][SerializeField] private Image playerShape;
	[ColoredGroup][SerializeField] TMP_Text LVL_Txt;
	[ColoredGroup][SerializeField] Slider persistentHealthBar;
	[ColoredGroup][SerializeField] Slider XP_Slider;

	[Header("Ability & Swap UI References")]
	[HorizontalRule]
	[SerializeField] List<Sprite> iconSprites;
	[SerializeField] List<Sprite> abilitySprites;
	[ColoredGroup][SerializeField] Sprite transparentSprite;
	[ColoredGroup][SerializeField] Image AbilityIcon;
	[ColoredGroup][SerializeField] Image abilityCooldownTimer;
	[ColoredGroup][SerializeField] TMP_Text abilityCooldownText;
	[ColoredGroup][SerializeField] GameObject LBIcon;
	[ColoredGroup][SerializeField] GameObject RBIcon;
	[ColoredGroup][SerializeField] RectTransform LB;
	[ColoredGroup][SerializeField] RectTransform RB;
	[ColoredGroup][SerializeField] Image LBCharacter;
	[ColoredGroup][SerializeField] Image RBCharacter;

	[Header("Berry UI References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] TMP_Text berryCountText;
	[ColoredGroup][SerializeField] TMP_Text berryCooldownTime;
	[ColoredGroup][SerializeField] Image berryCooldownSlider;
	[ColoredGroup][SerializeField] GameObject berryCountTxt;
	[ColoredGroup][SerializeField] GameObject berryCooldownTxt;

	[Header("Victory UI References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] GameObject tempWinText;
	[ColoredGroup][SerializeField] VictoryAnimationPlay victoryAnim;
	[ColoredGroup][SerializeField] GameObject victoryObject;
	[ColoredGroup][SerializeField] GameObject respawnScreen;
	[ColoredGroup][SerializeField] GameObject defeatedScreen;
	[ColoredGroup][SerializeField] StartGameScreen startGameScreen;


	List<LilGuyPopout> popouts;
	TMP_Text STR_Txt;
	TMP_Text SPD_Txt;
	TMP_Text DEF_Txt;
	TMP_Text HP_Txt;
	TMP_Text levelHPBar;



	public GameObject TempWinText { get => tempWinText; set => tempWinText = value; }
	public StartGameScreen StartGameScreen => startGameScreen;

	private void Awake()
	{
		EventManager.Instance.NotifyStartAbilityCooldown += SetCooldownIndicator;
		EventManager.Instance.NotifyUiSwap += RefreshIcons;
	}

	public void DisablePlayerUI()
	{
		foreach (GameObject g in livingUI)
		{
			g.SetActive(false);
		}
		defeatedScreen.SetActive(true);
	}

	public void SetColour()
	{
		healthBarFill.color = pb.PlayerColour;
		playerShape.sprite = UiManager.Instance.shapes[pb.Controller.PlayerNumber - 1];
	}

	public void MirrorUI(bool shouldMirror)
	{

		if (shouldMirror)
		{
			FlipUi();
			panel.transform.localScale = new Vector3(-1, 1, 1);
		}

	}

	private void Update()
	{
		if (pb.LilGuyTeam.Count <= 0) return;
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
		//  STR_Txt.text = "STR: " + pb.LilGuyTeam[0].Strength.ToString();
		//  SPD_Txt.text = "SPD: " + pb.LilGuyTeam[0].Speed.ToString();
		//  DEF_Txt.text = "DEF: " + pb.LilGuyTeam[0].Defense.ToString();
		LVL_Txt.text = pb.LilGuyTeam[0].Level.ToString();
		//  HP_Txt.text = "HP: " + pb.LilGuyTeam[0].Health.ToString() + " / " + pb.LilGuyTeam[0].MaxHealth.ToString();
		XP_Slider.maxValue = pb.LilGuyTeam[0].MaxXp;
		XP_Slider.value = pb.LilGuyTeam[0].Xp;
		if (Input.GetKeyDown("k")) { VictoryAnimPlay(); }

	}
	private void RefreshIcons(PlayerUi playerUi, float swapDirection)
	{
		Managers.DebugManager.Log("Swap Direction: " + swapDirection, DebugManager.DebugCategory.UI);
		if (playerUi == this)
		{
			SetIcons();
		}
	}

	private void SetIcons()
	{
		//CurrentCharacter.sprite = pb.LilGuyTeam[0].Icon;
		AbilityIcon.sprite = pb.LilGuyTeam[0].AbilityIcon;

		if (pb.LilGuyTeam.Count > 1) LBCharacter.sprite = pb.LilGuyTeam[1].Health > 0 ? pb.LilGuyTeam[1].Icon : pb.LilGuyTeam[1].MonochromeIcon;
		else LBCharacter.sprite = transparentSprite;

		if (pb.LilGuyTeam.Count > 2) RBCharacter.sprite = pb.LilGuyTeam[2].Health > 0 ? pb.LilGuyTeam[2].Icon : pb.LilGuyTeam[2].MonochromeIcon;
		else RBCharacter.sprite = transparentSprite;

	}

	public void ShowRespawnScreen()
	{
		respawnScreen.SetActive(true);
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

	private void SetCooldownIndicator(PlayerUi playerUi, float cooldownLength)
	{
		if (playerUi == this)
		{
			StartCoroutine(AbilityCooldownTimer(cooldownLength));
		}
	}

	private IEnumerator AbilityCooldownTimer(float cooldownLength)
	{
		float timer = cooldownLength;

		while (timer > 0)
		{
			timer -= Time.deltaTime;
			abilityCooldownTimer.fillAmount = timer / cooldownLength;
			abilityCooldownText.text = ((int)timer + 1).ToString();
			if (timer <= 0)
				abilityCooldownText.text = " ";
			yield return null;
		}

	}

	private void FlipUi()
	{
		//LVL_Txt.transform.localScale = new Vector3(-1, 1, 1);
		abilityCooldownText.transform.localScale = new Vector3(-1, 1, 1);
		LBIcon.transform.localScale = new Vector3((float)-0.65528, (float)0.65528, (float)0.65528);
		RBIcon.transform.localScale = new Vector3((float)-0.65528, (float)0.65528, (float)0.65528);
		berryCooldownTxt.transform.localScale = new Vector3((float)-1, (float)1, (float)1);
		berryCountTxt.transform.localScale = new Vector3((float)-1, (float)1, (float)1);
		victoryObject.transform.localScale = new Vector3((float)-1, (float)1, (float)1);

		// Swap their anchored positions
		LB.SetSiblingIndex(2);
		RB.SetSiblingIndex(1);
	}

	public void ResetCDTimer()
	{
		Managers.DebugManager.Log("ResetCDTimer", DebugManager.DebugCategory.UI);
		StopAllCoroutines();
		abilityCooldownText.text = " ";
		abilityCooldownTimer.fillAmount = 0;

	}

	public void VictoryAnimPlay()
	{
		StartCoroutine(VictoryAnimHoldSortingOrder());
		//victoryObject.SetActive(true);
		victoryAnim.PlayAnimations();
	}

	IEnumerator VictoryAnimHoldSortingOrder()
	{
		Canvas canvas = GetComponent<Canvas>();

		float timer = 10;
		float i = 0;
		while (i < timer)
		{
			canvas.sortingOrder = pb.LilGuyTeam[0].Mesh.sortingOrder - 1;
			i += Time.deltaTime;
			yield return null;
		}
	}
}



