using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortOrderAdjustment : MonoBehaviour
{
    SpriteRenderer sprite;

    [SerializeField] private bool movingObject = true;
    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

	private void OnValidate()
	{
		if (sprite == null) sprite = GetComponent<SpriteRenderer>();
		sprite.sortingOrder = (int)-transform.position.z;
	}

	// Update is called once per frame
	void Update()
    {
        if (movingObject) sprite.sortingOrder = (int) -transform.position.z;
    }
}
