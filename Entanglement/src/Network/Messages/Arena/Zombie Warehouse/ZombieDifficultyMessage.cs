using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Patching;
using Steamworks.Data;

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

        public override void HandleMessage(NetworkMessage message, ulong sender, bool isServerHandled)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            if (isServerHandled)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(SendType.Reliable, msgBytes, sender);
                return;
            }

            Zombie_GameControl instance = Zombie_GameControl.instance;
            if (instance) {
                Zombie_GameControl.Difficulty difficulty = (Zombie_GameControl.Difficulty)message.messageData[0];
                instance.SetDifficulty(difficulty);
            }
        }
    }

    public class ZombieDifficultyMessageData : NetworkMessageData {
        public Zombie_GameControl.Difficulty difficulty;
    }
}
