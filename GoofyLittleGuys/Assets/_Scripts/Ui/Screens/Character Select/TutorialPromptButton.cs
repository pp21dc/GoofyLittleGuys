using UnityEngine;

public class TutorialPromptButton : MonoBehaviour
{
    public void TutorialNotPressed()
    {
        Managers.UiManager.Instance.PlayButtonPressSfx();
		Managers.GameManager.Instance.MainMenuVolume.gameObject.SetActive(false);
		EventManager.Instance.CallLilGuyLockedInEvent();
    }

    public void TutorialPressed()
    {
        Managers.UiManager.Instance.PlayButtonPressSfx();
        Managers.GameManager.Instance.MainMenuVolume.gameObject.SetActive(false);
        EventManager.Instance.CallTutorialEvent();
    }
}