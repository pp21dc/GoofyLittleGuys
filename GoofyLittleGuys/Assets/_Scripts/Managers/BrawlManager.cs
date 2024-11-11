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
                BrawlTimeEnd();
            }

            if(players.Count > 1)
            {
                MonitorPlayerDefeats();
            }
            else
            {
                BrawlKnockoutEnd();
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
            players.Remove(defeatedPlayer);
        }

        /// <summary>
        /// When the game ends by timer, we want to determine each remaining player's total team health, and 
        /// add them to Rankings based on that.
        /// </summary>
        public void BrawlTimeEnd()
        {
            Debug.Log("Brawl Phase has ended, DING DING DING!!!");
        }

        /// <summary>
        /// If only one player is left standing, we can simply add them to the rankings list, since they will then be
        /// at the end, making them ranked the best.
        /// </summary>
        public void BrawlKnockoutEnd()
        {
            rankings.Add(players[0]);
            Debug.Log("Brawl Phase has ended by knockout!");
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

        private bool IsStillKicking(PlayerBody thePlayer)
        {
            List<LilGuyBase> playerTeam = thePlayer.LilGuyTeam;
            for (int i = 0;i < playerTeam.Count; i++)
            {
                if(playerTeam[i].health > 0)
                {
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// Checks if any players have run out of Lil Guys, then defeats them (calling PlayerDefeat) and
        /// adds them to the rankings 
        /// </summary>
        private void MonitorPlayerDefeats()
        {
            for(int i = 0; i < players.Count; i++)
            {
                PlayerBody thePlayer = players[i].GetComponent<PlayerBody>();
                if (!IsStillKicking(thePlayer))
                {
                    PlayerDefeat(players[i]);

                }
            }
        }

    }
}

