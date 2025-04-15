using UnityEngine;

namespace Singleton
{
    public class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object LockObj = new object();

        public static T Instance => _instance;

        protected virtual void Awake()
        {
            lock (LockObj)
            {
                if (_instance != null && _instance != this)
                {
                    Destroy(gameObject); // destroy duplicate
                    return;
                }

                _instance = (T)(object)this;
            }
        }
    }
}