using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameobjectEvent : MonoBehaviour
{
   public void DestroyGameObject()
	{
		Destroy(gameObject);
	}
}
