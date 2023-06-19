using System;

using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Representation;

using UnityEngine;

using StressLevelZero.Pool;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class PowerPunchMessageHandler : NetworkMessageHandler<PowerPunchMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.PowerPunch;

        public override NetworkMessage CreateMessage(PowerPunchMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ulong) * 2];

            int index = 0;
            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.force.ToULong()), ref index);

            message.messageData = message.messageData.AddBytes(BitConverter.GetBytes(data.localPosition.ToULong()), ref index);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            Vector3 force = BitConverter.ToUInt64(message.messageData, index).ToVector3(); // Decode from ushort
            index += sizeof(ulong);

            PlayerScripts.playerPhysBody.AddImpulseForce(force); // Add force to player

            // Play Effects
            if (PlayerRepresentation.representations.ContainsKey(sender))
            {
                PlayerRepresentation rep = PlayerRepresentation.representations[sender];
                Vector3 localPosition = BitConverter.ToUInt64(message.messageData, index).ToVector3();
                Vector3 position = rep.repRoot.TransformPosition(localPosition);
                Quaternion direction = Quaternion.LookRotation(force.normalized);

                rep.repPowerPunchSFX.transform.position = position;
                rep.repPowerPunchSFX.GravFire();

                PoolSpawner.SpawnBlaster(PoolSpawner.BlasterType.Dust, position, direction);
                PoolSpawner.SpawnSmoker(position, direction);
            }
        }
    }

    public class PowerPunchMessageData : NetworkMessageData {
        public Vector3 force;
        public Vector3 localPosition;
    }
}
