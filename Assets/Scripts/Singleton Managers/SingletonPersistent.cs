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
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this as T;
                }
                else if (_instance != this)
                {
                    Debug.LogWarning($"[SingletonPersistent<{typeof(T).Name}>] Duplicate found on '{gameObject.name}'. Destroying this instance.");
                    Destroy(gameObject);
                }
            }
        }

        /*protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }*/
    }
}