using UnityEngine;

public class TutorialPromptButton : MonoBehaviour
{
    public void TutorialNotPressed()
    {
        EventManager.Instance.CallLilGuyLockedInEvent();
    }

    public void TutorialPressed()
    {
        EventManager.Instance.CallTutorialEvent();
    }
}