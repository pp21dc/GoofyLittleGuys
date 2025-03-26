using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractCanvas : MonoBehaviour
{
	[SerializeField] private int playerIndex;
	[Header("UI References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private Image icon;
	[ColoredGroup][SerializeField] private TMP_Text keyboardSymbol;

	public int PlayerIndex => playerIndex;
	public Image Icon { get =>  icon; set => icon = value; }
	public TMP_Text KeyboardSymbol { get => keyboardSymbol; set => keyboardSymbol = value; }
}
