using UnityEngine;

public class DefenseType : LilGuyBase
{
    private Transform attackPos;
    private GameObject spawnedShieldObj = null;    // The actual instantiated shield object on the lil guy
    private bool isShieldActive = false;

    [SerializeField] private GameObject shieldPrefab; // The shield prefab to instantiate
    [SerializeField] private float duration = 1;
	[SerializeField] private float damageReduction = 0.5f;     // Change to damageReduction
	// Insert variable 3 that can vary between lil guys
	public GameObject SpawnedShieldObj { get { return spawnedShieldObj; } set { spawnedShieldObj = value; } }
    public bool IsShieldActive { get { return isShieldActive; } set { isShieldActive = value; } }
    public float DamageReduction { get { return damageReduction; } }

	public DefenseType(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength) : base(guyName, heath, maxHealth, type, speed, stamina, strength)
    {
    }

    public override void Special()
	{
		//TODO: ADD DEFENSE SPECIAL ATTACK
		if (currentCharges <= 0 && cooldownTimer > 0) return;
		spawnedShieldObj ??= Instantiate(shieldPrefab, transform.position, Quaternion.identity, transform); // If spawnShieldObj is null, assign it this instantiated GO
        spawnedShieldObj.GetComponent<Shield>().Initialize(duration, this);
        isShieldActive = true;


		cooldownTimer = cooldownDuration;
		chargeTimer = chargeRefreshRate;
        currentCharges--;

	}
}
