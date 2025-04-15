using UnityEngine;
using System;
using System.Collections.Generic;

namespace Singleton
{
    public abstract class SingletonPersistent : MonoBehaviour
    {
        private static readonly Dictionary<System.Type, SingletonPersistent> Instances = new();

        protected virtual void Awake()
        {
            var type = GetType();

            if (Instances.TryGetValue(type, out var existingInstance))
            {
                if (existingInstance != this)
                {
                    Debug.LogWarning($"Duplicate singleton of type {type.Name} found. Destroying new one.");
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                Instances[type] = this;
            }

            OnAwake();
        }

        protected virtual void OnAwake() { }

        public static T GetInstance<T>() where T : SingletonPersistent
        {
            Instances.TryGetValue(typeof(T), out var instance);
            return instance as T;
        }
    }
}