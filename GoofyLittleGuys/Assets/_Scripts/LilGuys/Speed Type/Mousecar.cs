using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mousecar : SpeedType
{
    [Header("Mousecar Specific")]
    [SerializeField] private float speedBoostAmount = 10f;
    [SerializeField] private float speedBoostDuration = 7f;


    [Header("Special FX Specific")]
    [SerializeField] private float spawnInterval = 0.2f;
    [SerializeField] private int maxAfterimages = 12;
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private Color emissionColour = new Color(1.00f, 0.82f, 0.25f, 1.0f);   // The yellow used for speed lil guys

    public override void StartChargingSpecial()
    {
        base.StartChargingSpecial();
    }
    public override void StopChargingSpecial()
    {
        base.StopChargingSpecial();
    }
    protected override void Special()
    {
        if (playerOwner != null)
        {
            EventManager.Instance.ApplySpeedBoost(playerOwner, speedBoostAmount, spawnInterval, maxAfterimages, fadeSpeed, emissionColour, speedBoostDuration);
        }
        else
        {
            EventManager.Instance.ApplySpeedBoost(this, speedBoostAmount, spawnInterval, maxAfterimages, fadeSpeed, emissionColour, speedBoostDuration);
        }
        base.Special();
    }
}
