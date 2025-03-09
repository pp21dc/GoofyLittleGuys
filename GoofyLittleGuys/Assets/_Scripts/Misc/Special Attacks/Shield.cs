using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shield : MonoBehaviour
{
	private float shieldUptime = 5;                   // How long the shield will last for before it is destroyed
	private DefenseType shieldOwner;                    // The owner of this shield
	[SerializeField] private SpriteRenderer shieldSprite;  // How fast this shield will reach its max size in seconds.
	[SerializeField] private float maxSize = 1;         // The maximum size this shield will reach
	[SerializeField] private float expansionSpeed = 1;  // How fast this shield will reach its max size in seconds.
	private SortingGroup sortingGroup;

	private Color startColour;
	private Color endColour;
	private bool isFading = false;

	private void Start()
	{
		sortingGroup = GetComponent<SortingGroup>();
		transform.localScale = Vector3.zero;
		StartCoroutine(ResizeShieldOvertime(Vector3.zero, Vector3.one * maxSize, false));
	}
	private void Update()
	{
		sortingGroup.sortingOrder = (int)-shieldOwner.transform.position.z + 1;
		gameObject.transform.rotation = shieldOwner.Mesh.transform.rotation;
	}
	/// <summary>
	/// Method called when this shield is instantiated in the world.
	/// </summary>
	/// <param name="duration"></param>
	/// <param name="shieldOwner"></param>
	public void Initialize(float duration, DefenseType shieldOwner, Color start, Color end)
	{
		shieldUptime = duration;
		this.shieldOwner = shieldOwner;
		startColour = start;
		endColour = end;
		shieldSprite.color = startColour;
	}

	/// <summary>
	/// Begins the shield's fade-out process when called.
	/// </summary>
	public void BeginShieldFade()
	{
		if (isFading) return; // Prevent multiple fade calls.
		isFading = true;
		StartCoroutine(FadeAndDestroyShield());
	}

	/// <summary>
	/// Coroutine that fades the shield out over shieldUptime before destroying it.
	/// </summary>
	private IEnumerator FadeAndDestroyShield()
	{
		float elapsedTime = 0;
		while (elapsedTime < shieldUptime)
		{
			shieldSprite.color = Color.Lerp(startColour, endColour, elapsedTime / shieldUptime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

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

	private void OnDestroy()
	{
		shieldOwner.SpawnedShieldObj = null;
		shieldOwner.IsShieldActive = false;
	}
}
