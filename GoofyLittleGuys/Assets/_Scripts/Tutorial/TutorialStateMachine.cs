public class TutorialStateMachine : AStateMachine
{
    public PlayerBody Player { get; }
    public TutorialIsland Island { get; }
    public bool CurrentStateComplete { get; private set; } = false;

    public TutorialAttackState TutorialAttackState { get; private set; }
    public TutorialSpecialState TutorialSpecialState { get; private set; }
    public TutorialDefeatState TutorialDefeatState { get; private set; }
    public TutorialTameState TutorialTameState { get; private set; }
    public TutorialStormState TutorialStormState { get; private set; }
    public TutorialBerryState TutorialBerryState { get; private set; }
    public TutorialFountainState TutorialFountainState { get; private set; }
    public TutorialSwapState TutorialSwapState { get; private set; }
    public TutorialPortalState TutorialPortalState { get; private set; }

    public TutorialStateMachine(PlayerBody player, TutorialIsland island)
    {
        //REFERENCES
        Player = player;
        Island = island;
        
        // STATES
        TutorialAttackState = new TutorialAttackState(this);
        TutorialSpecialState = new TutorialSpecialState(this);
        TutorialDefeatState = new TutorialDefeatState(this);
        TutorialTameState = new TutorialTameState(this);
        TutorialStormState = new TutorialStormState(this);
        TutorialBerryState = new TutorialBerryState(this);
        TutorialFountainState = new TutorialFountainState(this);
        TutorialSwapState = new TutorialSwapState(this);
        TutorialPortalState = new TutorialPortalState(this);
    }
}
