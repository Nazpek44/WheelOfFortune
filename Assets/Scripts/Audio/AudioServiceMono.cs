using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioServiceMono : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioSource _audioSource;

        [Header("Clips")]
        [SerializeField] private AudioClip _spinAudioClip;
        [SerializeField] private AudioClip _rewardAudioClip;
        [SerializeField] private AudioClip _bombAudioClip;
        [SerializeField] private AudioClip _collectAudioClip;

        private void Awake()
        {
            if (!_audioSource)
                _audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Editor only convenience. Reset runs when the component is added or
        /// reset by hand, which is a safe place to touch the hierarchy, unlike
        /// OnValidate.
        /// </summary>
        private void Reset()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlaySpin()
        {
            if (!_audioSource || !_spinAudioClip)
                return;

            _audioSource.Stop();
            _audioSource.clip = _spinAudioClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        public void StopSpin()
        {
            if (!_audioSource)
                return;

            _audioSource.Stop();
            _audioSource.loop = false;
            _audioSource.clip = null;
        }

        public void PlayReward()
        {
            PlayOneShot(_rewardAudioClip);
        }

        public void PlayBomb()
        {
            PlayOneShot(_bombAudioClip);
        }

        public void PlayCollect()
        {
            PlayOneShot(_collectAudioClip);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (!_audioSource || !clip)
                return;

            _audioSource.PlayOneShot(clip);
        }
    }
}
