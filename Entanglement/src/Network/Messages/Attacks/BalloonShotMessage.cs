using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using StressLevelZero.Pool;
using BalloonColor = StressLevelZero.Props.Balloon.BalloonColor;

using Entanglement.Representation;
using Entanglement.Data;
using Entanglement.Extensions;

namespace Entanglement.Network
{
    [Net.SkipHandleOnLoading]
    public class BalloonShotMessageHandler : NetworkMessageHandler<BalloonShotMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.BalloonShot;

        public override NetworkMessage CreateMessage(BalloonShotMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(byte) * 2 + SimplifiedTransform.size_small];

            int index = 0;
            // User
            message.messageData[index++] = DiscordIntegration.GetByteId(data.userId);
            // Color
            message.messageData[index++] = (byte)data.balloonColor;
            // Transform
            byte[] transformBytes = data.balloonTransform.GetSmallBytes(PlayerRepresentation.syncedRoot.position);
            for (int i = 0; i < SimplifiedTransform.size_small; i++)
                message.messageData[index++] = transformBytes[i];

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            // User
            long userId = DiscordIntegration.GetLongId(message.messageData[index++]);
            // Color
            BalloonColor balloonColor = (BalloonColor)message.messageData[index++];
            // Spawn Effects
            if (PlayerRepresentation.representations.ContainsKey(userId))
            {
                PlayerRepresentation rep = PlayerRepresentation.representations[userId];

                // Transform
                byte[] transformBytes = new byte[SimplifiedTransform.size_small];
                for (int i = 0; i < transformBytes.Length; i++)
                    transformBytes[i] = message.messageData[index++];
                SimplifiedTransform balloonTransform = SimplifiedTransform.FromSmallBytes(transformBytes, rep.repRoot.position);
                // Spawn Balloon
                Vector3 position = balloonTransform.position;
                Quaternion rotation = balloonTransform.rotation.ExpandQuat();

                PoolSpawner.SpawnBalloonProjectile(position, rotation, balloonColor);
                PoolSpawner.SpawnMuzzleFlare(position, rotation, PoolSpawner.MuzzleFlareType.Default);

                // Play Sounds
                balloonTransform.Apply(rep.repBalloonSFX.transform);
                rep.repBalloonSFX.GunShot();
            }

            if (Server.instance != null)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Attack, msgBytes, userId);
            }
        }
    }

    public class BalloonShotMessageData : NetworkMessageData
    {
        public long userId;
        public BalloonColor balloonColor;
        public SimplifiedTransform balloonTransform;
    }
}
