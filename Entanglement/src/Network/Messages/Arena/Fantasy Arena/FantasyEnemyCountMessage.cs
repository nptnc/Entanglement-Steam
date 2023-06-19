using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StressLevelZero.Arena;

using Entanglement.Patching;

using MelonLoader;
using Steamworks.Data;

namespace Entanglement.Network
{
    [Net.HandleOnLoaded]
    public class FantasyEnemyCountMessageHandler : NetworkMessageHandler<FantasyEnemyCountMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.FantasyCount;

        public override NetworkMessage CreateMessage(FantasyEnemyCountMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { Convert.ToByte(data.isLow) };

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

            bool isLow = Convert.ToBoolean(message.messageData[0]);

            Arena_GameManager instance = Arena_GameManager.instance;
            if (instance)
                instance.arenaChallengeUI.SetEnemyCount(isLow);
        }
    }

    public class FantasyEnemyCountMessageData : NetworkMessageData {
        public bool isLow = true;
    }
}
