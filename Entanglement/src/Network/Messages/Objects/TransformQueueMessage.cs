using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Objects;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class TransformQueueMessageHandler : NetworkMessageHandler<TransformQueueMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.TransformQueue;

        public override NetworkMessage CreateMessage(TransformQueueMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) + sizeof(byte) * 2];

            int index = 0;
            message.messageData[index++] = DiscordIntegration.GetByteId(data.userId);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectId), ref index);

            message.messageData[index++] = Convert.ToByte(data.isAdd);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            long userId = DiscordIntegration.GetLongId(message.messageData[index++]);

            ushort objectId = BitConverter.ToUInt16(message.messageData, index);
            index += sizeof(ushort);

            if (ObjectSync.TryGetSyncable(objectId, out Syncable syncable)) {
                bool isAdd = Convert.ToBoolean(message.messageData[index++]);

                // Try to enqueue the user
                if (isAdd) {
                    syncable.EnqueueOwner(userId);
                }
                // Remove the user from the queue
                else {
                    syncable.DequeueOwner(userId);
                }
            }

            if (Server.instance != null)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessage(NetworkChannel.Object, msgBytes);
            }
        }
    }

    public class TransformQueueMessageData : NetworkMessageData
    {
        public long userId;
        public ushort objectId;
        public bool isAdd;
    }
}
