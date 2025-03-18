using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;

public class RespawnScreen : MonoBehaviour
{
    [SerializeField] private PlayerBody player;

    [SerializeField] private Image respawnCircle;
    [SerializeField] private TMP_Text respawnCounter;

    private float respawnTime;
    private float currentRespawnTime;

	private void OnEnable()
	{
        respawnTime = player.DeathTime + GameManager.Instance.RespawnTimer;
        currentRespawnTime = respawnTime - GameManager.Instance.CurrentGameTime;

        UpdateUI();
	}
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		currentRespawnTime = respawnTime - GameManager.Instance.CurrentGameTime;
        UpdateUI();

		if (currentRespawnTime <= 0)
		{
			gameObject.SetActive(false);
		}
	}

    private void UpdateUI()
    {
        respawnCircle.fillAmount = currentRespawnTime / GameManager.Instance.RespawnTimer;
        respawnCounter.text = (currentRespawnTime + 1).ToString("0");
    }
}
