using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Entanglement.Data;
using Entanglement.Representation;
using Entanglement.Extensions;

using StressLevelZero;

namespace Entanglement.Network
{
    public enum PlayerEventType {
        Death = 0,
    }

    [Net.SkipHandleOnLoading]
    public class PlayerEventMessageHandler : NetworkMessageHandler<PlayerEventMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.PlayerEvent;

        public override NetworkMessage CreateMessage(PlayerEventMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[1] { (byte)data.type };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            PlayerEventType type = (PlayerEventType)message.messageData[0];

            switch (type) {
                case PlayerEventType.Death:
                    if (PlayerRepresentation.representations.ContainsKey(sender)) {
                        PlayerRepresentation.representations[sender].CreateRagdoll();
                    }
                    break;
            }
        }
    }

    public class PlayerEventMessageData : NetworkMessageData
    {
        public PlayerEventType type;
    }
}
