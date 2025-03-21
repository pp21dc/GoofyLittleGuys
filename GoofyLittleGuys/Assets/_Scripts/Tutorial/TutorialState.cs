using Managers;
using UnityEngine;

public class TutorialState : IState
{
    protected TutorialStateMachine stateMachine;
    
    public TutorialState(TutorialStateMachine tutorialStateMachine)
    {
        stateMachine = tutorialStateMachine;
    }

    protected bool complete = false;
    
    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) //TODO: remove later when tutorial is tested a lot more
        {
            var index = TutorialManager.Instance.IslandsComplete.FindIndex(b => !b); // find the first false index
            if (index != -1) // would return -1 in the case there isn't a false value
            {
                DebugManager.Log("Tutorial Stage Complete");
                TutorialManager.Instance.IslandsComplete[index] = true;
                complete = true;
            }
            TutorialManager.Instance.CheckComplete();
        }
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void CheckSectionComplete()
    {
        var index = TutorialManager.Instance.IslandsComplete.FindIndex(b => !b); // find the first false index
        if (index != -1) // would return -1 in the case there isn't a false value
        {
            DebugManager.Log("Tutorial Stage Complete");
            TutorialManager.Instance.IslandsComplete[index] = true;
            complete = true;
        }
        TutorialManager.Instance.CheckComplete();
    }
}