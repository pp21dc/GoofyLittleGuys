using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Managers;

public class RespawnScreen : MonoBehaviour
{
    [Header("References")]
    [HorizontalRule]
	[ColoredGroup][SerializeField] private PlayerBody player;
	[ColoredGroup][SerializeField] private Image respawnCircle;
	[ColoredGroup][SerializeField] private TMP_Text respawnCounter;

    private float respawnTime;
    private float currentRespawnTime;

	private void OnEnable()
	{
        respawnTime = player.DeathTime + GameManager.Instance.RespawnTimer;
        currentRespawnTime = respawnTime - Time.time;

        UpdateUI();
	}
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		currentRespawnTime = respawnTime - Time.time;
        UpdateUI();

		if (currentRespawnTime <= 0)
		{
			gameObject.SetActive(false);
		}
	}

    private void UpdateUI()
    {
        respawnCircle.fillAmount = currentRespawnTime / GameManager.Instance.RespawnTimer;
        respawnCounter.text = Mathf.CeilToInt(currentRespawnTime).ToString("0");
    }
}
