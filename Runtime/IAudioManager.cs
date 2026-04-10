using com.ktgame.core.manager;
using UnityEngine;

namespace com.ktgame.services.audio
{
	public interface IAudioManager : IManager
	{
		float MusicsVolume { get; set; }

		float SoundsVolume { get; set; }

		AudioPlayer Play(AudioType type, AudioClip clip, float fadeInDuration = 0f);

		AudioPlayer PlayMusic(AudioClip clip, float fadeInDuration = 0f);

		AudioPlayer PlaySound(AudioClip clip, float fadeInDuration = 0f);

		void RestartAllMusics();

		void RestartAllMusics(AudioClip clip);

		void RestartAllMusics(string id);

		void RestartAllSounds();

		void RestartAllSounds(AudioClip clip);

		void RestartAllSounds(string id);

		void RestartAll();

		void RestartAll(AudioClip clip);

		void RestartAll(string id);

		void StopAllMusics();

		void StopAllMusics(float fadeOutDuration);

		void StopAllMusics(AudioClip clip);

		void StopAllMusics(AudioClip clip, float fadeOutDuration);

		void StopAllMusics(string id);

		void StopAllMusics(string id, float fadeOutDuration);

		void StopAllSounds();

		void StopAllSounds(float fadeOutDuration);

		void StopAllSounds(AudioClip clip);

		void StopAllSounds(AudioClip clip, float fadeOutDuration);

		void StopAllSounds(string id);

		void StopAllSounds(string id, float fadeOutDuration);

		void StopAll();

		void StopAll(float fadeOutDuration);

		void StopAll(AudioClip clip);

		void StopAll(AudioClip clip, float fadeOutDuration);

		void StopAll(string id);

		void StopAll(string id, float fadeOutDuration);

		void PauseAllMusics();

		void PauseAllMusics(float fadeOutDuration);

		void PauseAllMusics(AudioClip clip);

		void PauseAllMusics(AudioClip clip, float fadeOutDuration);

		void PauseAllMusics(string id);

		void PauseAllMusics(string id, float fadeOutDuration);

		void PauseAllSounds();

		void PauseAllSounds(float fadeOutDuration);

		void PauseAllSounds(AudioClip clip);

		void PauseAllSounds(AudioClip clip, float fadeOutDuration);

		void PauseAllSounds(string id);

		void PauseAllSounds(string id, float fadeOutDuration);

		void PauseAll();

		void PauseAll(float fadeOutDuration);

		void PauseAll(AudioClip clip);

		void PauseAll(AudioClip clip, float fadeOutDuration);

		void PauseAll(string id);

		void PauseAll(string id, float fadeOutDuration);

		void UnPauseAllMusics();

		void UnPauseAllMusics(float fadeInDuration);

		void UnPauseAllMusics(AudioClip clip);

		void UnPauseAllMusics(AudioClip clip, float fadeInDuration);

		void UnPauseAllMusics(string id);

		void UnPauseAllMusics(string id, float fadeInDuration);

		void UnPauseAllSounds();

		void UnPauseAllSounds(float fadeInDuration);

		void UnPauseAllSounds(AudioClip clip);

		void UnPauseAllSounds(AudioClip clip, float fadeInDuration);

		void UnPauseAllSounds(string id);

		void UnPauseAllSounds(string id, float fadeInDuration);

		void UnPauseAll();

		void UnPauseAll(float fadeInDuration);

		void UnPauseAll(AudioClip clip);

		void UnPauseAll(AudioClip clip, float fadeInDuration);

		void UnPauseAll(string id);

		void UnPauseAll(string id, float fadeInDuration);

		void Release(AudioPlayer player);
	}
}
