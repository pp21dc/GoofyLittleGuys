using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureBase : MonoBehaviour
{
    // -- Variables --
    [SerializeField] private GameObject barrier;
    private int maxTime = 10;
    private float time = 0;
    private bool complete = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (complete)
        {


        }
        else
        {
            time += Time.deltaTime;
            if (time >= maxTime)
            {
                // fail
            }
        }
    }
}
