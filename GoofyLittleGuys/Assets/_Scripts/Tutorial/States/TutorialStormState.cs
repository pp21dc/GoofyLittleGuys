using UnityEngine;

public class TutorialStormState : TutorialState
{
    public TutorialStormState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        stateMachine.Island.storm.SetActive(true);
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
        if (stateMachine.Player.LilGuyTeam[0].IsDying)
        {
            base.CheckSectionComplete();
            stateMachine.Island.storm.SetActive(false);
        }
    }
}