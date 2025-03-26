
using UnityEngine;

public class TutorialBerryState : TutorialState
{
    public TutorialBerryState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    private int _lastFrameBerryCount = 0;
    
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
        
        _lastFrameBerryCount = stateMachine.Player.BerryCount;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void CheckSectionComplete()
    {
        if (stateMachine.Player.BerryCount < _lastFrameBerryCount || Mathf.Approximately(stateMachine.Player.ActiveLilGuy.Health, stateMachine.Player.ActiveLilGuy.MaxHealth))
            base.CheckSectionComplete();
    }
}