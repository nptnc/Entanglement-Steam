using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Patching;

namespace Entanglement.Network
{
    [Net.HandleOnLoaded]
    public class ZombieLoadoutMessageHandler : NetworkMessageHandler<ZombieLoadoutMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.ZombieLoadout;

        public override NetworkMessage CreateMessage(ZombieLoadoutMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { data.loadIndex };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            Zombie_GameControl instance = Zombie_GameControl.instance;
            if (instance) {
                byte loadIndex = message.messageData[0];
                ZombieMode_Settings.m_invalidSettings = true;
                instance.ToggleLoadout(loadIndex);
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class ZombieLoadoutMessageData : NetworkMessageData {
        public byte loadIndex;
    }
}
