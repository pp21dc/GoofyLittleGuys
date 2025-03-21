using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : SingletonBase<TutorialManager>
{
    [Serializable]
    public class TutorialText
    {
        public string text;
        public Sprite image;
    }

    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private List<TutorialIsland> tutorialIslands = new List<TutorialIsland>();
    [SerializeField] private List<TutorialText> tutorialTexts = new List<TutorialText>();
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private AudioSource musicSource;
    
    private List<TutorialStateMachine> tutorialStateMachines = new List<TutorialStateMachine>();
    private List<bool> islandsComplete = new List<bool>();
    public List<bool> IslandsComplete => islandsComplete;

    private int _currentTutorialState = -1;

    private void Start()
    {
        for (var i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            tutorialStateMachines.Add(new TutorialStateMachine(GameManager.Instance.Players[i], tutorialIslands[i]));
            
            islandsComplete.Add(false);

            GameManager.Instance.Players[i].InMenu = false;
            GameManager.Instance.Players[i].Starter = GameManager.Instance.Players[i].LilGuyTeam[0].gameObject;
            GameManager.Instance.Players[i].GetComponent<Rigidbody>().MovePosition(spawnPoints[i].position);
            EventManager.Instance.RefreshUi(GameManager.Instance.Players[i].PlayerUI, 0);
        }
        Time.timeScale = 1.0f;
        EventManager.Instance.GameStartedEvent();
        GameManager.Instance.StartGame = true;
        ChangeAllStates();
    }

    private void Update()
    {
        foreach (var tm in tutorialStateMachines)
        {
            tm.Update();
        }
    }

    private void FixedUpdate()
    {
        foreach (var tm in tutorialStateMachines)
        {
            tm.FixedUpdate();
        }
    }

    public void CheckComplete()
    {
        if (islandsComplete.All(o => o == true)) // if all elements in the list are true
        {
            ChangeAllStates();
            for (var i = 0; i < islandsComplete.Count; i++)
            {
                islandsComplete[i] = false;
            }
        }
    }

    private void SetTutorialText()
    {
        tutorialText.text = tutorialTexts[_currentTutorialState].text;
        buttonImage.sprite = tutorialTexts[_currentTutorialState].image ? tutorialTexts[_currentTutorialState].image : null; // image is from list otherwise null
    }

    private void ChangeAllStates()
    {
        _currentTutorialState++;
        foreach (var tm in tutorialStateMachines)
        {
            switch (_currentTutorialState)
            {
                case 0:
                    tm.ChangeState(tm.TutorialAttackState);
                    break;
                case 1:
                    tm.ChangeState(tm.TutorialSpecialState);
                    break;
                case 2:
                    tm.ChangeState(tm.TutorialDefeatState);
                    break;
                case 3:
                    tm.ChangeState(tm.TutorialTameState);
                    break;
                case 4:
                    tm.ChangeState(tm.TutorialSwapState);
                    break;
                case 5:
                    tm.ChangeState(tm.TutorialStormState);
                    break;
                case 6:
                    tm.ChangeState(tm.TutorialBerryState);
                    break;
                case 7:
                    tm.ChangeState(tm.TutorialFountainState);
                    break;
                case 8:
                    tm.ChangeState(tm.TutorialPortalState);
                    break;
                case 9:
                    DebugManager.Log("B.. B.. B... BOOOOOOM!");
                    GameManager.Instance.StartGame = false;
                    ResetPlayers();
                    EventManager.Instance.CallLilGuyLockedInEvent();
                    
                    break;
            }
        }
        SetTutorialText();
    }

    private void ResetPlayers()
    {
        foreach (var player in GameManager.Instance.Players)
        {
            if (player.LilGuyTeam.Count > 1)
            {
                var demoTeddy = player.LilGuyTeam.Find(lg => lg.MaxHealth == 30);
            player.LilGuyTeam.Remove(demoTeddy);
            Destroy(demoTeddy.gameObject);
            player.InStorm = false;
            player.StormHurtFx.SetActive(false);
            }
            
            
            foreach (var lg in player.LilGuyTeam)
                Destroy(lg.gameObject);

            player.LilGuyTeam.Clear();
            //add back starter
            GameObject starterGo = Instantiate(player.Starter);
            LilGuyBase starter = starterGo.GetComponent<LilGuyBase>();

            starter.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
            starter.SetFollowGoal(player.LilGuyTeamSlots[0].transform);
            starter.Init(LayerMask.NameToLayer("PlayerLilGuys"));
            starter.SetMaterial(GameManager.Instance.OutlinedLilGuySpriteMat);
            starterGo.transform.SetParent(player.transform, false);
            starterGo.GetComponent<Rigidbody>().isKinematic = true;
            starterGo.transform.localPosition = Vector3.zero;
            player.SetActiveLilGuy(starter);
            player.LilGuyTeam.Add(starter);
            player.LilGuyTeam[0].PlayerOwner = player;
            player.LilGuyTeamSlots[0].LilGuyInSlot = starter;

            //enable all components cause we need to do that for some reason??
            player.LilGuyTeam[0].enabled = true;
            player.LilGuyTeam[0].Animator.enabled = true;
            player.LilGuyTeam[0].GetComponent<AudioSource>().enabled = true;
            player.LilGuyTeam[0].GetComponent<AiController>().enabled = true;
            player.LilGuyTeam[0].GetComponent<TamedBehaviour>().enabled = true;
            player.LilGuyTeam[0].GetComponent<Hurtbox>().enabled = true;
        }
    }
    
}
