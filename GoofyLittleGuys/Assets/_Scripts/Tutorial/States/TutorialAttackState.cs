using System.Collections;
using Managers;
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

        var lilG = Object.Instantiate(stateMachine.Island.lilGuyPref, stateMachine.Island.enemySpawnPoint.position, Quaternion.identity, stateMachine.Island.transform);
        stateMachine.Island.enemies.Add(lilG);
        targetLilG = lilG.GetComponent<LilGuyBase>();
        targetLilG.GetComponent<TutorialBehaviour>().Home = stateMachine.Island.enemySpawnPoint;
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
                targetLilG.GetComponent<TutorialBehaviour>().Home = stateMachine.Island.enemySpawnPoint;
            }
        }
        if (!complete) CheckSectionComplete();
    }

    public override void CheckSectionComplete()
    {
        DebugManager.Log("checksectioncomplete");
        if (targetLilG.Health < targetLilG.MaxHealth)
            base.CheckSectionComplete();
    }
}