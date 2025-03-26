public class TutorialGetBerryState : TutorialState
{
    public TutorialGetBerryState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (!complete) CheckSectionComplete();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void CheckSectionComplete()
    {
        if (stateMachine.Player.BerryCount > 0)
            base.CheckSectionComplete();
    }
}