using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.ktgame.core.di;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using com.ktgame.services.audio.enums;
using AudioType = com.ktgame.services.audio.enums.AudioType;

namespace com.ktgame.services.audio
{
public sealed class AudioPlayer
	{
		public string ID { get; private set; }

		public AudioType Type => _audioType;

		public AudioState State { get; private set; }

		public AudioSource Source => _audioSource;

		public bool IsPlaying => State == AudioState.Delay || State == AudioState.Playing;

		public float Time => _audioSource.time;

		public int TimeSamples => _audioSource.timeSamples;

		public AudioClip Clip => _audioSource.clip;

		public bool Mute => _audioSource.mute;

		public bool IsAudioSourceDestroyed => _audioSource == null;

		private Action _onStart;
		private Action _onPlay;
		private Action _onPause;
		private Action _onStop;
		private Action _onComplete;

		private float _delay;
		private bool _syncPitchWithTimeScale;
		private bool _autoStop = true;
		private readonly List<AudioLinkTrigger> _linkTriggers = new();

		private bool _isFadeVolumePlaying;
		private float _fadeVolumeStartTime;
		private float _fadeVolumeDuration;
		private float _fadeVolumeStartValue;
		private float _fadeVolumeEndValue;
		private Action _onFadeVolumeComplete;

		private bool _isFadePitchPlaying;
		private float _fadePitchStartTime;
		private float _fadePitchDuration;
		private float _fadePitchStartValue;
		private float _fadePitchEndValue;
		private Action _onFadePitchComplete;

		private float _timer;
		private float _pitch = 1f;
		private float _volume = 1f;
		private float _startFadeDuration;
		private bool _started;

		private readonly AudioType _audioType;
		private AudioSource _audioSource;

		[Inject] private IAudioManager _audioManager;

		public AudioPlayer(AudioSource audioSource, AudioType audioType, AudioClip clip)
		{
			_audioSource = audioSource;
			_audioType = audioType;
			audioSource.clip = clip;
			audioSource.playOnAwake = false;
			Initialize();
		}

		public void Update()
		{
			if (IsAudioSourceDestroyed)
			{
				return;
			}

			if (!IsPlaying)
			{
				return;
			}

			SetPitch(_pitch);
			SetVolume(_volume);
			_timer += UnityEngine.Time.unscaledDeltaTime;

			_onPlay?.Invoke();

			if (_timer < _delay)
			{
				State = AudioState.Delay;
				return;
			}

			State = AudioState.Playing;
			if (!_started)
			{
				Start();
			}
			else if (!_audioSource.isPlaying)
			{
				_onComplete?.Invoke();
				if (!_autoStop)
				{
					_timer = 0;
					_started = false;
					State = AudioState.Wait;
				}
				else
				{
					Stop();
				}
			}

			UpdateFadeVolume();
			UpdateFadePitch();
		}

		private void UpdateFadeVolume()
		{
			if (!_isFadeVolumePlaying)
			{
				return;
			}

			if (_audioSource == null)
			{
				return;
			}

			var fadeTime = UnityEngine.Time.realtimeSinceStartup - _fadeVolumeStartTime;
			SetVolume(Mathf.Lerp(_fadeVolumeStartValue, _fadeVolumeEndValue, Mathf.InverseLerp(0, _fadeVolumeDuration, fadeTime)));
			if (fadeTime >= _fadeVolumeDuration)
			{
				_isFadeVolumePlaying = false;
				_onFadeVolumeComplete?.Invoke();
			}
		}

		private void UpdateFadePitch()
		{
			if (!_isFadePitchPlaying)
			{
				return;
			}

			if (_audioSource == null)
			{
				return;
			}

			var fadeTime = UnityEngine.Time.realtimeSinceStartup - _fadePitchStartTime;
			SetPitch(Mathf.Lerp(_fadePitchStartValue, _fadePitchEndValue, Mathf.InverseLerp(0, _fadePitchDuration, fadeTime)));
			if (fadeTime >= _fadePitchDuration)
			{
				_isFadePitchPlaying = false;
				_onFadePitchComplete?.Invoke();
			}
		}

		private void Initialize()
		{
			_timer = 0;
			_started = false;
			_startFadeDuration = 0f;

			State = AudioState.Wait;
			_delay = 0;
			_syncPitchWithTimeScale = false;
			_autoStop = true;

			_pitch = 1f;
			_volume = 1f;

			_isFadePitchPlaying = false;
			_isFadeVolumePlaying = false;

			foreach (var trigger in _linkTriggers)
			{
				if (trigger != null)
				{
					trigger.RemoveAudioPlayer(this);
				}
			}

			_linkTriggers.Clear();

			_audioSource.Stop();
			_audioSource.outputAudioMixerGroup = null;
			_audioSource.loop = false;
			_audioSource.mute = false;
			_audioSource.pitch = 1f;
			_audioSource.volume = 1f;
			_audioSource.spatialBlend = 0f;
			_audioSource.panStereo = 0f;
			_audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Auto;
			_audioSource.priority = 128;
			_audioSource.dopplerLevel = 1f;
			_audioSource.spread = 0f;
			_audioSource.reverbZoneMix = 1f;
			_audioSource.minDistance = 1;
			_audioSource.maxDistance = 500;
			_audioSource.ignoreListenerPause = false;
			_audioSource.ignoreListenerVolume = false;
			_audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
			_audioSource.transform.position = Vector3.zero;

			_onStart = null;
			_onPlay = null;
			_onPause = null;
			_onStop = null;
			_onComplete = null;
		}

