using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Patching;

namespace Entanglement.Network
{
    [Net.HandleOnLoaded]
    public class ZombieModeMessageHandler : NetworkMessageHandler<ZombieModeMessageData> {
        public override byte? MessageIndex => BuiltInMessageType.ZombieMode;

        public override NetworkMessage CreateMessage(ZombieModeMessageData data) {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { data.mode };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            Zombie_GameControl instance = Zombie_GameControl.instance;
            if (instance) {
                byte mode = message.messageData[0];
                ZombieMode_Settings.m_invalidSettings = true;
                instance.SetGameMode(mode);
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class ZombieModeMessageData : NetworkMessageData {
        public byte mode;
    }
}
