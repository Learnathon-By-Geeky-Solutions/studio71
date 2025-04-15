using UnityEngine;

namespace Singleton
{
    public class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _initialized;

        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if (_initialized)
            {
                if (_instance != this)
                {
                    Debug.LogWarning($"[SingletonPersistent<{typeof(T).Name}>] Duplicate instance found, destroying: {gameObject.name}");
                    Destroy(gameObject);
                }
                return;
            }

            _instance = (T)(object)this;
            _initialized = true;
        }
    }
}