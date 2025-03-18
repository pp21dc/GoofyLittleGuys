using System.Collections;
using UnityEngine;

public class TutorialAttackState : TutorialState
{
    public TutorialAttackState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }

    private LilGuyBase targetLilG;

    public override void Enter()
    {
        base.Enter();
        
        stateMachine.Player.transform.position = stateMachine.Island.spawnPoint.position;

        var lilG = Object.Instantiate(stateMachine.Island.lilGuyPref, stateMachine.Island.enemySpawnPoint.position, Quaternion.identity);
        stateMachine.Island.enemies.Add(lilG);
        targetLilG = lilG.GetComponent<LilGuyBase>();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (!targetLilG)
        {
            if (stateMachine.Island.enemies.Count > 0) 
                targetLilG = stateMachine.Island.enemies[0].GetComponent<LilGuyBase>();
            else
            {
                var lilG = Object.Instantiate(stateMachine.Island.lilGuyPref, stateMachine.Island.enemySpawnPoint.position, Quaternion.identity);
                stateMachine.Island.enemies.Add(lilG);
                targetLilG = lilG.GetComponent<LilGuyBase>();
            }
        }
        if (!complete) CheckSectionComplete();
    }

    public override void CheckSectionComplete()
    {
        if (targetLilG.Health < targetLilG.MaxHealth)
            base.CheckSectionComplete();
    }
}