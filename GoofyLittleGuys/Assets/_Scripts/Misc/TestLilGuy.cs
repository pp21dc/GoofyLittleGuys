using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLilGuy : MonoBehaviour
{
    [SerializeField] public float lifeTime;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LifeTime()); 
    }

    public IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.SpawnManager.Instance.DespawnForest(this.gameObject);
    }
}
