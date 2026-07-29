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
}
