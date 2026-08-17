using System;
using System.Collections.Generic;

namespace AsoulChess.Game.Core.Audio
{
    public enum FrameworkAudioType
    {
        SoundEffect,
        BGM,
    }

    public interface IFrameworkAudioPlayer
    {
        FrameworkAudioType AudioType { get; }
        void ApplyMasterVolume(float soundEffectMaster, float bgmMaster);
        void PauseSelf();
        void ResumeSelf();
        void StopSelf();
    }

    public interface IAudioService
    {
        float SoundEffectVolume { get; }
        float BgmVolume { get; }

        void RegisterPlayer(IFrameworkAudioPlayer player);
        void UnregisterPlayer(IFrameworkAudioPlayer player);
        void ChangeSoundEffect(float value);
        void ChangeBgm(float value);
        void PauseAll();
        void ResumeAll();
        void StopAll();

        bool AcquireUniqueLimited(string clipName, IFrameworkAudioPlayer player, int maxHolders, out bool registered);
        IFrameworkAudioPlayer ReleaseUnique(string clipName, IFrameworkAudioPlayer player);
    }
}
