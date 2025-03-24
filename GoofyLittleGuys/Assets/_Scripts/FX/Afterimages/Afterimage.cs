using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Afterimage : MonoBehaviour
{
	[Header("Afterimage Settings")]
	[HorizontalRule]
	[ColoredGroup][SerializeField] private Color emissionColour = new Color(1.00f, 0.82f, 0.25f, 1.0f);   // The yellow used for speed lil guys
	[ColoredGroup][SerializeField] private float initialIntensity = 2f;
	[ColoredGroup][SerializeField] private float fadeSpeed;

	private SpriteRenderer spriteRenderer;
	private ColorMutator emissionEditor;
	private Color initialColour = new Color(1, 1, 1, 0.5f);

	public void Initialize(Sprite sprite, Vector3 position, Quaternion rotation, float fadeSpeed)
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		// Disable shadow casting for the afterimage
		spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		// Enable emission on the material
		spriteRenderer.material.EnableKeyword("_EMISSION");

		// Set the emission color
		spriteRenderer.material.SetColor("_EmissionColor", emissionColour);

		initialColour = Color.white; // Initialize color
		spriteRenderer.color = initialColour;

		emissionEditor = new ColorMutator(emissionColour);
		emissionEditor.exposureValue = initialIntensity;
		spriteRenderer.material.SetColor("_EmissionColor", emissionEditor.exposureAdjustedColor);

		this.fadeSpeed = fadeSpeed;

		StartCoroutine(FadeOutAndDestroy());
	}


	private IEnumerator FadeOutAndDestroy()
	{
		float fadeTimer = 0f;

		while (fadeTimer < fadeSpeed)
		{
			fadeTimer += Time.deltaTime;
			float alpha = Mathf.Clamp01(1 - fadeTimer / fadeSpeed);

			initialColour.a = alpha;
			spriteRenderer.color = initialColour;

			emissionEditor.exposureValue = alpha * initialIntensity;
			spriteRenderer.material.SetColor("_EmissionColor", emissionEditor.exposureAdjustedColor);

			yield return null;
		}

		Destroy(gameObject);
	}
}
