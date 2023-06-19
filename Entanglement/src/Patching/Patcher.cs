using System.Reflection;

using HarmonyLib;

using Entanglement.Compat;

namespace Entanglement.Patching {
    public static class Patcher {
        public static void Initialize() {
            OptionalAssemblyPatch.AttemptPatches();
        }

        public static void Patch(MethodBase method, HarmonyMethod prefix = null, HarmonyMethod postfix = null) => EntanglementMod.Instance.HarmonyInstance.Patch(method, prefix, postfix);
    }
}
