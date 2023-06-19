using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Objects;

using UnityEngine;

#if DEBUG
using MelonLoader;
#endif

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class TransformCreateMessageHandler : NetworkMessageHandler<TransformCreateMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.TransformCreate;

        public override NetworkMessage CreateMessage(TransformCreateMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            byte[] utf8 = Encoding.UTF8.GetBytes(data.objectPath);
            
            message.messageData = new byte[sizeof(ushort) * 2 + sizeof(short) + sizeof(float) + sizeof(byte) * 2 + utf8.Length];

            int index = 0;
            message.messageData[index++] = DiscordIntegration.GetByteId(data.ownerId);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.objectId), ref index);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.callbackIndex), ref index);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.spawnIndex), ref index);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.spawnTime), ref index);

            message.messageData[index++] = Convert.ToByte(data.enqueueOwner);

            message.messageData = message.messageData.AddBytes(utf8, ref index);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            long ownerId = DiscordIntegration.GetLongId(message.messageData[index++]);

            ushort objectId = 0;
            ushort callbackIndex = 0;

            if (Server.instance != null)
            {
                objectId = ObjectSync.lastId;
                objectId += 1;
                ObjectSync.lastId = objectId;
                message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(objectId), index);

                index += sizeof(ushort);

                callbackIndex = BitConverter.ToUInt16(message.messageData, index);
                index += sizeof(ushort);
            }
            else {
                objectId = BitConverter.ToUInt16(message.messageData, index);
                ObjectSync.lastId = objectId;
                index += sizeof(ushort) * 2;
            }

            short spawnIndex = BitConverter.ToInt16(message.messageData, index);
            index += sizeof(short);

            float spawnTime = BitConverter.ToSingle(message.messageData, index);
            index += sizeof(float);

            bool enqueueOwner = Convert.ToBoolean(message.messageData[index++]);

            byte[] pathBytes = new byte[message.messageData.Length - index];
            for (int i = 0; i < pathBytes.Length; i++)
                pathBytes[i] = message.messageData[index++];

            string objectPath = Encoding.UTF8.GetString(pathBytes);

            Transform objectTransform = objectPath.GetFromFullPath(spawnIndex, spawnTime);
            bool destroySync = false;
            if (objectTransform) {
#if DEBUG
                EntangleLogger.Log($"Retrieved object from path {objectPath}!");
#endif

                TransformSyncable existingSync = TransformSyncable.cache.GetOrAdd(objectTransform.gameObject);
                if (existingSync) {
#if DEBUG
                    EntangleLogger.Log("Object is already synced! Don't freeze it!");
#endif
                    ObjectSync.MoveSyncable(existingSync, objectId);
                    existingSync.ClearOwner();
                    existingSync.TrySetStale(ownerId);

                    if (enqueueOwner) {
                        existingSync.EnqueueOwner(ownerId);
                    }
                }
                else {
#if DEBUG
                    EntangleLogger.Log($"Creating sync object!");
#endif

                    Syncable syncable = TransformSyncable.CreateSync(ownerId, ComponentCacheExtensions.m_RigidbodyCache.GetOrAdd(objectTransform.gameObject), objectId);

                    if (enqueueOwner) {
                        syncable.EnqueueOwner(ownerId);
                    }
                }
            }
            else {
#if DEBUG
                EntangleLogger.Warn($"Failed to retrieve object from path {objectPath}!");
#endif
            }

            ObjectSync.lastId = objectId;

            if (Server.instance != null) {
                // Send callback to owner
                IDCallbackMessageData idCallback = new IDCallbackMessageData()
                {
                    objectIndex = callbackIndex,
                    newId = objectId,
                    destroySync = destroySync,
                };

                NetworkMessage callbackMessage = NetworkMessage.CreateMessage((byte)BuiltInMessageType.IDCallback, idCallback);
                Server.instance.SendMessage(ownerId, NetworkChannel.Object, callbackMessage.GetBytes());

                // Send sync create to clients
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Object, msgBytes, ownerId);
            }
        }
    }

    public class TransformCreateMessageData : NetworkMessageData
    {
        public long ownerId;
        public ushort objectId;
        public ushort callbackIndex;
        public short spawnIndex = -1; // Used as an identifier to work-around different uuids
        public float spawnTime = -1f;
        public bool enqueueOwner = true;
        public string objectPath;
    }
}
