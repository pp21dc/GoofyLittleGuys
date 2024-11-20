using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject target;

    // Update is called once per frame
    void Update()
    {
        transform.position = target.transform.position + offset;
    }
}
