using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObj : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider hitCollider)
    {

        if(hitCollider.attachedRigidbody)
        {
            PlayerBody hitPlayer = hitCollider.gameObject.GetComponent<PlayerBody>();
            if (hitPlayer != null)
            {
                
            }

        }
    }

}
