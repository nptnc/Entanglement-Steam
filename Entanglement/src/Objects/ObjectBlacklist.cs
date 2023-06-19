using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Entanglement.Extensions;

namespace Entanglement.Objects
{
    public static class ObjectBlacklist {
        private static string[] blacklistedObjects = new string[1] {
            "[RigManager (Default Brett)]",
        };

        public static bool IsBlacklisted(this GameObject obj) {
            for (int i = 0; i < blacklistedObjects.Length; i++)
                if (obj.transform.InHierarchyOf(blacklistedObjects[i])) return true;
            return false;
        }
    }
}