		private void Start()
		{
			if (_startFadeDuration > 0)
			{
				SetVolume(0);
				FadeVolume(1, _startFadeDuration);
			}

			_audioSource.Play();
			_onStart?.Invoke();
			_started = true;
		}

		public void Play()
		{
			ThrowExceptionIfStop();

			State = _timer < _delay ? AudioState.Delay : AudioState.Playing;
		}

		public void Play(float fadeInDuration)
		{
			ThrowExceptionIfStop();

			_startFadeDuration = fadeInDuration;
			Play();
		}

		public void Restart()
		{
			ThrowExceptionIfStop();

			_timer = 0;
			_started = false;

			if (_audioSource != null)
			{
				_audioSource.Stop();
			}

			Play();
		}

		public void Stop()
		{
			if (_audioSource != null)
			{
				_audioSource.Stop();
			}

			_onStop?.Invoke();
			_audioManager?.Release(this);
			_audioSource = null;
			State = AudioState.Stop;
		}

		public void Stop(float fadeOutDuration)
		{
			if (fadeOutDuration <= 0)
			{
				Stop();
			}
			else
			{
				FadeVolume(0, fadeOutDuration, Stop);
			}
		}

		public void Pause()
		{
			ThrowExceptionIfStop();

			if (_audioSource != null)
			{
				_audioSource.Pause();
			}

			_onPause?.Invoke();
			State = AudioState.Pause;
		}

		public void Pause(float fadeOutDuration)
		{
			ThrowExceptionIfStop();

			if (fadeOutDuration <= 0)
			{
				Pause();
			}
			else
			{
				FadeVolume(0, fadeOutDuration, Pause);
			}
		}

		public void UnPause()
		{
			ThrowExceptionIfStop();

			if (State != AudioState.Pause)
			{
				return;
			}

			_audioSource.UnPause();
			State = _timer < _delay ? AudioState.Delay : AudioState.Playing;
		}

		public void UnPause(float fadeInDuration)
		{
			ThrowExceptionIfStop();
			if (State != AudioState.Pause)
			{
				return;
			}

			_audioSource.UnPause();
			State = _timer < _delay ? AudioState.Delay : AudioState.Playing;
			var v = _audioSource.volume;
			SetVolume(0);
			FadeVolume(v, fadeInDuration);
		}

		public AudioPlayer OnStart(Action callback)
		{
			_onStart += callback;
			return this;
		}

		public AudioPlayer OnPlay(Action callback)
		{
			_onPlay += callback;
			return this;
		}

		public AudioPlayer OnPause(Action callback)
		{
			_onPause += callback;
			return this;
		}

		public AudioPlayer OnStop(Action callback)
		{
			_onStop += callback;
			return this;
		}

		public AudioPlayer OnComplete(Action callback)
		{
			_onComplete += callback;
			return this;
		}

		public AudioPlayer SetClip(AudioClip audioClip)
		{
			_audioSource.clip = audioClip;
			return this;
		}

		public AudioPlayer SetVolume(float volume)
		{
			_volume = volume;
			switch (_audioType)
			{
				default:
					_audioSource.volume = volume;
					break;
				case AudioType.Music:
					_audioSource.volume = volume * _audioManager?.MusicsVolume ?? volume;
					break;
				case AudioType.Sound:
					_audioSource.volume = volume * _audioManager?.SoundsVolume ?? volume;
					break;
			}

			return this;
		}

		public AudioPlayer SetPitch(float pitch)
		{
			_pitch = pitch;
			_audioSource.pitch = pitch * (_syncPitchWithTimeScale ? UnityEngine.Time.timeScale : 1);
			return this;
		}

		public AudioPlayer SetDelay(float delay)
		{
			_delay = delay;
			return this;
		}

		public AudioPlayer SetAudioMixerGroup(AudioMixerGroup audioMixerGroup)
		{
			_audioSource.outputAudioMixerGroup = audioMixerGroup;
			return this;
		}

		public AudioPlayer SetLoop(bool loop = true)
		{
			_audioSource.loop = loop;
			return this;
		}

		public AudioPlayer SetPosition(Vector3 point)
		{
			_audioSource.transform.position = point;
			return this;
		}

		public AudioPlayer SetSpatialBlend(float value)
		{
			_audioSource.spatialBlend = value;
			return this;
		}

		public AudioPlayer SetPanStereo(float value)
		{
			_audioSource.panStereo = value;
			return this;
		}

		public AudioPlayer SetPriority(int value)
		{
			_audioSource.priority = value;
			return this;
		}

