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
    public class FantasyChallengeMessageHandler : NetworkMessageHandler<FantasyChallengeMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.FantasyChal;

        public override NetworkMessage CreateMessage(FantasyChallengeMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { Convert.ToByte(data.index) };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            Arena_GameManager instance = Arena_GameManager.instance;
            if (instance) {
                Arena_Challenge challenge = instance.masterChallengeList.ToArray()[message.messageData[0]];
                bool isLocked = challenge.profile.isLocked;
                challenge.profile.isLocked = false;
                instance.arenaChallengeUI.HoverButton(challenge);
                challenge.profile.isLocked = isLocked;
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class FantasyChallengeMessageData : NetworkMessageData {
        public byte index;
    }
}
