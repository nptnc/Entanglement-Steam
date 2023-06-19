using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Objects;
using Entanglement.Patching;

using StressLevelZero.Pool;
using StressLevelZero.Interaction;

using UnityEngine;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class MagazinePlugMessageHandler : NetworkMessageHandler<MagazinePlugMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.MagazinePlug;

        public override NetworkMessage CreateMessage(MagazinePlugMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) * 2 + sizeof(bool)];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.magId), ref index);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.gunId), ref index);

            message.messageData[index++] = Convert.ToByte(data.isInsert);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
#if DEBUG
            EntangleLogger.Log("Received mag sync message!");
#endif

            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

#if DEBUG
            EntangleLogger.Log("Got past load and length!");
#endif

            int index = 0;
            ushort magId = BitConverter.ToUInt16(message.messageData, index);
            index += sizeof(ushort);

            ushort gunId = BitConverter.ToUInt16(message.messageData, index);
            index += sizeof(ushort);

            if (ObjectSync.TryGetSyncable(magId, out Syncable magazine) && ObjectSync.TryGetSyncable(gunId, out Syncable gun)) {
#if DEBUG
                EntangleLogger.Log("Got syncables!");
#endif

                TransformSyncable syncMag = magazine.TryCast<TransformSyncable>();
                TransformSyncable syncGun = gun.TryCast<TransformSyncable>();

                if (syncMag && syncGun) {

                    if (syncMag._CachedPlug && syncGun._CachedGun) {

                        bool isInsert = Convert.ToBoolean(message.messageData[index++]);

                        MagazineSocket magSocket = syncGun._CachedGun.magazineSocket;

                        if (isInsert) {
                            syncMag._CachedPlug.InsertPlug(magSocket);

#if DEBUG
                            EntangleLogger.Log("Trying to insert a magazine!");
#endif
                        }
                        else {
                            syncMag._CachedPlug.ForceEject();

#if DEBUG
                            EntangleLogger.Log("Trying to eject a magazine!");
#endif
                        }
                    }
#if DEBUG
                    else
                        EntangleLogger.Log($"No cached plug or gun? Object names are mag {syncMag.name} and gun {syncGun.name}.");
#endif
                }
#if DEBUG
                else {
                    EntangleLogger.Log("Failed to cast syncables to TransformSyncable!");
                }
#endif
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class MagazinePlugMessageData : NetworkMessageData
    {
        public ushort magId;
        public ushort gunId;
        public bool isInsert = true;
    }
}
