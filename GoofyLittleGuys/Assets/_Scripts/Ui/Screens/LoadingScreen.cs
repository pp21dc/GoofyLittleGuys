using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("References")]
    [HorizontalRule]
	[SerializeField] private Sprite[] loadingScreenBackgrounds;
	[ColoredGroup][SerializeField] private Image loadingBar;
	[ColoredGroup][SerializeField] private Image loadingBackground;
	[ColoredGroup][SerializeField] private Slider loadingBarOption2;
	[ColoredGroup][SerializeField] private TMP_Text loadProgress;


    public Image LoadingBar => loadingBar;
    public Slider LoadingBarOption2 => loadingBarOption2;
    public TMP_Text LoadProgress => loadProgress;
    public Sprite[] LoadProgressBGs => loadingScreenBackgrounds;
    public Image LoadingBackground => loadingBackground;

	private void OnEnable()
	{
		LoadingBarOption2.value = 0;
		LoadingBackground.sprite = LoadProgressBGs[0];
		LoadProgress.text = "0%";
	}
}
