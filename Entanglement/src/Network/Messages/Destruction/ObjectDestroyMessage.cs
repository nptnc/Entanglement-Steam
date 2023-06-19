using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Objects;
using Steamworks.Data;
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

            ushort objectId = BitConverter.ToUInt16(message.messageData, 0);

            if (ObjectSync.TryGetSyncable(objectId, out Syncable syncable)) {
                if (syncable is TransformSyncable) {
                    TransformSyncable syncObj = syncable.Cast<TransformSyncable>();
                    syncObj.Destruct();
                }
            }
        }
    }

    public class ObjectDestroyMessageData : NetworkMessageData {
        public ushort objectId;
    }
}
