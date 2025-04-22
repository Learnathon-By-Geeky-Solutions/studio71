using UnityEngine;
using Singleton;
using System.Collections.Generic;
using System.Collections;

namespace SingletonManagers
{
    public static class SoundKeys
    {
        public const string BackgroundMusic = "background_music";
        public const string ButtonPress = "button_press";
        public const string ButtonHover = "button_hover";
        public const string GunShot = "gunShot";
        public const string BloodHit = "bloodHit";
        public const string TerrainHit = "terrainHit";
        public const string ReloadStart = "reload_start";
        public const string GrenadeThrow = "grenadeThrow";
        public const string GrenadeExplosion = "Grenade_Explosion";
        // Add other sound keys here as needed
    }

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
        [SerializeField] private List<AudioClipInfo> audioClips; // Assign in the Inspector
        public List<AudioClipInfo> AudioClips => audioClips;
        
        [Header("Audio Source Settings")]
        [SerializeField] private int initialPoolSize = 5;
        [SerializeField] private Transform audioSourceParent;

        private readonly Dictionary<string, AudioClipInfo> _clipDictionary = new Dictionary<string, AudioClipInfo>();
        private readonly Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();
        private float _sfxVolumeMultiplier = 1.0f; // Global volume multiplier for SFX
        private float _backgroundMusicVolumeMultiplier = 1.0f; // Global volume multiplier for background music
        private AudioSource _backgroundMusicSource; // Reference to the current background music source

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

        public void SetSfxVolume(float volume)
        {
            _sfxVolumeMultiplier = Mathf.Clamp01(volume);
        }

        public void SetBackgroundMusicVolume(float volume)
        {
            _backgroundMusicVolumeMultiplier = Mathf.Clamp01(volume);

            // Update the volume of the currently playing background music source
            if (_backgroundMusicSource != null && _backgroundMusicSource.isPlaying)
            {
                // Find the original AudioClipInfo to get the default volume
                if (_clipDictionary.TryGetValue(SoundKeys.BackgroundMusic, out AudioClipInfo clipInfo))
                {
                    // Apply the multiplier to the clip's default volume
                    _backgroundMusicSource.volume = clipInfo.defaultVolume * _backgroundMusicVolumeMultiplier;
                }
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

        public static void PlaySound(string soundName)
        {
            PlaySound(soundName, Vector3.zero);
        }

        public static void PlaySound(string soundName, Vector3 position, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
        {
            if (Instance != null)
            {
                Instance.PlaySoundInternal(soundName, position, volumeMultiplier, pitchMultiplier);
            }
            else
            {
                Debug.LogWarning($"AudioManager instance not found. Cannot play sound: {soundName}");
            }
        }

        private void PlaySoundInternal(string soundName, Vector3 position, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
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
            float finalVolume = clipInfo.defaultVolume * volumeMultiplier;

            // Apply the correct global volume multiplier
            if (soundName == SoundKeys.BackgroundMusic)
            {
                finalVolume *= _backgroundMusicVolumeMultiplier;
            }
            else // Apply SFX volume to all other sounds
            {
                 finalVolume *= _sfxVolumeMultiplier;
            }

            audioSource.volume = finalVolume;
            audioSource.pitch = clipInfo.defaultPitch * pitchMultiplier;
            audioSource.loop = clipInfo.isLoop;
            audioSource.spatialBlend = clipInfo.spatialBlend;
            
            // Position the audio source
            audioSource.transform.position = position;
            
            // Enable and play
            audioSource.gameObject.SetActive(true);
            audioSource.Play();

            // If this is background music, store the reference
            if (soundName == SoundKeys.BackgroundMusic)
            {
                // Optional: Stop any previously playing background music if desired
                // if (_backgroundMusicSource != null && _backgroundMusicSource.isPlaying) 
                // { 
                //     StopSoundInternal(SoundKeys.BackgroundMusic); // Or just stop the specific source 
                // }
                _backgroundMusicSource = audioSource; 
            }

            // Return to pool when done playing (only for non-looping sounds)
            if (!clipInfo.isLoop)
            {
                StartCoroutine(ReturnToPool(audioSource, clipInfo.clip.length / audioSource.pitch));
            }
        }

        public static void StopSound(string soundName)
        {
            if (Instance != null)
            {
                Instance.StopSoundInternal(soundName);
            }
            else
            {
                Debug.LogWarning($"AudioManager instance not found. Cannot stop sound: {soundName}");
            }
        }

        private void StopSoundInternal(string soundName)
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
                    // If stopping the background music source, clear the reference
                    if (source == _backgroundMusicSource)
                    {
                        _backgroundMusicSource = null;
                    }
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
                // If returning the background music source, clear the reference
                if (audioSource == _backgroundMusicSource)
                {
                     _backgroundMusicSource = null;
                }
                audioSource.Stop();
                audioSource.gameObject.SetActive(false);
                _audioSourcePool.Enqueue(audioSource);
            }
        }
    }
}