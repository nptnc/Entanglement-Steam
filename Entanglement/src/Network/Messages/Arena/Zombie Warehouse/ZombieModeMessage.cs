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
    public class ZombieModeMessageHandler : NetworkMessageHandler<ZombieModeMessageData> {
        public override byte? MessageIndex => BuiltInMessageType.ZombieMode;

        public override NetworkMessage CreateMessage(ZombieModeMessageData data) {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { data.mode };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, ulong sender, bool isServerHandled) {
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
                byte mode = message.messageData[0];
                ZombieMode_Settings.m_invalidSettings = true;
                instance.SetGameMode(mode);
            }
        }
    }

    public class ZombieModeMessageData : NetworkMessageData {
        public byte mode;
    }
}
