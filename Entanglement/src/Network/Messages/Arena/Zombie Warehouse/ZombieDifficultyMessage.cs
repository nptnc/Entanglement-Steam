using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Patching;

namespace Entanglement.Network
{
    [Net.HandleOnLoaded]
    public class ZombieDifficultyMessageHandler : NetworkMessageHandler<ZombieDifficultyMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.ZombieDiff;

        public override NetworkMessage CreateMessage(ZombieDifficultyMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { (byte)data.difficulty };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            Zombie_GameControl instance = Zombie_GameControl.instance;
            if (instance) {
                Zombie_GameControl.Difficulty difficulty = (Zombie_GameControl.Difficulty)message.messageData[0];
                instance.SetDifficulty(difficulty);
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class ZombieDifficultyMessageData : NetworkMessageData {
        public Zombie_GameControl.Difficulty difficulty;
    }
}
