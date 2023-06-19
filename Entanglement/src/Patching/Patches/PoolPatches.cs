using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Objects;
using Entanglement.Network;
using Entanglement.Extensions;
using Entanglement.Data;

using StressLevelZero.Pool;
using StressLevelZero.AI;

using HarmonyLib;

using UnityEngine;

using PuppetMasta;

using MelonLoader;

namespace Entanglement.Patching {

    public static class Pool_Settings {
        public static List<Poolee> GetAllPoolees(this Pool pool) {
            List<Poolee> poolees;
            if (!ObjectSync.poolPairs.TryGetValue(pool, out poolees)) {
                poolees = new List<Poolee>();
                ObjectSync.poolPairs.Add(pool, poolees);
            }
            return poolees;
        }

        public static float GetRelativeSpawnTime(this Poolee poolee) {
            if (!poolee.pool) return -1f;
            float timeSpawned = poolee.gameObject.activeInHierarchy ? poolee.timeSpawned : 0f;
            return (float)poolee.pool._timeOfLastSpawn - timeSpawned;
        }

        public static Poolee GetAccuratePoolee(this Pool pool, int index, float relativeTime = -1f) {
            List<Poolee> poolees = pool.GetAllPoolees();
            if (poolees.Count <= 0) return null;

            Poolee toReturn = null;

            if (relativeTime < 0f)
                return poolees[Math.Min(index, poolees.Count() - 1)];
            else {
                int closestIndex = -1;
                float closestTime = -1f;
                for (int thisIndex = 0; thisIndex < poolees.Count(); thisIndex++) {
                    Poolee thisPoolee = poolees[thisIndex];
                    float thisTime = thisPoolee.GetRelativeSpawnTime();

                    if (Mathf.Abs(thisTime - relativeTime) > Mathf.Abs(closestTime - relativeTime))
                        continue;

                    if (Math.Abs(thisIndex - index) > Math.Abs(closestIndex - index))
                        continue;

                    closestIndex = thisIndex;
                    closestTime = thisTime;
                    toReturn = thisPoolee;
                }
            }

            return toReturn;
        }
    }

    [HarmonyPatch(typeof(Poolee), "OnCleanup")]
    public static class CleanupPatch {
        public static void Prefix(Poolee __instance, ref SimplifiedTransform __state) {
            __state = new SimplifiedTransform(__instance.transform);
        }

        public static void Postfix(Poolee __instance, ref SimplifiedTransform __state) {
            __state.Apply(__instance.transform);
        }
    }

    [HarmonyPatch(typeof(Pool), "InstantiatePoolee")]
    public static class InstantiatePatch {
        // We patch the prefix to prevent the NullReferenceException in the base game- The error didn't break anything but it was annoying/caused confusion
        public static bool Prefix(Pool __instance) {
            if (!__instance.Prefab)
                return false;
            return true;
        }

        public static void Postfix(Pool __instance, Poolee __result, Vector3 position, Quaternion rotation) {
            if (__instance.IsBlacklisted())
                return;

            try {
                if (__instance._pooledObjects.Contains(__result)) {
                    List<Poolee> poolees;

                    if (!ObjectSync.poolPairs.TryGetValue(__instance, out poolees))
                    {
                        poolees = new List<Poolee>();
                        if (ObjectSync.poolPairs.ContainsKey(__instance))
                            ObjectSync.poolPairs[__instance] = poolees;
                        else
                            ObjectSync.poolPairs.Add(__instance, poolees);
                    }

                    if (!poolees.Contains(__result)) { 
                        poolees.Add(__result);
                        __result.onSpawnDelegate = Il2CppSystem.Delegate.Combine(__result.onSpawnDelegate, (Il2CppSystem.Action<GameObject>)((go) => { OnSpawn(go, __instance); })).Cast<Il2CppSystem.Action<GameObject>>();
                    }
                }
            } catch { }
        }

        public static void OnSpawn(GameObject spawnedObject, Pool pool) {
            if (!DiscordIntegration.hasLobby || SpawnManager.SpawnOverride)
                return;

            // We don't want to dupe items
            var pooleeSyncable = PooleeSyncable._Cache.Get(spawnedObject);
            if (pooleeSyncable) {
                // Now we transfer the spawn to the host
                if (Node.isServer) {
                    // Set us as owner
                    pooleeSyncable.SetOwner(DiscordIntegration.currentUser.Id);

                    SpawnTransferMessageData data = new SpawnTransferMessageData()
                    {
                        spawnId = pooleeSyncable.id,
                        transform = new SimplifiedTransform(spawnedObject.transform),
                    };

                    NetworkMessage transferMessage = NetworkMessage.CreateMessage(BuiltInMessageType.SpawnTransfer, data);
                    Node.activeNode.BroadcastMessage(NetworkChannel.Object, transferMessage.GetBytes());
                }
                return;
            }

            // Server spawned (---> Clients)
            if (Node.isServer) {
                MelonCoroutines.Start(OnSpawnHost(spawnedObject, pool));
            }

            // Client spawned (We ignore this, spawn gun sync will handle sending Client ---> Server ---> Clients)
            else {
                MelonCoroutines.Start(OnSpawnClient(spawnedObject));
            }
        }

        public static IEnumerator OnSpawnClient(GameObject spawnedObject) {
            // Wait till we finish loading
            if (SceneLoader.loading) {
                while (SceneLoader.loading)
                    yield return null;
            }

            for (int i = 0; i < 2; i++) {
                yield return null;
                if (spawnedObject)
                    spawnedObject.SetActive(false);
            }
        }

        public static IEnumerator OnSpawnHost(GameObject spawnedObject, Pool pool) {
            // Wait till we finish loading
            if (SceneLoader.loading) {
                while (SceneLoader.loading)
                    yield return null;

                if (!spawnedObject.activeInHierarchy)
                    yield break;
            }

            // Get the next id for use in sync
            ushort id = ObjectSync.lastId;
            id++;

            // Create the sync transforms
            Rigidbody[] rbs = spawnedObject.GetComponentsInChildren<Rigidbody>();
            byte rbCount = (byte)rbs.Length;
            for (ushort i = 0; i < rbs.Length; i++)
            {
                Rigidbody rb = rbs[i];
                GameObject go = rb.gameObject;
                ushort thisId = (ushort)(i + id);

                TransformSyncable existingSync = TransformSyncable.cache.GetOrAdd(go);
                if (existingSync) {
                    ObjectSync.MoveSyncable(existingSync, thisId);
                    existingSync.ClearOwner();
                    existingSync.TrySetStale(DiscordIntegration.lobby.OwnerId);
                }
                else {
                    TransformSyncable.CreateSync(DiscordIntegration.lobby.OwnerId, ComponentCacheExtensions.m_RigidbodyCache.GetOrAdd(go), thisId);
                }

                ObjectSync.lastId = thisId;
            }

            // Now we sync this object back to the clients
            SpawnClientMessageData data = new SpawnClientMessageData()
            {
                rbCount = rbCount,
                spawnId = id,
                title = SpawnManager.GetPoolTitle(pool),
                transform = new SimplifiedTransform(spawnedObject.transform),
            };

            NetworkMessage clientMessage = NetworkMessage.CreateMessage(BuiltInMessageType.SpawnClient, data);
            Node.activeNode.BroadcastMessage(NetworkChannel.Object, clientMessage.GetBytes());

            var pooleeSyncable = spawnedObject.AddComponent<PooleeSyncable>();
            pooleeSyncable.id = id;
            pooleeSyncable.transforms = spawnedObject.GetComponentsInChildren<TransformSyncable>(true);
        }
    }
}
