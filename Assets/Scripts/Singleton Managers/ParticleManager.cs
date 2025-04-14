using UnityEngine;
using Singleton;
using System.Collections.Generic;
using System.Collections;
namespace SingletonManagers
{
    public class ParticleManager : SingletonPersistent<ParticleManager>
    {
        [System.Serializable]
        public class ParticlePrefab
        {
            public string name;
            public ParticleSystem prefab;
        }
        [SerializeField] private List<ParticlePrefab> particlePrefabs;
        public List<ParticlePrefab> ParticlePrefabs => particlePrefabs; // Assign in the Inspector
        private readonly Dictionary<string, ParticleSystem> _prefabDictionary = new Dictionary<string, ParticleSystem>();
        private readonly Dictionary<string, Queue<ParticleSystem>> _particlePools = new Dictionary<string, Queue<ParticleSystem>>();

        private void Start()
        {
            foreach (var entry in particlePrefabs)
            {
                if (entry.prefab != null)
                {
                    _prefabDictionary[entry.name] = entry.prefab;
                    _particlePools[entry.name] = new Queue<ParticleSystem>();
                }
            }
        }
        public void PlayParticle(string particleName, Vector3 position, Quaternion rotation)
        {
            if (!_prefabDictionary.ContainsKey(particleName))
            {
                Debug.LogWarning($"Particle '{particleName}' not found!");
                return;
            }

            string key = particleName;
            ParticleSystem particle;

            if (_particlePools[key].Count > 0)
            {
                particle = _particlePools[key].Dequeue();
                particle.transform.position = position;
                particle.transform.rotation = rotation;
                particle.gameObject.SetActive(true);
            }
            else
            {
                particle = Instantiate(_prefabDictionary[key], position, rotation);
            }

            particle.Play();
            StartCoroutine(ReturnToPool(particle, key, particle.main.duration));
        }

        private IEnumerator ReturnToPool(ParticleSystem particle, string key, float delay)
        {
            yield return new WaitForSeconds(delay);
            particle.Stop();
            particle.gameObject.SetActive(false);
            _particlePools[key].Enqueue(particle);
        }
    }
}
    
