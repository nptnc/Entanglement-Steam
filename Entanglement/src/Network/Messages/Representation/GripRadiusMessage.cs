using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StressLevelZero;

using Entanglement.Representation;
using Entanglement.Extensions;
using Steamworks.Data;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class GripRadiusMessageHandler : NetworkMessageHandler<GripRadiusMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.GripRadius;

        public override NetworkMessage CreateMessage(GripRadiusMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(byte) * 3];

            int index = 0;
            message.messageData[index++] = DiscordIntegration.GetByteId(data.userId);

            message.messageData[index++] = (byte)data.hand;

            message.messageData[index++] = (byte)(data.radius * 255f);

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

            int index = 0;
            // User
            ulong userId = DiscordIntegration.GetLongId(message.messageData[index++]);


            if (PlayerRepresentation.representations.ContainsKey(userId))
            {
                PlayerRepresentation rep = PlayerRepresentation.representations[userId];

                if (rep.repFord)
                {
                    Handedness hand = (Handedness)message.messageData[index];
                    index += sizeof(byte);

                    float radius = ((float)message.messageData[index]) / 255f;

                    rep.UpdatePoseRadius(hand, radius);
                }
            }
        }
    }

    public class GripRadiusMessageData : NetworkMessageData
    {
        public ulong userId;
        public Handedness hand;
        public float radius;
    }
}
