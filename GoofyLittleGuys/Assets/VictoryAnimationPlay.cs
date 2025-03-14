using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryAnimationPlay : MonoBehaviour
{
    [SerializeField] Animator textAnim;
    [SerializeField] Animator imageAnim;
    public void PlayAnimations()

    {
        imageAnim.SetTrigger("PlayAnim");
        textAnim.SetTrigger("PlayAnim");
            
    }
}
