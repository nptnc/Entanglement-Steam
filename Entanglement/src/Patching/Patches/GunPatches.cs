using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using StressLevelZero.Combat;
using StressLevelZero.Props.Weapons;
using StressLevelZero.Pool;

using Entanglement.Data;
using Entanglement.Network;

using HarmonyLib;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(Gun), "OnFire")]
    public class GunShotPatch
    {
        public static void Prefix(Gun __instance) {
            BulletObject bulletObject = __instance.chamberedCartridge;
            Transform firePoint = __instance.firePointTransform;

            if (!firePoint || !bulletObject) return;

            GunShotMessageData shotData = new GunShotMessageData()
            {
                userId = DiscordIntegration.currentUser.Id,
                bulletObject = bulletObject,
                bulletTransform = new SimplifiedTransform(firePoint)
            };

            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.GunShot, shotData);
            Node.activeNode.BroadcastMessage(NetworkChannel.Attack, message.GetBytes());
        }
    }

    [HarmonyPatch(typeof(BalloonGun), "OnFire")]
    public class BalloonShotPatch
    {
        public static void Prefix(BalloonGun __instance) {
            Transform firePoint = __instance.firePointTransform;

            if (!firePoint) return;

            BalloonShotMessageData balloonData = new BalloonShotMessageData()
            {
                userId = DiscordIntegration.currentUser.Id,
                balloonColor = __instance.currentColor,
                balloonTransform = new SimplifiedTransform(firePoint),
            };

            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.BalloonShot, balloonData);
            Node.activeNode.BroadcastMessage(NetworkChannel.Attack, message.GetBytes());
        }
    }
}
