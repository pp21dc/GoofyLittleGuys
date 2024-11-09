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
        [SerializeField] private float swapCooldown = 3;
        [SerializeField] private TextMeshProUGUI gameTimer;

        private TimeSpan gameTime;

        public bool swappedRecently = false;
        public List<GameObject> players;

        // Start is called before the first frame update
        void Start()
        {
            Time.timeScale = 1;
            foreach (PlayerInput input in PlayerInput.all)
            {
                players.Add(input.gameObject);
            }

        }

        // Update is called once per frame
        void Update()
        {
            currentGameTime += Time.deltaTime;
            gameTime = TimeSpan.FromSeconds(currentGameTime);
            if (gameTimer != null) gameTimer.text = gameTime.ToString("mm':'ss");
        }

        
    }
}

