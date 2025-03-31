using UnityEngine;

public class TutorialSpecialState : TutorialState
{
    public TutorialSpecialState(TutorialStateMachine tutorialStateMachine) : base(tutorialStateMachine)
    {
    }
    
    private LilGuyBase targetLilG;

    public override void Enter()
    {
        base.Enter();
        
        if (stateMachine.Island.enemies.Count > 0) 
            targetLilG = stateMachine.Island.enemies[0].GetComponent<LilGuyBase>();
        else
        {
            var lilG = Object.Instantiate(stateMachine.Island.lilGuyPref, stateMachine.Island.enemySpawnPoint.position, Quaternion.identity, stateMachine.Island.transform);
            stateMachine.Island.enemies.Add(lilG);
            targetLilG = lilG.GetComponent<LilGuyBase>();
            targetLilG.GetComponent<TutorialBehaviour>().Home = stateMachine.Island.enemySpawnPoint;
        }
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

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void CheckSectionComplete()
    {
        if (stateMachine.Player.LilGuyTeam[0].IsInSpecialAttack)
            base.CheckSectionComplete();
    }
}
