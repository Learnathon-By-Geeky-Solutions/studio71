using UnityEngine;

namespace Singleton
{
    public class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Only safe to use in editor or initialization phase
                    _instance = Object.FindAnyObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogWarning($"No instance of {typeof(T).Name} found in the scene.");
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this as T;
                }
                else if (_instance != this)
                {
                    Destroy(gameObject); // Ensure only one instance
                }
            }
        }
    }
}

