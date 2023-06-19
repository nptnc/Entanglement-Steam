using System;

using UnityEngine;

using StressLevelZero.Combat;
using StabPoint = StressLevelZero.Combat.StabSlash.StabPoint;

using Entanglement.Network;
using Entanglement.Objects;

using HarmonyLib;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(StabSlash.StabPoint), "SpawnStab")]
    public class StabPatch
    {
        public static void Postfix(StabPoint __instance, Transform tran, Collision c, float stabForce, ImpactProperties surfaceProperties) {
            try
            {
                if (__instance.rb)
                {
                    TransformSyncable cachedTransform = TransformSyncable.cache.Get(__instance.rb.gameObject);
                    if (cachedTransform && !cachedTransform.IsOwner()) return;
                }

                Transform root = surfaceProperties.transform.root;
                string objName = root.name;
                if (!objName.Contains("PlayerRep"))
                    return;
                string[] playerName = objName.Split('.');
                if (playerName.Length < 2)
                    throw new IndexOutOfRangeException();
                long id = long.Parse(playerName[1]);

                NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.PlayerAttack, new PlayerAttackMessageData()
                {
                    attackType = AttackType.Stabbing,
                    attackDamage = __instance.damage * surfaceProperties.FireResistance // Since fire is unused its used as a damage multiplier for body parts
                });

                byte[] msgBytes = message.GetBytes();

                Node.activeNode.SendMessage(id, NetworkChannel.Attack, msgBytes);
            }
            catch { }
        }
    }
}
