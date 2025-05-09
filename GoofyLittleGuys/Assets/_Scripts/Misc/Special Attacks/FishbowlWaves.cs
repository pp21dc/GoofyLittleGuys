using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishbowlWaves : MonoBehaviour
{
	[SerializeField] float minDuration = 0.5f;
	[SerializeField] float maxDuration = 2f;
	[SerializeField] private Vector3 minScale = new Vector3(0.25f, 0.25f, 0.25f);
	[SerializeField] private Vector3 maxScale = new Vector3(1f, 1, 1f);

	Animator anim;

	float duration;
	public void Init(float duration, float chargeTime, float maxChargeTime, float minChargeTime, float minSize = 0.25f, float maxSize = 1f)
	{
		this.duration = duration;
		minScale = new Vector3 (minSize, minSize, minSize);
		maxScale = new Vector3 (maxSize, maxSize, maxSize);
		transform.localScale = Vector3.Lerp(minScale, maxScale, (chargeTime - minChargeTime) / (maxChargeTime - minChargeTime));
		duration = Mathf.Lerp(minDuration, maxDuration, (chargeTime - minChargeTime) / (maxChargeTime - minChargeTime));

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
