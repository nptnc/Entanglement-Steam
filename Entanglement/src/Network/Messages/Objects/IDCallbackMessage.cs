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
    public class IDCallbackMessageHandler : NetworkMessageHandler<IDCallbackMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.IDCallback;

        public override NetworkMessage CreateMessage(IDCallbackMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) * (data.destroySync ? 1 : 2) + sizeof(byte)];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectIndex), ref index);

            message.messageData[index++] = Convert.ToByte(data.destroySync);

            if (!data.destroySync)
                message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.newId), ref index);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            ushort objectIndex = BitConverter.ToUInt16(message.messageData, index);
            index += sizeof(ushort);

            bool destroySync = Convert.ToBoolean(message.messageData[index++]);

            if (!destroySync) {
                ushort newId = BitConverter.ToUInt16(message.messageData, index);
                index += sizeof(ushort);

                try
                {
                    Syncable queuedSync = ObjectSync.queuedSyncs[objectIndex];
                    queuedSync.RemoveFromQueue(newId);
                }
                catch { }
            }
            else {
                Syncable queuedSync = ObjectSync.queuedSyncs[objectIndex];
                queuedSync.Cleanup();
            }
        }
    }

    public class IDCallbackMessageData : NetworkMessageData
    {
        public ushort objectIndex;
        public bool destroySync = false;
        public ushort newId;
    }
}
