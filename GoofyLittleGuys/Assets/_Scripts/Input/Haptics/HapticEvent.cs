using UnityEngine;

[CreateAssetMenu(fileName = "Haptic Event", menuName = "Haptics")]
public class HapticEvent : ScriptableObject
{
	[Tooltip("The name of the haptic event. Preferable to give it a name that corresponds to when it should occur\nEx.'Phase 2'")] public string eventName;
	[Tooltip("Low rumble frequency. Typically used for deeper rumbles.\nMost cases you just make low and high the same number unless you know what you're doing.")][Range(0, 1)] public float lowFrequency;
	[Tooltip("High rumble frequency. Typically used for finer vibrations.\nMost cases you just make low and high the same number unless you know what you're doing.")][Range(0, 1)] public float highFrequency;
	[Tooltip("The length in seconds that this haptic event should last for.")] [Min(0.1f)] public float duration;
}