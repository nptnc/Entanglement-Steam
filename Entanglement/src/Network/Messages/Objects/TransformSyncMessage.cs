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

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

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

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Unreliable, msgBytes, sender);
            }
        }
    }

    public class TransformSyncMessageData : NetworkMessageData {
        public ushort objectId;
        public SimplifiedTransform simplifiedTransform;
    }
}
