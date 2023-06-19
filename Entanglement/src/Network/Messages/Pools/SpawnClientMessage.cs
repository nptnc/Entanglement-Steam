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
    public class SpawnClientMessageHandler : NetworkMessageHandler<SpawnClientMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.SpawnClient;

        public override NetworkMessage CreateMessage(SpawnClientMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            byte[] utf8 = Encoding.UTF8.GetBytes(data.title);
            message.messageData = new byte[sizeof(byte) + sizeof(ushort) + utf8.Length + SimplifiedTransform.size];

            int index = 0;
            message.messageData[index++] = data.rbCount;

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.spawnId), ref index);

            byte[] transformBytes = data.transform.GetBytes();
            for (int i = 0; i < SimplifiedTransform.size; i++)
                message.messageData[index++] = transformBytes[i];

            for (int i = 0; i < utf8.Length; i++)
                message.messageData[index++] = utf8[i];

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            if (!Node.isServer)
            {
                int index = 0;
                byte rbCount = message.messageData[index++];

                ushort id = BitConverter.ToUInt16(message.messageData, index);
                index += sizeof(ushort);

                byte[] transformBytes = new byte[SimplifiedTransform.size];
                for (int i = 0; i < transformBytes.Length; i++)
                    transformBytes[i] = message.messageData[index++];

                int remaining = message.messageData.Length - index;
                byte[] stringBytes = new byte[remaining];
                for (int i = 0; i < remaining; i++)
                    stringBytes[i] = message.messageData[index++];

                string title = Encoding.UTF8.GetString(stringBytes);
                SimplifiedTransform transform = SimplifiedTransform.FromBytes(transformBytes);

#if DEBUG
                EntangleLogger.Log($"Received Object Spawn for Spawnable {title}!");
#endif

                MelonCoroutines.Start(RegisterAndSpawn(title, transform.position, transform.rotation.ExpandQuat(), id, rbCount));
            }
            else
                throw new ExpectedClientException();
        }

        internal static IEnumerator RegisterAndSpawn(string title, Vector3 position, Quaternion rotation, ushort id, byte rbCount)
        {
            SpawnableObject spawnable = SpawnableData.TryGetSpawnable(title);

            yield return null;

            yield return null;

            if (!spawnable) yield break;

            Vector3 scale = spawnable.prefab.transform.localScale;

            SpawnManager.SpawnOverride = true;

            Poolee poolee = PoolManager.DynamicPools[title].InstantiatePoolee(position, rotation);
            poolee.transform.position = position;
            poolee.transform.rotation = rotation;
            poolee.transform.localScale = scale;
            poolee.transform.parent = null;
            poolee.gameObject.SetActive(true);

            SpawnManager.SpawnOverride = false;

            // Create the sync transforms
            Rigidbody[] rbs = poolee.GetComponentsInChildren<Rigidbody>();
            if (rbs.Length == rbCount) {
                for (ushort i = 0; i < rbs.Length; i++) {
                    Rigidbody rb = rbs[i];
                    GameObject go = rb.gameObject;
                    ushort thisId = (ushort)(i + id);

                    TransformSyncable existingSync = TransformSyncable.cache.GetOrAdd(go);
                    if (existingSync)
                    {
                        ObjectSync.MoveSyncable(existingSync, thisId);
                        existingSync.ClearOwner();
                        existingSync.TrySetStale(DiscordIntegration.lobby.OwnerId);
                    }
                    else
                    {
                        TransformSyncable.CreateSync(DiscordIntegration.lobby.OwnerId, ComponentCacheExtensions.m_RigidbodyCache.GetOrAdd(go), thisId);
                    }

                    ObjectSync.lastId = thisId;
                }
            }

            GameObject spawnedObject = poolee.gameObject;
            var pooleeSyncable = spawnedObject.AddComponent<PooleeSyncable>();
            pooleeSyncable.id = id;
            pooleeSyncable.transforms = spawnedObject.GetComponentsInChildren<TransformSyncable>(true);
        }
    }

    public class SpawnClientMessageData : NetworkMessageData
    {
        // The id of the spawned object (starting at first rigidbody)
        public ushort spawnId;

        // The amount of rigidbodies in the object (So we know if the object should be ignored or not)
        public byte rbCount;

        // The title of the spawn pool
        public string title;

        // The position and rotation of the spawned object
        public SimplifiedTransform transform;
    }
}
