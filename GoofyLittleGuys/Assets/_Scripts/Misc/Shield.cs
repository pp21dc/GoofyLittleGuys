using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    private float shieldDuration = 1;
    private float shieldTimer = 0;
    private DefenseType shieldOwner;
    [SerializeField] private float maxSize = 1;
    [SerializeField] private float expansionSpeed = 1;

        
    public void Initialize(float duration, DefenseType shieldOwner)
    {
        shieldDuration = duration;
        this.shieldOwner = shieldOwner;
        StartCoroutine("ShieldUp");
    }

	private void Start()
	{
        transform.localScale = Vector3.zero;
        StartCoroutine(ResizeShieldOvertime(Vector3.zero, Vector3.one * maxSize));
	}
	private void OnDestroy()
	{
        StartCoroutine(ResizeShieldOvertime(Vector3.one * maxSize, Vector3.zero));
	}

	private IEnumerator ShieldUp()
    {
        shieldTimer = shieldDuration;
        while (shieldTimer > 0)
        {
            shieldTimer -= Time.deltaTime;
            yield return null;
        }
        transform.parent.GetComponent<DefenseType>().SpawnedShieldObj = null;
        shieldOwner.IsShieldActive = false;
        Destroy(gameObject);
    }
    
    private IEnumerator ResizeShieldOvertime(Vector3 initialScale, Vector3 targetScale)
    {
		float elapsedTime = 0;
		while (elapsedTime < expansionSpeed)
		{
			// Expanding the hitbox zone
			transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / expansionSpeed);
			elapsedTime += Time.deltaTime;

			yield return null;
		}
	}

}
