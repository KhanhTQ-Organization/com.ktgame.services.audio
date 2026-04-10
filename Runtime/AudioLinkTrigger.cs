using System.Collections.Generic;
using com.ktgame.services.audio.enums;
using UnityEngine;

namespace com.ktgame.services.audio
{
	public class AudioLinkTrigger : MonoBehaviour
	{
		private readonly List<AudioLink> _audioLinkList = new List<AudioLink>();

		private void OnEnable()
		{
			foreach (var link in _audioLinkList)
			{
				if (link.Player.IsAudioSourceDestroyed)
				{
					continue;
				}

				if (link.Player.State == AudioState.Stop)
				{
					continue;
				}

				switch (link.Behaviour)
				{
					case AudioLinkBehaviour.PlayOnEnable:
						link.Player.Play();
						break;
					case AudioLinkBehaviour.RestartOnEnable:
						link.Player.Restart();
						break;
					case AudioLinkBehaviour.PauseOnDisableUnPauseOnEnable:
						link.Player.UnPause();
						break;
					case AudioLinkBehaviour.PauseOnDisableRestartOnEnable:
						link.Player.Restart();
						break;
				}
			}
		}

		private void OnDisable()
		{
			foreach (var link in _audioLinkList)
			{
				if (link.Player.IsAudioSourceDestroyed)
				{
					continue;
				}

				if (link.Player.State == AudioState.Stop)
				{
					continue;
				}

				switch (link.Behaviour)
				{
					case AudioLinkBehaviour.StopOnDisable:
						link.Player.Stop();
						break;
					case AudioLinkBehaviour.PauseOnDisable:
					case AudioLinkBehaviour.PauseOnDisableUnPauseOnEnable:
					case AudioLinkBehaviour.PauseOnDisableRestartOnEnable:
						link.Player.Pause();
						break;
				}
			}
		}

		private void OnDestroy()
		{
			foreach (var link in _audioLinkList)
			{
				if (link.Player.IsAudioSourceDestroyed)
				{
					continue;
				}

				if (link.Player.State == AudioState.Stop)
				{
					continue;
				}

				link.Player.Stop();
			}
		}

		public void AddAudioPlayer(AudioPlayer player, AudioLinkBehaviour behaviour)
		{
			var link = _audioLinkList.Find(x => x.Player == player);
			if (link == null)
			{
				_audioLinkList.Add(new AudioLink(player, behaviour));
			}
			else
			{
				link.Behaviour = behaviour;
			}
		}

		public void RemoveAudioPlayer(AudioPlayer player)
		{
			var link = _audioLinkList.Find(x => x.Player == player);
			if (link != null)
			{
				_audioLinkList.Remove(link);
			}
		}
	}
}