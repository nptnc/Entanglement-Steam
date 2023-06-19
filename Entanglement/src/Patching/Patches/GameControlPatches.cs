using HarmonyLib;

using System;

using Entanglement.Network;

namespace Entanglement.Patching {
    // This patch removes annoying scene reloads which break Entanglement until level reload
    [HarmonyPatch(typeof(GameControl), "RELOADLEVEL")]
    public static class ReloadLevelPatch {
        public static bool Prefix() {
            if (DiscordIntegration.hasLobby)
                return false;
            return true;
        }
    }
}
