using Entanglement.Network;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.src.Network.Messages.Redirect
{
    public class MessageRedirectMessageHandler : NetworkMessageHandler<MessageRedirectData>
    {
        public override byte? MessageIndex => BuiltInMessageType.MessageRelay;

        public override NetworkMessage CreateMessage(MessageRedirectData data)
        {
            NetworkMessage message = new NetworkMessage();

            ByteBuffer byteBuffer = new ByteBuffer(sizeof(byte) + data.data.Length + (sizeof(ulong) * data.toSendTo.Count));
            byte count = (byte) data.toSendTo.Count;
            byteBuffer.WriteByte(count);
            foreach (var id in data.toSendTo) {
                byteBuffer.WriteULong(id);
            }
            byteBuffer.WriteBytes(data.data);

            message.messageData = byteBuffer.GetBytes();

            return message;
        }

        public override void HandleMessage(NetworkMessage message, ulong sender, bool isServerHandled)
        {
            if (message.messageData.Length <= 0)
                throw new System.IndexOutOfRangeException();

            if (!isServerHandled)
            {
                return;
            }

            ByteBuffer byteBuffer = new ByteBuffer(message.messageData);
            byte count = byteBuffer.ReadByte();
            for (byte i = 0; i < count; i++) {
                NetworkSender.SendMessageToClient(byteBuffer.ReadULong(), SendType.Reliable, byteBuffer.GetRemainingBytes());
            }
        }
    }

    public class MessageRedirectData : NetworkMessageData {
        public List<ulong> toSendTo = new List<ulong>();
        public byte[] data;
    }
}
