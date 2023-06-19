using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Objects;

using StressLevelZero.Pool;

using UnityEngine;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class GripEventMessageHandler : NetworkMessageHandler<GripEventMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.GripEvent;

        public override NetworkMessage CreateMessage(GripEventMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) + sizeof(byte) * 2];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectId), ref index);

            message.messageData[index++] = data.index;
            message.messageData[index++] = (byte)data.type;

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            ushort objectId = BitConverter.ToUInt16(message.messageData, index);
            index += sizeof(ushort);

            if (ObjectSync.TryGetSyncable(objectId, out Syncable syncable))
            {
                if (syncable is TransformSyncable)
                {
                    TransformSyncable syncObj = syncable.Cast<TransformSyncable>();
                    byte gripIdx = message.messageData[index++];
                    var type = (TransformSyncable.GripEventType)message.messageData[index++];

                    syncObj.CallEvent(type, gripIdx);
                }
            }

            if (Server.instance != null)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class GripEventMessageData : NetworkMessageData
    {
        public ushort objectId;
        public byte index;
        public TransformSyncable.GripEventType type;
    }
}
