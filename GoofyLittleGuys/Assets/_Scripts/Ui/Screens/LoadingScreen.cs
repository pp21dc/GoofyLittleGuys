using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image loadingBar;
    [SerializeField] private Image loadingBackground;
    [SerializeField] private Slider loadingBarOption2;
    [SerializeField] private TMP_Text loadProgress;

    [SerializeField] private Sprite[] loadingScreenBackgrounds;

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
