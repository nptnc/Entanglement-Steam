using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using Entanglement.Data;
using Entanglement.Exceptions;
using Entanglement.Extensions;
using Entanglement.Objects;

using StressLevelZero.Pool;
using StressLevelZero.Data;

using UnityEngine;

using MelonLoader;

namespace Entanglement.Network
{
    [Net.HandleOnLoaded]
    public class SpawnTransferMessageHandler : NetworkMessageHandler<SpawnTransferMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.SpawnTransfer;

        public override NetworkMessage CreateMessage(SpawnTransferMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) + SimplifiedTransform.size];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.spawnId), ref index);

            byte[] transformBytes = data.transform.GetBytes();
            for (int i = 0; i < SimplifiedTransform.size; i++)
                message.messageData[index++] = transformBytes[i];

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            if (!Node.isServer)
            {
                int index = 0;
                ushort id = BitConverter.ToUInt16(message.messageData, index);
                index += sizeof(ushort);

                byte[] transformBytes = new byte[SimplifiedTransform.size];
                for (int i = 0; i < transformBytes.Length; i++)
                    transformBytes[i] = message.messageData[index++];

                SimplifiedTransform transform = SimplifiedTransform.FromBytes(transformBytes);

                if (PooleeSyncable._PooleeLookup.TryGetValue(id, out PooleeSyncable pooleeSyncable))
                    pooleeSyncable.OnSpawn(DiscordIntegration.lobby.OwnerId, transform);
            }
            else
                throw new ExpectedClientException();
        }
    }

    public class SpawnTransferMessageData : NetworkMessageData
    {
        // The id of the spawned poolee
        public ushort spawnId;

        // The position and rotation of the spawned object
        public SimplifiedTransform transform;
    }
}
