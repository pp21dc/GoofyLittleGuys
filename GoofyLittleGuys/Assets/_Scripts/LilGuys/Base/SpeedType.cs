using UnityEngine;

[RequireComponent (typeof(DashSpecialMove))]
public class SpeedType : LilGuyBase
{
    private Transform attackPos;
	[SerializeField]
	private DashSpecialMove dashSpecialMove;
	public SpeedType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
    {
    }

    public override void Special()
    {
        //TODO: ADD SPEED SPECIAL ATTACK
        //base.Special();
        dashSpecialMove.OnSpecialUsed();
    }
}
