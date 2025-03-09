using UnityEngine;

[CreateAssetMenu(fileName ="AudioObject",menuName ="AudioObject")]
public class AudioObject : ScriptableObject
{
    public AudioClip[] clips;
    public bool isSpatial = false;
    public bool playOnce = false;
    [Range(0f, 1f)]
    public float volume = 1f;
    public Vector2 pitch = new Vector2(1f, 1f); //rand pitch between x and y
    public string key = "";
}
