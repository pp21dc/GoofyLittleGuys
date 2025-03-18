
public class TutorialSwapState : TutorialState
{
    public TutorialSwapState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    private LilGuyBase lastFrameLilG;

    public override void Enter()
    {
        base.Enter();

        lastFrameLilG = stateMachine.Player.LilGuyTeam[0];
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (!complete) CheckSectionComplete();
        
        lastFrameLilG = stateMachine.Player.LilGuyTeam[0];
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void CheckSectionComplete()
    {
        if (lastFrameLilG != stateMachine.Player.LilGuyTeam[0])
            base.CheckSectionComplete();
    }
}
