using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientedBillboard : MonoBehaviour
{
    private float lifetime = 0.2f;
    
    public float Lifetime { set { lifetime = value; } }

	private void Start()
	{
        Destroy(gameObject, lifetime);
	}
	// Update is called once per frame
	void Update()
    {
    }
}
