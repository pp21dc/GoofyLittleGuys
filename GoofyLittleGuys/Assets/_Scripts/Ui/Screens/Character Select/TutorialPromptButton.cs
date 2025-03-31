using UnityEngine;

public class TutorialPromptButton : MonoBehaviour
{
    public void TutorialNotPressed()
    {
        Managers.UiManager.Instance.PlayButtonPressSfx();
        EventManager.Instance.CallLilGuyLockedInEvent();
    }

    public void TutorialPressed()
    {
        Managers.UiManager.Instance.PlayButtonPressSfx();
        EventManager.Instance.CallTutorialEvent();
    }
}