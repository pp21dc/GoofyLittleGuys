using UnityEngine;

public class DefenseType : LilGuyBase
{
    private Transform attackPos;

    public DefenseType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
    {
    }

    public override void Special()
    {
        //TODO: ADD DEFENSE SPECIAL ATTACK
        base.Special();
    }
}
