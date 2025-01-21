using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Rendering;

public class StormObj : MonoBehaviour
{
    [SerializeField] private float dmgPerInterval = 10f;
    [SerializeField] private float interval = 1f;
    [SerializeField] private float passiveTime = 15f;
    [SerializeField] private BoxCollider damageZone;
    [SerializeField] private Volume postVol;

    private List<PlayerBody> playersInStorm = new List<PlayerBody>();
    private bool isRunning = true;
    private Dictionary<PlayerBody, CancellationTokenSource> playerTokens = new Dictionary<PlayerBody, CancellationTokenSource>();

    private  void Start()
    {
        if (damageZone == null) damageZone = GetComponent<BoxCollider>();
        if (postVol == null) postVol = GetComponent<Volume>();
        damageZone.enabled = false;
        postVol.enabled = false;
    }

    private async void OnEnable()
    {
        await Task.Delay((int)(passiveTime * 1000)); //task.delay uses ms, so *1000 for seconds
        damageZone.enabled = true;
        postVol.enabled = true;

        _ = DamageCycleAsync();
    }

    private async Task DamageCycleAsync()
    {
        while (isRunning)
        {
            if (playersInStorm.Count > 0)
            {
                foreach (var player in playersInStorm.ToArray())
                {
                    if (player != null)
                    {
                        player.StormDamage(dmgPerInterval);
                    }
                }
            }
            await Task.Delay((int)(interval * 1000));
        }
    }

    private async void OnTriggerEnter(Collider collision)
    {
        if (!damageZone.enabled) return;

        var playerHurtbox = collision.gameObject.GetComponent<Hurtbox>();
        if (playerHurtbox != null && playerHurtbox.gameObject.TryGetComponent<TamedBehaviour>(out _))
        {
            var playerHit = playerHurtbox.gameObject.GetComponent<LilGuyBase>()?.PlayerOwner;
            if (playerHit == null) return;

            var tokenSource = new CancellationTokenSource();
            playerTokens[playerHit] = tokenSource;

            try
            {
                await HandlePlayerEntryAsync(playerHit, tokenSource.Token);
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log($"Storm cancelled for {playerHit.name}");
                if (playerHit != null)
                {
                    playerHit.InStorm = false;
                }
                playerTokens.Remove(playerHit);
                playersInStorm.Remove(playerHit);
            }
        }
    }

    private async Task HandlePlayerEntryAsync(PlayerBody player, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        try
        {
            if (!playersInStorm.Contains(player))
            {
                playersInStorm.Add(player);
            }
        }
        catch (System.OperationCanceledException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            CleanUpPlayerState(player);
        }
    }

    private void CleanUpPlayerState(PlayerBody player)
    {
        if (player != null) return;

        player.InStorm = false;
        playersInStorm.Remove(player);
        if (playerTokens.TryGetValue(player, out var tokenSource))
        {
            tokenSource.Dispose();
            playerTokens.Remove(player);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        var playerHurtbox = collision.gameObject.GetComponent<Hurtbox>();
        if (playerHurtbox != null && playerHurtbox.gameObject.TryGetComponent<TamedBehaviour>(out _))
        {
            var playerHit = playerHurtbox.gameObject.GetComponent<LilGuyBase>()?.PlayerOwner;
            if (playerHit == null) return;

            // Cancel any ongoing operations for this player
            if (playerTokens.TryGetValue(playerHit, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                playerTokens.Remove(playerHit);
            }

            if (playersInStorm.Contains(playerHit))
            {
                playerHit.InStorm = false;
                playersInStorm.Remove(playerHit);
            }
        }
    }

    private void OnDisable()
    {
        isRunning = false;
        // Clean up all token sources
        foreach (var tokenSource in playerTokens.Values)
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
        playerTokens.Clear();
    }
}