using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParentGameobject : MonoBehaviour
{
    private void OnAnimEnd()
    {
        Destroy(transform.parent.gameObject);
    }
}
