using System.Collections.Generic;

using StressLevelZero.Data;
using StressLevelZero.Pool;

using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Entanglement.Data
{
    public static class SpawnableData
    {
        public static readonly Dictionary<string, SpawnableObject> spawnableObjects = new Dictionary<string, SpawnableObject>();

        public static void GetData() {
            spawnableObjects.Clear();
            
            Il2CppReferenceArray<UnityObject> foundSpawnables = UnityObject.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<SpawnableObject>());
            foreach (UnityObject obj in foundSpawnables) {
                SpawnableObject spawnable = obj.Cast<SpawnableObject>();
                if (!spawnableObjects.ContainsKey(spawnable.title))
                    spawnableObjects.Add(spawnable.title, spawnable);
            }    
        }

        public static bool TryRegister(string title, out SpawnableObject spawnable) {
            bool found = false;
            if (found = spawnableObjects.TryGetValue(title, out spawnable))
                PoolManager.RegisterPool(spawnable);
                
            return found;
        }

        // Tries to get a spawnable, if it can't it tries to register it
        public static SpawnableObject TryGetSpawnable(string title)
        {
            SpawnableObject registered = PoolManager.GetRegisteredSpawnable(title);
            
            if (!registered)
                TryRegister(title, out registered);
            
            return registered;
        }
        
        public static Pool GetSpawnablePool(string title) {
            if (!PoolManager.GetRegisteredSpawnable(title))
                TryRegister(title, out SpawnableObject registered);

            if (!PoolManager.DynamicPools.ContainsKey(title)) return null;

            return PoolManager.DynamicPools[title];
        }
    }
}
