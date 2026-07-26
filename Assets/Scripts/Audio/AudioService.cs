using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Audio
{
    public interface IAudioService
    {
        void PlaySpin();
        void StopSpin();
        void PlayReward();
        void PlayBomb();
        void PlayCollect();
    }

    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioSource audioSource;

        [Header("Clips")]
        [SerializeField] private AudioClip spinAudioClip;
        [SerializeField] private AudioClip rewardAudioClip;
        [SerializeField] private AudioClip bombAudioClip;
        [SerializeField] private AudioClip collectAudioClip;

        private void OnValidate()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        public void PlaySpin()
        {
            if (audioSource == null || spinAudioClip == null)
                return;

            audioSource.Stop();
            audioSource.clip = spinAudioClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        public void StopSpin()
        {
            if (audioSource == null)
                return;

            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }

        public void PlayReward()
        {
            PlayOneShot(rewardAudioClip);
        }

        public void PlayBomb()
        {
            PlayOneShot(bombAudioClip);
        }

        public void PlayCollect()
        {
            PlayOneShot(collectAudioClip);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource == null || clip == null)
                return;

            audioSource.PlayOneShot(clip);
        }
    }
}