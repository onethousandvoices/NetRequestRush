using UnityEngine;
using Zenject;

namespace Services
{
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioSource _source;

        [Inject] private AudioConfig _config;

        public void Play(SoundType sound)
        {
            if (_config.TryGetClip(sound, out var clip, out var volume))
                _source.PlayOneShot(clip, volume);
        }
    }
}
