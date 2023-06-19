using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StressLevelZero.Pool;
using StressLevelZero.Props.Weapons;

using UnityEngine;

namespace Entanglement.Objects {
    public static class SpawnManager {
        internal static bool SpawnOverride = false;

        private static string[] BlacklistedPools = new string[3] {
            "ProjectilePool",
            "AudioPlayer",
            "Utility Gun",
        };

        public static string GetPoolTitle(Pool pool) => pool.name.Remove(0, 7);

        public static bool IsBlacklisted(this Pool pool) {
            string title = GetPoolTitle(pool);
            for (int i = 0; i < BlacklistedPools.Length; i++)
                if (title == BlacklistedPools[i] || !(pool.Prefab && pool.Prefab.GetComponentInChildren<Rigidbody>() && !pool.Prefab.HasBlacklistedComponent())) return true;
            return false;
        }

        public static bool HasBlacklistedComponent(this GameObject prefab) {
            return prefab.GetComponentInChildren<Magazine>();
        }
    }
}