		public AudioPlayer SetMute(bool value = true)
		{
			_audioSource.mute = value;
			return this;
		}

		public AudioPlayer SetReverbZoneMix(float value)
		{
			_audioSource.reverbZoneMix = value;
			return this;
		}

		public AudioPlayer SetVelocityUpdateMode(AudioVelocityUpdateMode mode)
		{
			_audioSource.velocityUpdateMode = mode;
			return this;
		}

		public AudioPlayer SetDopplerLevel(float value)
		{
			_audioSource.dopplerLevel = value;
			return this;
		}

		public AudioPlayer SetSpread(float value)
		{
			_audioSource.spread = value;
			return this;
		}

		public AudioPlayer SetRolloffMode(AudioRolloffMode mode)
		{
			_audioSource.rolloffMode = mode;
			return this;
		}

		public AudioPlayer SetMinDistance(float value)
		{
			_audioSource.minDistance = value;
			return this;
		}

		public AudioPlayer SetMaxDistance(float value)
		{
			_audioSource.maxDistance = value;
			return this;
		}

		public AudioPlayer SetIgnoreListenerPause(bool value = true)
		{
			_audioSource.ignoreListenerPause = value;
			return this;
		}

		public AudioPlayer SetIgnoreListenerVolume(bool value = true)
		{
			_audioSource.ignoreListenerVolume = value;
			return this;
		}

		public AudioPlayer SetSyncPitchWithTimeScale(bool value = true)
		{
			_syncPitchWithTimeScale = value;
			return this;
		}

		public AudioPlayer SetAutoStop(bool value = true)
		{
			_autoStop = value;
			return this;
		}

		public AudioPlayer SetLink(GameObject target, AudioLinkBehaviour behaviour = AudioLinkBehaviour.StopOnDestroy)
		{
			if (!target.TryGetComponent<AudioLinkTrigger>(out var trigger))
			{
				trigger = target.AddComponent<AudioLinkTrigger>();
			}

			if (!_linkTriggers.Contains(trigger))
			{
				_linkTriggers.Add(trigger);
			}

			trigger.AddAudioPlayer(this, behaviour);
			return this;
		}

		public AudioPlayer SetLink(Component component, AudioLinkBehaviour behaviour = AudioLinkBehaviour.StopOnDestroy)
		{
			return SetLink(component.gameObject, behaviour);
		}

		public AudioPlayer SetID(string id)
		{
			ID = id;
			return this;
		}

		public void FadeVolume(float endValue, float duration, Action callback = null)
		{
			if (duration > 0)
			{
				_fadeVolumeStartTime = UnityEngine.Time.realtimeSinceStartup;
				_fadeVolumeDuration = duration;
				switch (_audioType)
				{
					default:
						_fadeVolumeStartValue = _audioSource.volume;
						break;
					case AudioType.Music:
						// fadeVolumeStartValue = LucidAudio.BGMVolume == 0 ? 1 : (audioSource.volume / LucidAudio.BGMVolume);
						break;
					case AudioType.Sound:
						// fadeVolumeStartValue = LucidAudio.SEVolume == 0 ? 1 : (audioSource.volume / LucidAudio.SEVolume);
						break;
				}

				_fadeVolumeEndValue = endValue;
				_onFadeVolumeComplete = callback;
				_isFadeVolumePlaying = true;
			}
			else
			{
				_audioSource.volume = endValue;
				callback?.Invoke();
			}
		}

		public void FadePitch(float endValue, float duration, Action callback = null)
		{
			if (duration > 0)
			{
				_fadePitchStartTime = UnityEngine.Time.realtimeSinceStartup;
				_fadePitchDuration = duration;
				_fadePitchStartValue = _audioSource.pitch;
				_fadePitchEndValue = endValue;
				_onFadePitchComplete = callback;
				_isFadePitchPlaying = true;
			}
			else
			{
				_audioSource.pitch = endValue;
				callback?.Invoke();
			}
		}

		public IEnumerator WaitForCompletion()
		{
			return new WaitWhile(() => IsPlaying || State == AudioState.Pause);
		}

		public async Task WaitForCompletionAsync()
		{
			while (IsPlaying || State == AudioState.Pause)
			{
				await Task.Yield();
			}
		}

		public async UniTask ToUniTask(AudioCancelBehaviour audioCancelBehaviour = AudioCancelBehaviour.Stop, CancellationToken cancellationToken = default)
		{
			try
			{
				while (IsPlaying || State == AudioState.Pause)
				{
					await UniTask.Yield(cancellationToken: cancellationToken);
				}
			}
			catch (OperationCanceledException e)
			{
				switch (audioCancelBehaviour)
				{
					case AudioCancelBehaviour.Stop:
						Stop();
						break;
					case AudioCancelBehaviour.Pause:
						Pause();
						break;
				}

				throw;
			}
		}

		private void ThrowExceptionIfStop()
		{
			if (State == AudioState.Stop)
			{
				throw new InvalidOperationException("A stopped AudioPlayer is not allowed to play again. You need to create a new AudioPlayer.");
			}
		}
	}
}
