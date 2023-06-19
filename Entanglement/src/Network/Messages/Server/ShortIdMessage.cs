using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Entanglement.Extensions;

using MelonLoader;

namespace Entanglement.Network
{
    public class ShortIdMessageHandler : NetworkMessageHandler<ShortIdMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.ShortId;

        public override NetworkMessage CreateMessage(ShortIdMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(long) + sizeof(byte)];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.userId), ref index);

            message.messageData[index++] = data.byteId;

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            long userId = BitConverter.ToInt64(message.messageData, index);
            index += sizeof(long);

            byte byteId = message.messageData[index++];

            if (userId == DiscordIntegration.currentUser.Id)
                DiscordIntegration.localByteId = byteId;
            DiscordIntegration.RegisterUser(userId, byteId);
        }
    }

    public class ShortIdMessageData : NetworkMessageData
    {
        public long userId;
        public byte byteId;
    }
}
