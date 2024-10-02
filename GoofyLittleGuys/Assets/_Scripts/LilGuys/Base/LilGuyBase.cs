using System;
using UnityEngine;

public class LilGuyBase : MonoBehaviour
{
    //VARIABLES//
    public string guyName;
    public int heath;
    public int maxHeath;
    public PrimaryType type;
    public int speed;
    public int stamina;
    public int strength;
    private int average;
    public const int MAX_STAT = 100;
    private Transform attackPosition;
    [SerializeField] private GameObject hitboxPrefab;

    public enum PrimaryType
    {
        Strength,
        Stamina,
        Speed,
        AllAround
    }

    private void Awake()
    {
        attackPosition = gameObject.transform;
    }

    /// <summary>
    /// This is the basic attack across all lil guys\
    /// it uses a hitbox prefab to detect other ai within it and deal damage from that script
    /// NOTE: the second value in destroy (line 29) is the duration that the attack lasts
    /// </summary>
    public void Attack()
    {
        GameObject hitbox = Instantiate(hitboxPrefab, attackPosition.position + attackPosition.forward * 0.5f, Quaternion.identity);
        hitbox.transform.SetParent(transform);
        Destroy(hitbox, 1f);
    }

    /// <summary>
    /// override this function in all derivitives of this class with its unique special attack
    /// </summary>
    public virtual void Special() { throw new NotImplementedException(); }

    // Lil Guy constructor :3
    public LilGuyBase(string guyName, int heath, int maxHealth, PrimaryType type, int speed, int stamina, int strength)
    {
        this.guyName = guyName;
        this.heath = heath;
        this.maxHeath = maxHealth;
        this.type = type;
        this.speed = speed;
        this.stamina = stamina;
        this.strength = strength;
    }

    public GameObject GetHitboxPrefab()
    {
        return hitboxPrefab;
    }

    public Transform GetAttackPosition()
    {
        return attackPosition;
    }
}