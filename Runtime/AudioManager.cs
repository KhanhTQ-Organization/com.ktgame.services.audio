using System.Collections.Generic;
using System.Linq;
using com.ktgame.core.di;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using com.ktgame.services.audio.enums;
using AudioType = com.ktgame.services.audio.enums.AudioType;
using com.ktgame.core;

namespace com.ktgame.services.audio
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        public int Priority => 0;
        
        public bool IsInitialized { get; private set; }
        
        public IReadOnlyList<AudioPlayer> ActivePlayers => _activeAudioPlayers;

        public int ActiveSoundCount
        {
            get { return _activeAudioPlayers.Count(x => x.Type == AudioType.Sound); }
        }

        public int ActiveMusicCount
        {
            get { return _activeAudioPlayers.Count(x => x.Type == AudioType.Music); }
        }

        public float MusicsVolume { get; set; } = 1f;

        public float SoundsVolume { get; set; } = 1f;

        private readonly AudioMixerGroup _defaultMixerGroup = null;
        private readonly Queue<AudioSource> _seSourcePool = new();
        private readonly Queue<AudioSource> _bgmSourcePool = new();
        private readonly List<AudioPlayer> _activeAudioPlayers = new();
        private readonly List<AudioPlayer> _waitingAudioPlayers = new();

        [Inject] private IInjector _injector;

        public UniTask Initialize()
        {
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        private void Update()
        {
            _activeAudioPlayers.RemoveAll(x => x == null || x.State == AudioState.Stop);

            if (_waitingAudioPlayers.Count > 0)
            {
                _activeAudioPlayers.AddRange(_waitingAudioPlayers);
                _waitingAudioPlayers.Clear();
            }
            
            for (var i = 0; i < _activeAudioPlayers.Count; i++)
            {
                var player = _activeAudioPlayers[i];
                player?.Update();
            }

        }

        private void OnDestroy()
        {
            foreach (var audioPlayer in _activeAudioPlayers)
            {
                audioPlayer?.Stop();
            }
            _activeAudioPlayers.Clear();
            _waitingAudioPlayers.Clear();
        }

        public AudioPlayer Play(AudioType type, AudioClip clip, float fadeInDuration = 0f)
        {
            return PlayInternal(type, clip, fadeInDuration).SetAudioMixerGroup(_defaultMixerGroup);
        }

        public AudioPlayer Play(UnityEngine.AudioType type, AudioClip clip, float fadeInDuration = 0)
        {
            throw new System.NotImplementedException();
        }

        public AudioPlayer PlayMusic(AudioClip clip, float fadeInDuration = 0f)
        {
            return PlayInternal(AudioType.Music, clip, fadeInDuration);
        }

        public AudioPlayer PlaySound(AudioClip clip, float fadeInDuration = 0f)
        {
            return PlayInternal(AudioType.Sound, clip, fadeInDuration);
        }

        public void RestartAllMusics()
        {
            RestartInternal(AudioType.Music);
        }

        public void RestartAllMusics(AudioClip clip)
        {
            RestartInternal(AudioType.Music, clip);
        }

        public void RestartAllMusics(string id)
        {
            RestartInternal(AudioType.Music, null, id);
        }

        public void RestartAllSounds()
        {
            RestartInternal(AudioType.Sound);
        }

        public void RestartAllSounds(AudioClip clip)
        {
            RestartInternal(AudioType.Sound, clip);
        }

        public void RestartAllSounds(string id)
        {
            RestartInternal(AudioType.Sound, null, id);
        }

        public void RestartAll()
        {
            RestartAllMusics();
            RestartAllSounds();
        }

        public void RestartAll(AudioClip clip)
        {
            RestartAllMusics(clip);
            RestartAllSounds(clip);
        }

        public void RestartAll(string id)
        {
            RestartAllMusics(id);
            RestartAllSounds(id);
        }

        public void StopAllMusics()
        {
            StopInternal(AudioType.Music);
        }

        public void StopAllMusics(float fadeOutDuration)
        {
            StopInternal(AudioType.Music, null, null, fadeOutDuration);
        }

        public void StopAllMusics(AudioClip clip)
        {
            StopInternal(AudioType.Music, clip);
        }

        public void StopAllMusics(AudioClip clip, float fadeOutDuration)
        {
            StopInternal(AudioType.Music, clip);
        }

        public void StopAllMusics(string id)
        {
            StopInternal(AudioType.Music, null, id);
        }

        public void StopAllMusics(string id, float fadeOutDuration)
        {
            StopInternal(AudioType.Music, null, id);
        }

        public void StopAllSounds()
        {
            StopInternal(AudioType.Sound);
        }

        public void StopAllSounds(float fadeOutDuration)
        {
            StopInternal(AudioType.Sound, null, null, fadeOutDuration);
        }

        public void StopAllSounds(AudioClip clip)
        {
            StopInternal(AudioType.Sound, clip);
        }

        public void StopAllSounds(AudioClip clip, float fadeOutDuration)
        {
            StopInternal(AudioType.Sound, clip, null, fadeOutDuration);
        }

        public void StopAllSounds(string id)
        {
            StopInternal(AudioType.Sound, null, id);
        }

        public void StopAllSounds(string id, float fadeOutDuration)
        {
            StopInternal(AudioType.Sound, null, id, fadeOutDuration);
        }

        public void StopAll()
        {
            StopAllMusics();
            StopAllSounds();
        }

        public void StopAll(float fadeOutDuration)
        {
            StopAllMusics(fadeOutDuration);
            StopAllSounds(fadeOutDuration);
        }

        public void StopAll(AudioClip clip)
        {
            StopAllMusics(clip);
            StopAllSounds(clip);
        }

        public void StopAll(AudioClip clip, float fadeOutDuration)
        {
            StopAllMusics(clip, fadeOutDuration);
            StopAllSounds(clip, fadeOutDuration);
        }

        public void StopAll(string id)
        {
            StopAllMusics(id);
            StopAllSounds(id);
        }

        public void StopAll(string id, float fadeOutDuration)
        {
            StopAllMusics(id, fadeOutDuration);
            StopAllSounds(id, fadeOutDuration);
        }

        public void PauseAllMusics()
        {
            PauseInternal(AudioType.Music);
        }

        public void PauseAllMusics(float fadeOutDuration)
        {
            PauseInternal(AudioType.Music, null, null, fadeOutDuration);
        }

        public void PauseAllMusics(AudioClip clip)
        {
            PauseInternal(AudioType.Music, clip);
        }

        public void PauseAllMusics(AudioClip clip, float fadeOutDuration)
        {
            PauseInternal(AudioType.Music, clip, null, fadeOutDuration);
        }

        public void PauseAllMusics(string id)
        {
            PauseInternal(AudioType.Music, null, id);
        }

        public void PauseAllMusics(string id, float fadeOutDuration)
        {
            PauseInternal(AudioType.Music, null, id, fadeOutDuration);
        }

        public void PauseAllSounds()
        {
            PauseInternal(AudioType.Sound);
        }

        public void PauseAllSounds(float fadeOutDuration)
        {
            PauseInternal(AudioType.Sound, null, null, fadeOutDuration);
        }

        public void PauseAllSounds(AudioClip clip)
        {
            PauseInternal(AudioType.Sound, clip);
        }

        public void PauseAllSounds(AudioClip clip, float fadeOutDuration)
        {
            PauseInternal(AudioType.Sound, clip, null, fadeOutDuration);
        }

        public void PauseAllSounds(string id)
        {
            PauseInternal(AudioType.Sound, null, id);
        }

        public void PauseAllSounds(string id, float fadeOutDuration)
        {
            PauseInternal(AudioType.Sound, null, id, fadeOutDuration);
        }

        public void PauseAll()
        {
            PauseAllMusics();
            PauseAllSounds();
        }

        public void PauseAll(float fadeOutDuration)
        {
            PauseAllMusics(fadeOutDuration);
            PauseAllSounds(fadeOutDuration);
        }

        public void PauseAll(AudioClip clip)
        {
            PauseAllMusics(clip);
            PauseAllSounds(clip);
        }

        public void PauseAll(AudioClip clip, float fadeOutDuration)
        {
            PauseAllMusics(clip, fadeOutDuration);
            PauseAllSounds(clip, fadeOutDuration);
        }

        public void PauseAll(string id)
        {
            PauseAllMusics(id);
            PauseAllSounds(id);
        }

        public void PauseAll(string id, float fadeOutDuration)
        {
            PauseAllMusics(id, fadeOutDuration);
            PauseAllSounds(id, fadeOutDuration);
        }

        public void UnPauseAllMusics()
        {
            UnPauseInternal(AudioType.Music);
        }

        public void UnPauseAllMusics(float fadeInDuration)
        {
            UnPauseInternal(AudioType.Music, null, null, fadeInDuration);
        }

        public void UnPauseAllMusics(AudioClip clip)
        {
            UnPauseInternal(AudioType.Music, clip);
        }

        public void UnPauseAllMusics(AudioClip clip, float fadeInDuration)
        {
            UnPauseInternal(AudioType.Music, clip, null, fadeInDuration);
        }

        public void UnPauseAllMusics(string id)
        {
            UnPauseInternal(AudioType.Music, null, id);
        }

        public void UnPauseAllMusics(string id, float fadeInDuration)
        {
            UnPauseInternal(AudioType.Music, null, id, fadeInDuration);
        }

        public void UnPauseAllSounds()
        {
            UnPauseInternal(AudioType.Sound);
        }

        public void UnPauseAllSounds(float fadeInDuration)
        {
            UnPauseInternal(AudioType.Sound, null, null, fadeInDuration);
        }

        public void UnPauseAllSounds(AudioClip clip)
        {
            UnPauseInternal(AudioType.Sound, clip);
        }

        public void UnPauseAllSounds(AudioClip clip, float fadeInDuration)
        {
            UnPauseInternal(AudioType.Sound, clip, null, fadeInDuration);
        }

        public void UnPauseAllSounds(string id)
        {
            UnPauseInternal(AudioType.Sound, null, id);
        }

        public void UnPauseAllSounds(string id, float fadeInDuration)
        {
            UnPauseInternal(AudioType.Sound, null, id, fadeInDuration);
        }

        public void UnPauseAll()
        {
            UnPauseAllMusics();
            UnPauseAllSounds();
        }

        public void UnPauseAll(float fadeInDuration)
        {
            UnPauseAllMusics(fadeInDuration);
            UnPauseAllSounds(fadeInDuration);
        }

        public void UnPauseAll(AudioClip clip)
        {
            UnPauseAllMusics(clip);
            UnPauseAllSounds(clip);
        }

        public void UnPauseAll(AudioClip clip, float fadeInDuration)
        {
            UnPauseAllMusics(clip, fadeInDuration);
            UnPauseAllSounds(clip, fadeInDuration);
        }

        public void UnPauseAll(string id)
        {
            UnPauseAllMusics(id);
            UnPauseAllSounds(id);
        }

        public void UnPauseAll(string id, float fadeInDuration)
        {
            UnPauseAllMusics(id, fadeInDuration);
            UnPauseAllSounds(id, fadeInDuration);
        }

        public void Release(AudioPlayer player)
        {
            ReleaseAudioSource(player.Source, player.Type);
        }

        public void Dispose()
        {
            _activeAudioPlayers.RemoveAll(x => x.State == AudioState.Stop);
            _waitingAudioPlayers.Clear();
        }

        private AudioPlayer PlayInternal(AudioType audioType, AudioClip clip, float duration = 0)
        {
            var player = GetAudioPlayer(audioType, clip);
            player.SetVolume(audioType == AudioType.Music ? MusicsVolume : SoundsVolume);

            if (duration > 0)
            {
                player.Play(duration);
            }
            else
            {
                player.Play();
            }

            return player;
        }

        private void RestartInternal(AudioType audioType, AudioClip clip = null, string id = null)
        {
            foreach (var audioPlayer in FindAudioPlayers(audioType, clip, id))
            {
                audioPlayer.Restart();
            }
        }

        private void PauseInternal(AudioType audioType, AudioClip clip = null, string id = null, float duration = 0)
        {
            foreach (var audioPlayer in FindAudioPlayers(audioType, clip, id))
            {
                if (duration > 0)
                {
                    audioPlayer.Pause(duration);
                }
                else
                {
                    audioPlayer.Pause();
                }
            }
        }

        private void UnPauseInternal(AudioType audioType, AudioClip clip = null, string id = null, float duration = 0)
        {
            foreach (var audioPlayer in FindAudioPlayers(audioType, clip, id))
            {
                if (duration > 0)
                {
                    audioPlayer.UnPause(duration);
                }
                else
                {
                    audioPlayer.UnPause();
                }
            }
        }

        private void StopInternal(AudioType audioType, AudioClip clip = null, string id = null, float duration = 0)
        {
            var players = FindAudioPlayers(audioType, clip, id)?.ToList();
            if (players == null || players.Count == 0)
            {
                return;
            }

            foreach (var audioPlayer in players)
            {
                if (audioPlayer == null)
                {
                    continue;
                }

                if (duration > 0f)
                {
                    audioPlayer.Stop(duration);
                }
                else
                {
                    audioPlayer.Stop();
                }
            }
        }

        private void ReleaseAudioSource(AudioSource source, AudioType audioType)
        {
            if (source == null)
            {
                return;
            }

            var pool = GetAudioSourcePool(audioType);
            if (pool == null)
            {
                return;
            }
            
            if (!pool.Contains(source))
            {
                pool.Enqueue(source);
            }
        }

        private IEnumerable<AudioPlayer> FindAudioPlayers(AudioType audioType, AudioClip clip = null, string id = null)
        {
            var audioPlayers = _activeAudioPlayers
                .Where(p =>
                        p != null &&
                        p.Type == audioType &&
                        p.State != AudioState.Wait &&
                        p.State != AudioState.Stop
                );

            if (clip != null)
            {
                audioPlayers = audioPlayers.Where(p => p.Clip == clip);
            }

            if (!string.IsNullOrEmpty(id))
            {
                audioPlayers = audioPlayers.Where(p => p.ID == id);
            }

            return audioPlayers;
        }

        private AudioPlayer GetAudioPlayer(AudioType audioType, AudioClip clip)
        {
            var pool = GetAudioSourcePool(audioType);
            AudioSource source;
            AudioPlayer audioPlayer;

            if (pool != null && pool.TryDequeue(out source) && source != null)
            {
                audioPlayer = new AudioPlayer(source, audioType, clip);
            }
            else
            {
                var obj = new GameObject($"{audioType} Player");
                obj.transform.SetParent(transform, false);
                audioPlayer = new AudioPlayer(obj.AddComponent<AudioSource>(), audioType, clip);
                _injector?.Resolve(audioPlayer);
            }

            _waitingAudioPlayers.Add(audioPlayer);
            return audioPlayer;
        }

        private Queue<AudioSource> GetAudioSourcePool(AudioType audioType)
        {
            switch (audioType)
            {
                case AudioType.Music:
                    return _bgmSourcePool;
                case AudioType.Sound:
                    return _seSourcePool;
            }

            return null;
        }
    }
}