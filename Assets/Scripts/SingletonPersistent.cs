using UnityEngine;

namespace Singleton
{
    public class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance => _instance;

        protected virtual void Awake()
        {
            // Ensure that this instance is the only one
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Destroy(gameObject); // Destroy duplicate
            }
        }

        public static void Initialize(T instance)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = instance;
                }

                else
                {
                    Debug.Log($"An instance of {typeof(T).Name} has already been initialized.");
                }
            }
        }
    }
}
