using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// Data-driven sound definition.
    /// code only ever asks the AudioManager to play a cue, it never touches
    /// clips, volumes, or mixer routing directly.
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Cue", fileName = "AC_NewCue")]
    public class AudioCue : ScriptableObject
    {
        [Header("Clips (one is chosen at random, avoiding immediate repeats)")]
        public AudioClip[] clips;

        [Header("Randomization")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.5f)] public float volumeVariation = 0.05f;
        public float pitch = 1f;
        [Range(0f, 0.5f)] public float pitchVariation = 0.05f;

        [Header("Playback")]
        public bool loop = false;
        public float fadeInSeconds = 0f;
        [Tooltip("0-255 like Unity's AudioSource priority, but ALSO used for pool stealing. Lower number = more important.")]
        [Range(0, 256)] public int priority = 128;

        [Header("Spatialization")]
        [Range(0f, 1f)] public float spatialBlend = 1f;
        public float minDistance = 1f;
        public float maxDistance = 50f;
        public AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;
        [Range(0f, 5f)] public float dopplerLevel = 0f;

        [Header("Routing")]
        public AudioMixerGroup outputGroup;

        // -----------------------------------------------------------------

        private int _lastClipIndex = -1;

        public bool HasClips => clips != null && clips.Length > 0;

        public AudioClip GetClip()
        {
            if (clips.Length == 1) return clips[0];

            int index = Random.Range(0, clips.Length);
            if (index == _lastClipIndex)
                index = (index + 1) % clips.Length;
            _lastClipIndex = index;
            return clips[index];
        }

        public float GetVolume() => Mathf.Clamp01(volume + Random.Range(-volumeVariation, volumeVariation));
        public float GetPitch() => pitch + Random.Range(-pitchVariation, pitchVariation);
    }
}