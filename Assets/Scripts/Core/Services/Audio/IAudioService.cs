namespace Services
{
    public enum SoundType : byte
    {
        Click,
        AutoCollect,
        TabSwitch
    }

    public interface IAudioService
    {
        void Play(SoundType sound);
    }
}
