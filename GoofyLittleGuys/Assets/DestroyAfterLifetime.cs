using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterLifetime : MonoBehaviour
{
    [SerializeField] private float lifetime = 1;
    // Start is called before the first frame update
    void Awake()
    {
        Destroy(gameObject, lifetime);
    }

}
