using UnityEngine;

[RequireComponent (typeof(AoeSpecialMove))]
public class StrengthType : LilGuyBase
{
    private Transform attackPos;
    [SerializeField]
    private AoeSpecialMove attackSpecial;

	public StrengthType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
    {
    }

    public override void Special()
    {
        //TODO: ADD STRENGTH SPECIAL ATTACK
        base.Special();
        attackSpecial.OnSpecialUsed();
    }
}
