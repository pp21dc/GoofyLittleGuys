using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    private float shieldDuration = 1;                   // How long the shield will last for before it is destroyed
    private float shieldTimer = 0;                      // How long the shield has before it is destroyed.
    private DefenseType shieldOwner;                    // The owner of this shield
    [SerializeField] private float maxSize = 1;         // The maximum size this shield will reach
    [SerializeField] private float expansionSpeed = 1;  // How fast this shield will reach its max size in seconds.


	private void Start()
	{
		transform.localScale = Vector3.zero;
		StartCoroutine(ResizeShieldOvertime(Vector3.zero, Vector3.one * maxSize, false));
	}

    /// <summary>
    /// Method called when this shield is instantiated in the world.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="shieldOwner"></param>
	public void Initialize(float duration, DefenseType shieldOwner)
    {
        shieldDuration = duration;
        this.shieldOwner = shieldOwner;
        StartCoroutine("ShieldUp");
    }

    /// <summary>
    /// Coroutine that handles when the shield goes up, and keeps track of the time until it
    /// goes down again.
    /// </summary>
    /// <returns></returns>
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
		StartCoroutine(ResizeShieldOvertime(Vector3.one * maxSize, Vector3.zero, true));
	}
    
    /// <summary>
    /// Coroutine that rescales the shield from initialScale to targetScale over the expansion speed time.
    /// </summary>
    /// <param name="initialScale">The starting scale of the shield.</param>
    /// <param name="targetScale">The resulting scale of the shield.</param>
    /// <param name="destroyAfterResize">Should the shield be destroyed after it reaches its target scale.</param>
    /// <returns></returns>
    private IEnumerator ResizeShieldOvertime(Vector3 initialScale, Vector3 targetScale, bool destroyAfterResize)
    {
		float elapsedTime = 0;
		while (elapsedTime < expansionSpeed)
		{
			// Expanding the hitbox zone
			transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / expansionSpeed);
			elapsedTime += Time.deltaTime;

			yield return null;
		}
        if (destroyAfterResize) Destroy(gameObject);
	}

}
