using System;
using System.Collections;
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

    [Header("References")]
    [HorizontalRule]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private List<TutorialIsland> tutorialIslands = new List<TutorialIsland>();
    [SerializeField] private List<TutorialText> tutorialTexts = new List<TutorialText>();
    [SerializeField] private List<GameObject> playerCheckboxes = new List<GameObject>();
    private List<Image> playerCheckmarks = new List<Image>();
	[ColoredGroup][SerializeField] private TMP_Text tutorialText;
	[ColoredGroup][SerializeField] private Image buttonImage;
    
    private List<TutorialStateMachine> tutorialStateMachines = new List<TutorialStateMachine>();
    private List<bool> islandsComplete = new List<bool>();
    public List<bool> IslandsComplete => islandsComplete;

    private int _currentTutorialState = -1;
    private Coroutine changeStateCoroutine;

    private void Start()
    {
        for (var i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            tutorialStateMachines.Add(new TutorialStateMachine(GameManager.Instance.Players[i], tutorialIslands[i], i));
            
            islandsComplete.Add(false);
            playerCheckboxes[i].gameObject.SetActive(true);
            playerCheckmarks.Add(playerCheckboxes[i].transform.GetChild(0).GetComponent<Image>());
            playerCheckmarks[i].color = GameManager.Instance.PlayerColours[i];
            playerCheckmarks[i].enabled = false;
            
            //this is evil dont look here
            tutorialIslands[i].exitPortal.transform.GetChild(2).GetComponent<TutorialPortal>().Tsm = tutorialStateMachines[i];

            GameManager.Instance.Players[i].InMenu = false;
            GameManager.Instance.Players[i].GetComponent<Rigidbody>().MovePosition(spawnPoints[i].position);
            EventManager.Instance.RefreshUi(GameManager.Instance.Players[i].PlayerUI, 0);
            GameManager.Instance.Players[i].PlayerColour = GameManager.Instance.PlayerColours[i];
            GameManager.Instance.Players[i].PlayerUI.SetColour();
		}
        Time.timeScale = 1.0f;
        EventManager.Instance.GameStartedEvent();
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
            changeStateCoroutine ??= StartCoroutine(nameof(DelayStateChange));

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

    private IEnumerator DelayStateChange()
    {
        yield return new WaitForSeconds(1.25f);
        ChangeAllStates();
        changeStateCoroutine = null;
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
                    tm.ChangeState(tm.TutorialGetBerryState);
                    break;
                case 7:
                    tm.ChangeState(tm.TutorialBerryState);
                    break;
                case 8:
                    tm.ChangeState(tm.TutorialFountainState);
                    break;
                case 9:
                    tm.ChangeState(tm.TutorialPortalState);
                    break;
                default:
                    DebugManager.Log("B.. B.. B... BOOOOOOM!");
                    ResetPlayers();
                    EventManager.Instance.CallLilGuyLockedInEvent();
                    break;
            }
        }
        SetTutorialText();
        ResetChecks();
    }

    public void EnableCheckmark(int num)
    {
        playerCheckmarks[num].enabled = true;
    }
    
    private void ResetChecks()
    {
        foreach (var check in playerCheckmarks)
        {
            check.enabled = false;
        }
    }

    private void ResetPlayers()
    {
        foreach (var player in GameManager.Instance.Players)
        {
            if (player.LilGuyTeam.Count > 1)
            {
                var demoLilG = player.LilGuyTeam.Find(lg => lg.MaxXp == 5000);
                player.LilGuyTeam.Remove(demoLilG);
                Destroy(demoLilG.gameObject);
                player.InStorm = false;
                player.StormHurtFx.SetActive(false);
                player.BerryCount = 0;
            }
            
            
            for (int i = 0; i < player.LilGuyTeam.Count; i++)
                Destroy(player.LilGuyTeam[i].gameObject);

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
            player.ActiveLilGuy.PlayerOwner = player;

            //enable all components cause we need to do that for some reason??
            player.ActiveLilGuy.enabled = true;
            player.ActiveLilGuy.Animator.enabled = true;
            player.ActiveLilGuy.GetComponent<AudioSource>().enabled = true;
            player.ActiveLilGuy.GetComponent<AiController>().enabled = true;
            player.ActiveLilGuy.GetComponent<TamedBehaviour>().enabled = true;
            player.ActiveLilGuy.GetComponent<Hurtbox>().enabled = true;

			EventManager.Instance.UpdatePlayerHealthUI(player);
			EventManager.Instance.RefreshUi(player.PlayerUI, 0);
		}
    }
    
}
