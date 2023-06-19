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
    public class TransformSyncMessageHandler : NetworkMessageHandler<TransformSyncMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.TransformSync;

        public override NetworkMessage CreateMessage(TransformSyncMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) * 2 + SimplifiedTransform.size];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectId), ref index);

            message.messageData = message.messageData.AddBytes(data.simplifiedTransform.GetBytes(), ref index);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, ulong sender, bool isServerHandled)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            if (isServerHandled)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(SendType.Unreliable, msgBytes, sender);
                return;
            }

            int index = 0;
            ushort objectId = BitConverter.ToUInt16(message.messageData, index);
            index += sizeof(ushort);

            if (ObjectSync.TryGetSyncable(objectId, out Syncable syncable)) {
                if (syncable is TransformSyncable) {
                    TransformSyncable syncObj = syncable.Cast<TransformSyncable>();

                    SimplifiedTransform simpleTransform = SimplifiedTransform.FromBytes(message.messageData.ToList().GetRange(index, SimplifiedTransform.size).ToArray());
                    syncObj.ApplyTransform(simpleTransform);

                    GameObject go = syncObj.gameObject;
                }
            }
        }
    }

    public class TransformSyncMessageData : NetworkMessageData {
        public ushort objectId;
        public SimplifiedTransform simplifiedTransform;
    }
}
