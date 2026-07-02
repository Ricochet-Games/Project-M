using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary> FOR KYLE
    /// Audio is always CLIENT-SIDE. Never network an
    /// AudioSource. Network the *game event* (ability fired, impact happened),
    /// then each client calls into this manager locally in response.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Pool")]
        [SerializeField] private int initialPoolSize = 16;
        [SerializeField] private int maxPoolSize = 64;        
        [SerializeField] private bool expandable = true;

        [Header("Routing")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup defaultGroup;

        private readonly List<PooledAudioSource> _all = new List<PooledAudioSource>();
        private readonly Queue<PooledAudioSource> _idle = new Queue<PooledAudioSource>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < initialPoolSize; i++)
                _idle.Enqueue(CreatePooledSource(i));
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>Play a cue in 2D (UI, music stingers, announcer, etc.).</summary>
        public AudioHandle Play2D(AudioCue cue)
            => PlayInternal(cue, Vector3.zero, null, spatialOverride: 0f);

        /// <summary>Play a cue at a world position (impacts, explosions).</summary>
        public AudioHandle PlayAtPoint(AudioCue cue, Vector3 position)
            => PlayInternal(cue, position, null);

        /// <summary>Play a cue that follows a transform (loops on characters, projectiles).</summary>
        public AudioHandle PlayAttached(AudioCue cue, Transform followTarget)
            => PlayInternal(cue, followTarget != null ? followTarget.position : Vector3.zero, followTarget);

        /// <summary>Stop a sound started earlier. Safe to call with stale/expired handles.</summary>
        public void Stop(AudioHandle handle, float fadeOutSeconds = 0f)
        {
            if (TryResolve(handle, out var src))
                src.Stop(fadeOutSeconds);
        }

        public void SetVolume(AudioHandle handle, float volume, float rampSeconds = 0f)
        {
            if (TryResolve(handle, out var src))
                src.SetVolume(volume, rampSeconds);
        }

        public bool IsPlaying(AudioHandle handle) => TryResolve(handle, out _);

        public void SetMixerParam(string exposedParam, float value)
        {
            if (mixer != null) mixer.SetFloat(exposedParam, value);
        }

        // ---------------------------------------------------------------
        // Internals
        // ---------------------------------------------------------------

        private AudioHandle PlayInternal(AudioCue cue, Vector3 position, Transform follow, float spatialOverride = -1f)
        {
            if (cue == null || !cue.HasClips)
            {
                Debug.LogWarning("[AudioManager] Play called with null/empty cue.");
                return AudioHandle.Invalid;
            }

            var src = Rent(cue.priority);
            if (src == null) return AudioHandle.Invalid; 

            var group = cue.outputGroup != null ? cue.outputGroup : defaultGroup;
            return src.Play(cue, position, follow, group, spatialOverride);
        }

        private PooledAudioSource Rent(int incomingPriority)
        {
            if (_idle.Count > 0)
                return _idle.Dequeue();

            if (expandable && _all.Count < maxPoolSize)
                return CreatePooledSource(_all.Count);

            PooledAudioSource steal = null;
            int lowest = incomingPriority;
            foreach (var s in _all)
            {
                if (s.IsBusy && s.CurrentPriority < lowest)
                {
                    lowest = s.CurrentPriority;
                    steal = s;
                }
            }
            if (steal != null) steal.Stop(0f);
            return steal;
        }

        private PooledAudioSource CreatePooledSource(int index)
        {
            var go = new GameObject($"PooledAudio_{index:00}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<PooledAudioSource>();
            src.Init(this);
            _all.Add(src);
            return src;
        }

        internal void Return(PooledAudioSource src)
        {
            _idle.Enqueue(src);
        }

        private bool TryResolve(AudioHandle handle, out PooledAudioSource src)
        {
            src = null;
            if (!handle.IsValid || handle.SourceIndex >= _all.Count) return false;
            var candidate = _all[handle.SourceIndex];
            if (candidate.Generation != handle.Generation || !candidate.IsBusy) return false;
            src = candidate;
            return true;
        }
    }

    public readonly struct AudioHandle
    {
        public readonly int SourceIndex;
        public readonly uint Generation;

        public AudioHandle(int sourceIndex, uint generation)
        {
            SourceIndex = sourceIndex;
            Generation = generation;
        }

        public bool IsValid => Generation != 0;
        public static AudioHandle Invalid => default;
    }
}