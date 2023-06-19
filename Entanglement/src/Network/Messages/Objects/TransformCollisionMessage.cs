﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Objects;

using UnityEngine;
using Steamworks.Data;

#if DEBUG
using MelonLoader;
#endif

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class TransformCollisionMessageHandler : NetworkMessageHandler<TransformCollisionMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.TransformCollision;

        public override NetworkMessage CreateMessage(TransformCollisionMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) + sizeof(byte)];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectId), ref index);

            message.messageData[index++] = Convert.ToByte(data.enabled);

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

            int index = 0;
            ushort objectId = BitConverter.ToUInt16(message.messageData, index);
            index += sizeof(ushort);

            bool enabled = Convert.ToBoolean(message.messageData[index++]);

            if (ObjectSync.TryGetSyncable(objectId, out Syncable syncable)) {
                if (syncable is TransformSyncable) {
                    TransformSyncable syncObj = syncable.Cast<TransformSyncable>();

                    if (syncObj.rb)
                        syncObj.rb.detectCollisions = enabled;
                }
            }
        }
    }

    public class TransformCollisionMessageData : NetworkMessageData
    {
        public ushort objectId;
        public bool enabled;
    }
}
