using UnityEngine;
using Singleton;
using System.Collections.Generic;
using System.Collections;

namespace SingletonManagers
{
    public class AudioManager : SingletonPersistent
    {
        public static AudioManager Instance => GetInstance<AudioManager>();
        [System.Serializable]
        public class AudioClipInfo
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float defaultVolume = 1f;
            [Range(0.5f, 1.5f)]
            public float defaultPitch = 1f;
            public bool isLoop = false;
            [Range(0f, 1f)]
            public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
        }

        [Header("Audio Clips")]
        public List<AudioClipInfo> audioClips; // Assign in the Inspector
        
        [Header("Audio Source Settings")]
        [SerializeField] private int initialPoolSize = 5;
        [SerializeField] private Transform audioSourceParent;

        private readonly Dictionary<string, AudioClipInfo> _clipDictionary = new Dictionary<string, AudioClipInfo>();
        private readonly Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();

        private void Start()
        {
            // Create parent for audio sources if not assigned
            if (audioSourceParent == null)
            {
                audioSourceParent = new GameObject("Audio Sources").transform;
                audioSourceParent.SetParent(transform);
            }

            // Register all audio clips
            foreach (var entry in audioClips)
            {
                if (entry.clip != null)
                {
                    _clipDictionary[entry.name] = entry;
                }
            }

            // Pre-populate the pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateAudioSource();
            }
        }

        private AudioSource CreateAudioSource()
        {
            GameObject audioObj = new GameObject("Audio Source");
            audioObj.transform.SetParent(audioSourceParent);
            AudioSource audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.gameObject.SetActive(false);
            _audioSourcePool.Enqueue(audioSource);
            return audioSource;
        }

        public void PlaySound(string soundName)
        {
            PlaySound(soundName, Vector3.zero);
        }

        public void PlaySound(string soundName, Vector3 position, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
        {
            if (!_clipDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning($"Audio clip '{soundName}' not found!");
                return;
            }

            AudioClipInfo clipInfo = _clipDictionary[soundName];
            AudioSource audioSource = GetAudioSource();

            // Configure the audio source
            audioSource.clip = clipInfo.clip;
            audioSource.volume = clipInfo.defaultVolume * volumeMultiplier;
            audioSource.pitch = clipInfo.defaultPitch * pitchMultiplier;
            audioSource.loop = clipInfo.isLoop;
            audioSource.spatialBlend = clipInfo.spatialBlend;
            
            // Position the audio source
            audioSource.transform.position = position;
            
            // Enable and play
            audioSource.gameObject.SetActive(true);
            audioSource.Play();

            // Return to pool when done playing
            if (!clipInfo.isLoop)
            {
                StartCoroutine(ReturnToPool(audioSource, clipInfo.clip.length / audioSource.pitch));
            }
        }

        public void StopSound(string soundName)
        {
            if (!_clipDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning($"Audio clip '{soundName}' not found!");
                return;
            }

            // Find all active audio sources playing this sound
            AudioSource[] allAudioSources = audioSourceParent.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in allAudioSources)
            {
                if (source.isPlaying && source.clip == _clipDictionary[soundName].clip)
                {
                    source.Stop();
                    source.gameObject.SetActive(false);
                    _audioSourcePool.Enqueue(source);
                }
            }
        }

        private AudioSource GetAudioSource()
        {
            if (_audioSourcePool.Count == 0)
            {
                return CreateAudioSource();
            }
            return _audioSourcePool.Dequeue();
        }

        private IEnumerator ReturnToPool(AudioSource audioSource, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.gameObject.SetActive(false);
                _audioSourcePool.Enqueue(audioSource);
            }
        }
    }
}