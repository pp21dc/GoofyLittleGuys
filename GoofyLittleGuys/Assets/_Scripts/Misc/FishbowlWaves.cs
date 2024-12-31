using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishbowlWaves : MonoBehaviour
{
	Animator anim;
	float duration = 2f;

	public void Init(float duration)
	{
		this.duration = duration;
		anim = GetComponent<Animator>();
		StartCoroutine(EndWaves());
	}
	private IEnumerator EndWaves()
	{
		anim.ResetTrigger("WaveEnded");
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		anim.SetTrigger("WaveEnded");
		yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("OutWave") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
		Destroy(gameObject);
	}
}
