using com.ktgame.services.audio.enums;

namespace com.ktgame.services.audio
{
    internal class AudioLink
    {
        public readonly AudioPlayer Player;
        public AudioLinkBehaviour Behaviour;

        public AudioLink(AudioPlayer player, AudioLinkBehaviour behaviour)
        {
            Player = player;
            Behaviour = behaviour;
        }
    }
}