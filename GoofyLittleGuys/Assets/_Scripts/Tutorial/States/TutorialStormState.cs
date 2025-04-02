using Managers;
using Unity.VisualScripting;
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
        GameManager.Instance.Phase2CloudAnim.SetTrigger("Phase2");
    }

    public override void Exit()
    {
        base.Exit();
        stateMachine.Player.StormHurtFx.SetActive(false);
        stateMachine.Player.InStorm = false;
		GameManager.Instance.Phase2CloudAnim.SetTrigger("Revert");
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
        if (stateMachine.Player.LilGuyTeam.Count <= 1) return;
        if (stateMachine.Player.LilGuyTeam[1].IsDead && stateMachine.Player.LilGuyTeam[0].Health < stateMachine.Player.LilGuyTeam[0].MaxHealth)
        {
            base.CheckSectionComplete();
            stateMachine.Island.storm.SetActive(false);
            stateMachine.Player.InStorm = false;
        }
    }
}