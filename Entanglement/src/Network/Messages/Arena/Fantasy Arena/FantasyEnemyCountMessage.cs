using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StressLevelZero.Arena;

using Entanglement.Patching;

using MelonLoader;

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

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            bool isLow = Convert.ToBoolean(message.messageData[0]);

            Arena_GameManager instance = Arena_GameManager.instance;
            if (instance)
                instance.arenaChallengeUI.SetEnemyCount(isLow);

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class FantasyEnemyCountMessageData : NetworkMessageData {
        public bool isLow = true;
    }
}
