using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using TMPro;

public class TutorialManager : SingletonBase<TutorialManager>
{
    [Serializable]
    public class TutorialText
    {
        public string text;
        public Sprite sprite;
    }

    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private List<TutorialText> tutorialTexts = new List<TutorialText>();
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Sprite buttonSprite;
    
    private List<TutorialStateMachine> tutorialStateMachines = new List<TutorialStateMachine>();
    private List<TutorialIsland> tutorialIslands = new List<TutorialIsland>();
    private List<bool> islandsComplete = new List<bool>();
    public List<bool> IslandsComplete => islandsComplete;

    private int _currentTutorialState = 0;

    public override void Awake()
    {
        base.Awake();

        for (var i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            tutorialStateMachines.Add(new TutorialStateMachine(GameManager.Instance.Players[i], tutorialIslands[i]));
            
            tutorialStateMachines[i].ChangeState(tutorialStateMachines[i].TutorialAttackState);
            
            islandsComplete.Add(false);
            
            GameManager.Instance.Players[i].GetComponent<Rigidbody>().MovePosition(spawnPoints[i].position);
        }
    }

    public void CheckComplete()
    {
        if (islandsComplete.All(o => o == true)) // if all elements in the list are true
        {
            ChangeAllStates();
            for (var i = 0; i < islandsComplete.Count; i++)
            {
                islandsComplete[i] = false;
            }
        }
    }

    private void SetTutorialText()
    {
        tutorialText.text = tutorialTexts[_currentTutorialState].text;
        buttonSprite = tutorialTexts[_currentTutorialState].sprite;
    }

    private void ChangeAllStates()
    {
        _currentTutorialState++;
        foreach (var tm in tutorialStateMachines)
        {
            switch (_currentTutorialState)
            {
                case 0:
                    tm.ChangeState(tm.TutorialAttackState);
                    break;
                case 1:
                    tm.ChangeState(tm.TutorialSpecialState);
                    break;
                case 2:
                    tm.ChangeState(tm.TutorialDefeatState);
                    break;
                case 3:
                    tm.ChangeState(tm.TutorialTameState);
                    break;
                case 4:
                    tm.ChangeState(tm.TutorialStormState);
                    break;
                case 5:
                    tm.ChangeState(tm.TutorialBerryState);
                    break;
                case 6:
                    tm.ChangeState(tm.TutorialFountainState);
                    break;
                case 7:
                    tm.ChangeState(tm.TutorialSwapState);
                    break;
                case 8:
                    tm.ChangeState(tm.TutorialPortalState);
                    break;
                case 9:
                    //transition to normal game
                    break;
            }
        }
        SetTutorialText();
    }
    
}
