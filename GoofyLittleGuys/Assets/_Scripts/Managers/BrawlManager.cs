using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

namespace Managers
{
    public class BrawlManager : SingletonBase<BrawlManager>
    {
        [SerializeField] private const float phaseTwoDuration = 180f;
        [SerializeField] private float currentGameTime = 0;
        [SerializeField] private float swapCooldown = 3; // Length of cooldown between swaps
        [SerializeField] private float immuneTime = 2; // Length of invulnerablility after swap
        [SerializeField] private TextMeshProUGUI gameTimer;

        private TimeSpan gameTime;

        //public bool swappedRecently = false;
        public List<GameObject> players;
        public List<GameObject> rankings;

        // Start is called before the first frame update
        void Start()
        {
            Time.timeScale = 1;
            foreach (PlayerInput input in PlayerInput.all)
            {
                players.Add(input.gameObject);
            }
            if (swapCooldown < immuneTime)
            {
                swapCooldown += immuneTime;
            }
        }

        // Update is called once per frame
        void Update()
        {
            currentGameTime += Time.deltaTime;
            gameTime = TimeSpan.FromSeconds(currentGameTime);
            if (gameTimer != null) gameTimer.text = gameTime.ToString("mm':'ss");

            if (currentGameTime == phaseTwoDuration)
            {
                BrawlTimeOver();
            }
        }

        /// <summary>
        /// Takes a player that got defeated, and adds them to the end of the
        /// rankings list, meaning rankings is ordered from worst to best.
        /// </summary>
        /// <param name="defeatedPlayer"></param>
        public void PlayerDefeat(GameObject defeatedPlayer)
        {
            rankings.Add(defeatedPlayer);

        }

        /// <summary>
        /// At the end of this phase, determine the rankings of any remaining player(s), more Lil Guys left means
        /// a higher ranking. Then the remaining players will be added to the rankings list, and finally
        /// the scoreboard/podium will be displayed.
        /// </summary>
        public void BrawlTimeOver()
        {
            Debug.Log("Brawl Phase has ended, DING DING DING!!!");
        }

        public void StartSwapCooldown(PlayerBody waitingPlayer)
        {
            StartCoroutine(SwapCooldown(waitingPlayer));
        }
        private IEnumerator SwapCooldown(PlayerBody waitingPlayer)
        {
            waitingPlayer.HasSwappedRecently = true;
            yield return new WaitForSeconds(swapCooldown);
            waitingPlayer.HasSwappedRecently = false;
        }

        public void StartSwapImmunity(PlayerBody immunePlayer)
        {
            StartCoroutine(ImmunityFrames(immunePlayer));
        }
        private IEnumerator ImmunityFrames(PlayerBody immunePlayer)
        {
            immunePlayer.HasImmunity = true;
            yield return new WaitForSeconds(immuneTime);
            immunePlayer.HasImmunity = false;
        }

    }
}

