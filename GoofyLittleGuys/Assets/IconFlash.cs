using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconFlash : MonoBehaviour
{
	[SerializeField] private float flashAnimDuration = 2;
	private void OnEnable()
	{
		StartCoroutine(NotifyFlash());
	}

	private IEnumerator NotifyFlash()
	{
		yield return new WaitForSeconds(flashAnimDuration);
		GetComponent<Animator>().SetTrigger("Finish");
	}
}
