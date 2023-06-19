using UnityEngine;

using StressLevelZero.Pool;
using StressLevelZero.Props.Weapons;
using StressLevelZero.Data;

using HarmonyLib;

using Entanglement.Network;
using Entanglement.Data;
using Entanglement.Objects;

using System.Collections;

using MelonLoader;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(SpawnGun), "OnFire")]
    public class SpawnFirePatch
    {
        public static void Postfix(SpawnGun __instance) {
            if (!DiscordIntegration.hasLobby)
                return;

            MelonCoroutines.Start(SpawnGunFire(__instance));
        }

        public static IEnumerator SpawnGunFire(SpawnGun __instance)
        {
            SpawnableObject spawnable = __instance._selectedSpawnable;

            if (__instance._selectedMode != UtilityModes.SPAWNER || !spawnable)
                yield break;

            yield return null;

            yield return null;

            Pool objPool = PoolManager.GetPool(spawnable.title);

            if (!objPool)
                yield break;

            Poolee lastSpawn = objPool._lastSpawn;
            if (!lastSpawn)
                yield break;

            if (!Node.isServer) {
                Transform objTransform = lastSpawn.transform;
                Vector3 position = objTransform.position;
                Quaternion rotation = objTransform.rotation;

                SpawnRequestMessageData data = new SpawnRequestMessageData()
                {
                    title = spawnable.title,
                    transform = new SimplifiedTransform(position, rotation),
                };

                NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.SpawnRequest, data);
                Node.activeNode.BroadcastMessage(NetworkChannel.Object, message.GetBytes());

                // We need to make sure this object is disabled
                lastSpawn.gameObject.SetActive(false);
            }
        }
    }
}
