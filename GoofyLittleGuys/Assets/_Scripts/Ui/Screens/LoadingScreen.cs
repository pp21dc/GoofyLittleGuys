using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image loadingBar;
    [SerializeField] private TMP_Text loadProgress;

    public Image LoadingBar => loadingBar;
    public TMP_Text LoadProgress => loadProgress;
}
