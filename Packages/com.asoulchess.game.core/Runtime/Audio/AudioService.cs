using System.Collections.Generic;

namespace AsoulChess.Game.Core.Audio
{
    public sealed class AudioService : IAudioService
    {
        readonly List<IFrameworkAudioPlayer> _players = new List<IFrameworkAudioPlayer>();
        readonly Dictionary<string, int> _uniqueCount = new Dictionary<string, int>();
        readonly Dictionary<string, IFrameworkAudioPlayer> _uniqueCurrentPlayer = new Dictionary<string, IFrameworkAudioPlayer>();
        readonly Dictionary<string, List<IFrameworkAudioPlayer>> _uniqueHolders = new Dictionary<string, List<IFrameworkAudioPlayer>>();

        public float SoundEffectVolume { get; private set; } = 1f;
        public float BgmVolume { get; private set; } = 1f;

        public void RegisterPlayer(IFrameworkAudioPlayer player)
        {
            if (player != null && !_players.Contains(player))
                _players.Add(player);
        }

        public void UnregisterPlayer(IFrameworkAudioPlayer player)
        {
            if (player != null)
                _players.Remove(player);
        }

        public void ChangeSoundEffect(float value)
        {
            SoundEffectVolume = value;
            RefreshVolumes();
        }

        public void ChangeBgm(float value)
        {
            BgmVolume = value;
            RefreshVolumes();
        }

        void RefreshVolumes()
        {
            for (int i = 0; i < _players.Count; i++)
                _players[i]?.ApplyMasterVolume(SoundEffectVolume, BgmVolume);
        }

        public void PauseAll()
        {
            for (int i = 0; i < _players.Count; i++)
                _players[i]?.PauseSelf();
        }

        public void ResumeAll()
        {
            for (int i = 0; i < _players.Count; i++)
                _players[i]?.ResumeSelf();
        }

        public void StopAll()
        {
            for (int i = 0; i < _players.Count; i++)
                _players[i]?.StopSelf();
            _uniqueCount.Clear();
            _uniqueCurrentPlayer.Clear();
            _uniqueHolders.Clear();
        }

        public bool AcquireUniqueLimited(string clipName, IFrameworkAudioPlayer player, int maxHolders, out bool registered)
        {
            registered = false;
            if (player == null) return false;
            if (maxHolders < 1) maxHolders = int.MaxValue;

            if (!_uniqueHolders.TryGetValue(clipName, out var list))
            {
                list = new List<IFrameworkAudioPlayer>();
                _uniqueHolders[clipName] = list;
                _uniqueCount[clipName] = 0;
            }

            if (list.Contains(player)) return false;
            if (list.Count >= maxHolders) return false;

            list.Add(player);
            _uniqueCount[clipName]++;

            registered = true;
            if (_uniqueCount[clipName] == 1)
            {
                _uniqueCurrentPlayer[clipName] = player;
                return true;
            }

            return false;
        }

        public IFrameworkAudioPlayer ReleaseUnique(string clipName, IFrameworkAudioPlayer player)
        {
            if (!_uniqueHolders.TryGetValue(clipName, out var list)) return null;

            list.Remove(player);
            _uniqueCount[clipName]--;

            if (_uniqueCount[clipName] <= 0)
            {
                _uniqueHolders.Remove(clipName);
                _uniqueCount.Remove(clipName);
                _uniqueCurrentPlayer.Remove(clipName);
                return null;
            }

            if (_uniqueCurrentPlayer.TryGetValue(clipName, out var current) && current == player)
            {
                var next = list[0];
                _uniqueCurrentPlayer[clipName] = next;
                return next;
            }

            return null;
        }
    }
}
