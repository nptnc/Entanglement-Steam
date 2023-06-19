using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Entanglement.Objects;
using Entanglement.Network;

namespace Entanglement.Extensions {
    public static class SyncUtilities {
        public static void UpdateBodyAttached(Rigidbody rb, string rootName, short spawnIndex, float spawnTime) {
            TransformSyncable syncObj = TransformSyncable.cache.GetOrAdd(rb.gameObject);
            if (syncObj)
                syncObj.SendEnqueue();

            else if (!rb.isKinematic) {
                long ownerId = DiscordIntegration.currentUser.Id;

                ushort? objectId = null;
                ushort callbackIndex = 0;

                if (Node.isServer) {
                    objectId = ObjectSync.lastId;
                    objectId++;
                }

                Syncable syncable = TransformSyncable.CreateSync(ownerId, rb, objectId);

                syncable.EnqueueOwner(ownerId);

                if (Server.instance == null)
                    callbackIndex = ObjectSync.QueueSyncable(syncable);

                TransformCreateMessageData createSync = new TransformCreateMessageData() {
                    ownerId = ownerId,
                    objectId = objectId != null ? objectId.Value : (ushort)0,
                    callbackIndex = callbackIndex,
                    objectPath = rb.transform.GetFullPath(rootName),
                    spawnIndex = spawnIndex,
                    spawnTime = spawnTime,
                };

                NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.TransformCreate, createSync);
                Node.activeNode.BroadcastMessage(NetworkChannel.Object, message.GetBytes());
            }
        }

        public static void UpdateBodyDetached(Rigidbody rb) {
            TransformSyncable syncObj = TransformSyncable.cache.GetOrAdd(rb.gameObject);
            if (syncObj)
                syncObj.SendDequeue();
        }
    }
}
