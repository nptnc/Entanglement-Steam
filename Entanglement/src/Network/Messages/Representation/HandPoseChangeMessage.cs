using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StressLevelZero;

using Entanglement.Representation;
using Entanglement.Extensions;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class HandPoseChangeMessageHandler : NetworkMessageHandler<HandPoseChangeMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.HandPose;

        public override NetworkMessage CreateMessage(HandPoseChangeMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(byte) * 2 + sizeof(ushort)];

            int index = 0;
            message.messageData[index++] = DiscordIntegration.GetByteId(data.userId);

            message.messageData[index++] = (byte)data.hand;

            byte[] poseIndex = BitConverter.GetBytes(data.poseIndex);
            for (int i = 0; i < sizeof(ushort); i++)
                message.messageData[index++] = poseIndex[i];

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            long userId = DiscordIntegration.GetLongId(message.messageData[index++]);

            if (PlayerRepresentation.representations.ContainsKey(userId))
            {
                PlayerRepresentation rep = PlayerRepresentation.representations[userId];

                if (rep.repFord) {
                    Handedness hand = (Handedness)message.messageData[index];
                    index += sizeof(byte);

                    int poseIndex = BitConverter.ToUInt16(message.messageData, index);
                    index += sizeof(ushort);

                    rep.UpdatePose(hand, poseIndex);
                }
            }

            if (Server.instance != null)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, userId);
            }
        }
    }

    public class HandPoseChangeMessageData : NetworkMessageData
    {
        public long userId;
        public Handedness hand;
        public ushort poseIndex;
    }
}
