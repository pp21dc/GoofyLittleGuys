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
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void CheckSectionComplete()
    {
        complete = true;
        var index = TutorialManager.Instance.IslandsComplete.FindIndex(b => !b); // find the first false index
        if (index != -1) // would return -1 in the case there isn't a false value
        {
            TutorialManager.Instance.IslandsComplete[index] = true;
        }
        TutorialManager.Instance.CheckComplete();
    }
}