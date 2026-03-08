using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [SerializeField] private SoundEntry[] _sounds;

        private Dictionary<SoundType, SoundEntry> _lookup;

        public bool TryGetClip(SoundType type, out AudioClip clip, out float volume)
        {
            EnsureLookup();
            if (_lookup.TryGetValue(type, out var entry))
            {
                clip = entry.Clip;
                volume = entry.Volume;
                return clip;
            }

            clip = null;
            volume = 1f;
            return false;
        }

        private void EnsureLookup()
        {
            if (_lookup != null)
                return;

            _lookup = new(_sounds.Length);
            for (var i = 0; i < _sounds.Length; i++)
                _lookup[_sounds[i].Type] = _sounds[i];
        }
    }

    [Serializable]
    public struct SoundEntry
    {
        public SoundType Type;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume;
    }
}
