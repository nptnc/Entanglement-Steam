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
    public class ObjectDestroyMessageHandler : NetworkMessageHandler<ObjectDestroyMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.ObjectDestroy;

        public override NetworkMessage CreateMessage(ObjectDestroyMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort)];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectId), ref index);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            ushort objectId = BitConverter.ToUInt16(message.messageData, 0);

            if (ObjectSync.TryGetSyncable(objectId, out Syncable syncable)) {
                if (syncable is TransformSyncable) {
                    TransformSyncable syncObj = syncable.Cast<TransformSyncable>();
                    syncObj.Destruct();
                }
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class ObjectDestroyMessageData : NetworkMessageData {
        public ushort objectId;
    }
}
