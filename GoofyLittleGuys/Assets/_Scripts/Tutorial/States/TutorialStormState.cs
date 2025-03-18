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
        // one lil g dead other took dmg
        if (stateMachine.Player.LilGuyTeam[1].Health > 0 && stateMachine.Player.LilGuyTeam[0].Health < stateMachine.Player.LilGuyTeam[0].MaxHealth)
        {
            base.CheckSectionComplete();
            stateMachine.Island.storm.SetActive(false);
        }
    }
}