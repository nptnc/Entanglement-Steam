using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Objects;

using StressLevelZero.Pool;
using StressLevelZero.Data;

using UnityEngine;

using MelonLoader;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class SpawnObjectMessage : NetworkMessageHandler<SpawnMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.SpawnObject;

        public override NetworkMessage CreateMessage(SpawnMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            byte[] utf8 = Encoding.UTF8.GetBytes(data.spawnableTitle);
            message.messageData = new byte[sizeof(ushort) * 2 + sizeof(byte) + SimplifiedTransform.size + utf8.Length];

            int index = 0;
            message.messageData[index++] = DiscordIntegration.GetByteId(data.userId);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectId), ref index);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.callbackIndex), ref index);

            byte[] transformBytes = data.spawnTransform.GetBytes();
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

            byte[] transformBytes = new byte[SimplifiedTransform.size];

            int index = 0;
            long userId = DiscordIntegration.GetLongId(message.messageData[index++]);

            ushort objectId = 0;

            if (Server.instance != null)
            {
                objectId = ObjectSync.lastId;
                objectId += 1;
                ObjectSync.lastId = objectId;
                message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(objectId), index);

                index += sizeof(ushort);

                ushort callbackIndex = BitConverter.ToUInt16(message.messageData, index);
                index += sizeof(ushort);

                // Send callback to owner
                IDCallbackMessageData idCallback = new IDCallbackMessageData()
                {
                    objectIndex = callbackIndex,
                    newId = objectId,
                    destroySync = false
                };

                NetworkMessage callbackMessage = NetworkMessage.CreateMessage((byte)BuiltInMessageType.IDCallback, idCallback);
                Server.instance.SendMessage(userId, NetworkChannel.Object, callbackMessage.GetBytes());

                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Object, msgBytes, userId);
            }
            else {
                objectId = BitConverter.ToUInt16(message.messageData, index);
                ObjectSync.lastId = objectId;
                index += sizeof(ushort) * 2;
            }

            for (int i = 0; i < transformBytes.Length; i++)
                transformBytes[i] = message.messageData[index++];

            int remaining = message.messageData.Length - index;
            byte[] stringBytes = new byte[remaining];
            for (int i = 0; i < remaining; i++)
                stringBytes[i] = message.messageData[index++];

            string title = Encoding.UTF8.GetString(stringBytes);
            SimplifiedTransform transform = SimplifiedTransform.FromBytes(transformBytes);

#if DEBUG
            EntangleLogger.Log($"Received object spawn for title {title}!");
#endif

            ObjectSync.lastId = objectId;

            MelonCoroutines.Start(RegisterAndSpawn(title, transform.position, transform.rotation.ExpandQuat(), objectId, userId));
        }

        public static IEnumerator RegisterAndSpawn(string title, Vector3 position, Quaternion rotation, ushort objectId, long userId) {
            SpawnableObject spawnable = SpawnableData.TryGetSpawnable(title);

            yield return null;

            yield return null;

            if (!spawnable) yield break;

            Vector3 scale = spawnable.prefab.transform.localScale;

            GameObject obj = null;

            try {
                obj = GlobalPool.Spawn(title, position, rotation, scale);
            } catch { }

            if (obj) {
#if DEBUG
                EntangleLogger.Log($"Successfully spawned obj {obj.name}!");
#endif

                TransformSyncable.CreateSync(userId, ComponentCacheExtensions.m_RigidbodyCache.GetOrAdd(obj), objectId);
            }
            else {
#if DEBUG
                EntangleLogger.Warn($"No object spawned for {title}!");
#endif
            }
        }
    }

    public class SpawnMessageData : NetworkMessageData
    {
        public long userId;
        public ushort objectId;
        public ushort callbackIndex;
        public string spawnableTitle;
        public SimplifiedTransform spawnTransform;
    }
}
