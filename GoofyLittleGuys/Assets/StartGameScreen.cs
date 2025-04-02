using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartGameScreen : MonoBehaviour
{
	[Header("References")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private Image timerImage;
	[ColoredGroup][SerializeField] private TMP_Text timerText;
	
	public Image TimerImage { get => timerImage; set => timerImage = value;}
	public TMP_Text TimerText { get => timerText; set => timerText = value;}
}
