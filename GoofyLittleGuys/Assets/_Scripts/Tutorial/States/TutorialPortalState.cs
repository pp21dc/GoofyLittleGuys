
public class TutorialPortalState : TutorialState
{
    public TutorialPortalState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        stateMachine.Island.exitPortal.SetActive(true);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void CheckSectionComplete()
    {
        base.CheckSectionComplete();
    }
}