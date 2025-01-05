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
            playerOwner.TeamSpeedBoost += speedBoostAmount;
            foreach (LilGuyBase lilGuy in playerOwner.LilGuyTeam)
            {
                if (!lilGuy.isActiveAndEnabled) continue;
                lilGuy.ApplySpeedBoost(spawnInterval, maxAfterimages, fadeSpeed, emissionColour);

            }
        }
        else
        {
            speed += speedBoostAmount;
            ApplySpeedBoost(spawnInterval, maxAfterimages, fadeSpeed, emissionColour);
        }

        StartCoroutine(StopSpeedBoost(playerOwner != null));
        base.Special();
    }

    private IEnumerator StopSpeedBoost(bool playerOwned)
    {
        yield return new WaitForSeconds(speedBoostDuration);
        if (!playerOwned)
        {
            speed -= speedBoostAmount;
            RemoveSpeedBoost();
        }
        else
        {
            playerOwner.TeamSpeedBoost -= speedBoostAmount;
            foreach (LilGuyBase lilGuy in playerOwner.LilGuyTeam)
            {
                lilGuy.RemoveSpeedBoost();
            }
        }
    }
}
