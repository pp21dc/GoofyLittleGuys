
using System.Linq;

public class TutorialFountainState : TutorialState
{
    public TutorialFountainState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    //
    
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
        if (stateMachine.Player.LilGuyTeam.All(guy => guy.Health > 0))
            base.CheckSectionComplete();
    }
}