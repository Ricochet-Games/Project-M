using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// A single reusable AudioSource wrapper managed by AudioManager.
    /// Handles: configuring itself from an AudioCue, following a transform,
    /// fade in/out, volume ramps, and returning itself to the pool when done.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PooledAudioSource : MonoBehaviour
    {
        private AudioManager _manager;
        private AudioSource _source;
        private Transform _follow;
        private Coroutine _lifetimeRoutine;
        private Coroutine _fadeRoutine;

        private float _baseVolume;  
        private int _myIndex = -1;

        public bool IsBusy { get; private set; }
        public int CurrentPriority { get; private set; }

        /// <summary>Incremented every time this source starts a new sound. 0 = never used.</summary>
        public uint Generation { get; private set; }

        public void Init(AudioManager manager)
        {
            _manager = manager;
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _myIndex = transform.GetSiblingIndex();
            gameObject.SetActive(false);
        }

        public AudioHandle Play(AudioCue cue, Vector3 position, Transform follow,
                                AudioMixerGroup group, float spatialOverride)
        {
            Generation++;
            IsBusy = true;
            CurrentPriority = cue.priority;
            _follow = follow;

            gameObject.SetActive(true);
            transform.position = follow != null ? follow.position : position;

            _source.clip = cue.GetClip();
            _source.outputAudioMixerGroup = group;
            _source.loop = cue.loop;
            _source.pitch = cue.GetPitch();
            _source.spatialBlend = spatialOverride >= 0f ? spatialOverride : cue.spatialBlend;
            _source.minDistance = cue.minDistance;
            _source.maxDistance = cue.maxDistance;
            _source.rolloffMode = cue.rolloff;
            _source.dopplerLevel = cue.dopplerLevel;
            _source.priority = cue.priority;

            _baseVolume = cue.GetVolume();

            if (cue.fadeInSeconds > 0f)
            {
                _source.volume = 0f;
                StartFade(targetNormalized: 1f, seconds: cue.fadeInSeconds);
            }
            else
            {
                _source.volume = _baseVolume;
            }

            _source.Play();

            if (_lifetimeRoutine != null) StopCoroutine(_lifetimeRoutine);
            _lifetimeRoutine = StartCoroutine(LifetimeRoutine());

            return new AudioHandle(_myIndex, Generation);
        }

        public void Stop(float fadeOutSeconds)
        {
            if (!IsBusy) return;

            if (fadeOutSeconds <= 0f)
            {
                Release();
            }
            else
            {
                StartFade(targetNormalized: 0f, seconds: fadeOutSeconds, thenRelease: true);
            }
        }

        public void SetVolume(float normalized, float rampSeconds)
        {
            if (!IsBusy) return;
            if (rampSeconds <= 0f)
                _source.volume = _baseVolume * Mathf.Clamp01(normalized);
            else
                StartFade(Mathf.Clamp01(normalized), rampSeconds);
        }

        // ---------------------------------------------------------------

        private IEnumerator LifetimeRoutine()
        {
            if (_source.loop)
            {
                while (IsBusy) { FollowTick(); yield return null; }
            }
            else
            {
                while (IsBusy && _source.isPlaying) { FollowTick(); yield return null; }
                if (IsBusy) Release();
            }
        }

        private void FollowTick()
        {
            if (_follow != null)
                transform.position = _follow.position;
            else if (_follow == null && _source.spatialBlend > 0f && _followWasSet)
                _followWasSet = false;
        }
        private bool _followWasSet;

        private void StartFade(float targetNormalized, float seconds, bool thenRelease = false)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(targetNormalized, seconds, thenRelease));
        }

        private IEnumerator FadeRoutine(float targetNormalized, float seconds, bool thenRelease)
        {
            float start = _source.volume;
            float target = _baseVolume * targetNormalized;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                _source.volume = Mathf.Lerp(start, target, t / seconds);
                yield return null;
            }
            _source.volume = target;
            if (thenRelease) Release();
        }

        private void Release()
        {
            IsBusy = false;
            _follow = null;
            _followWasSet = false;
            if (_fadeRoutine != null) { StopCoroutine(_fadeRoutine); _fadeRoutine = null; }
            if (_lifetimeRoutine != null) { StopCoroutine(_lifetimeRoutine); _lifetimeRoutine = null; }
            _source.Stop();
            _source.clip = null;
            gameObject.SetActive(false);
            _manager.Return(this);
        }
    }
}