using UnityEngine;

namespace Managers
{
    public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance;

        [SerializeField] private bool dontDestroyOnLoad = true;

        public virtual void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this.gameObject);

            else if (Instance == null)
                Instance = this as T;
        }

        public void Ping()
        {
            Debug.Log("Pinged " + this.GetType().Name);
        }
    }
}
