using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterimageEffect : MonoBehaviour
{
	[SerializeField] private float spawnInterval = 0.01f;
	[SerializeField] private int maxAfterimages = 12;
	[SerializeField] private float fadeSpeed = 0.5f;

	private SpriteRenderer characterSprite;
	private Coroutine afterimageCoroutine;
	private Queue<GameObject> afterimageQueue = new Queue<GameObject>();

	public SpriteRenderer CharacterSprite { set { characterSprite = value; } }
	public float SpawnInterval { set { spawnInterval = value; } }

	public void StartAfterimages()
	{
		if (afterimageCoroutine == null)
		{
			afterimageCoroutine = StartCoroutine(SpawnAfterimages());
		}
	}

	public void StopAfterimages()
	{
		if (afterimageCoroutine != null)
		{
			StopCoroutine(afterimageCoroutine);
			afterimageCoroutine = null;
		}

		// Optionally clear existing afterimages
		while (afterimageQueue.Count > 0)
		{
			afterimageQueue.Dequeue();
		}

		Destroy(this);
	}

	private IEnumerator SpawnAfterimages()
	{
		while (true)
		{
			if (afterimageQueue.Count >= maxAfterimages)
			{
				Destroy(afterimageQueue.Dequeue());
			}

			// Create afterimage
			GameObject afterimage = new GameObject("Afterimage");
			SpriteRenderer afterimageSprite = afterimage.AddComponent<SpriteRenderer>();
			afterimageSprite.sprite = characterSprite.sprite;
			afterimageSprite.material = new Material(characterSprite.material);
			afterimageSprite.sortingOrder = characterSprite.sortingOrder + 1;
			afterimageSprite.flipX = characterSprite.flipX;
			afterimage.transform.position = transform.position;
			afterimage.transform.rotation = transform.rotation;

			afterimage.AddComponent<Afterimage>().Initialize(
				characterSprite.sprite,
				transform.position,
				transform.rotation,
				fadeSpeed
			);

			afterimageQueue.Enqueue(afterimage);

			yield return new WaitForSeconds(spawnInterval);
		}
	}
}
