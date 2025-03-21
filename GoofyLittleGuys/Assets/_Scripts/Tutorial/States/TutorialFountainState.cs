
using System.Linq;
using UnityEngine;

public class TutorialFountainState : TutorialState
{
    public TutorialFountainState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    //
    
    public override void Enter()
    {
        base.Enter();
        
        //turn on the fountain component
        stateMachine.Island.fountain.GetComponent<TutorialFountain>().interactArea.enabled = true;
    }

    public override void Exit()
    {
        base.Exit();
        stateMachine.Island.fountain.GetComponent<TutorialFountain>().interactArea.enabled = false;
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
        if (stateMachine.Player.LilGuyTeam.All(guy => Mathf.Approximately(guy.Health, guy.MaxHealth)))
            base.CheckSectionComplete();
    }
}