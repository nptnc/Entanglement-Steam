using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using MelonLoader;

using Entanglement.Network;

using UnityEngine.EventSystems;

using StressLevelZero.Arena;

namespace Entanglement.Patching
{
    public static class FantasyArena_Settings {
        public static bool m_invalidSettings = false;

        public static void SendEnemyCount(bool isLow) {
            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.FantasyCount, new FantasyEnemyCountMessageData() {
                isLow = isLow,
            });

            byte[] msgBytes = message.GetBytes();
            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);
        }

        public static void SendDifficulty(byte difficulty) {
            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.FantasyDiff, new FantasyDifficultyMessageData() {
                difficulty = difficulty,
            });

            byte[] msgBytes = message.GetBytes();
            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);
        }
    }

    [HarmonyPatch(typeof(UIHapticHoverArena), "OnPointerEnter")]
    public static class ChallengePatch {
        public static void Postfix(UIHapticHoverArena __instance, PointerEventData eventData) {
            if (!__instance.arenaUIControl || !__instance.challenge) return;

            if (__instance.arenaUIControl.activeChallenge == __instance.challenge) {
                Arena_Challenge challenge = __instance.challenge;

#if DEBUG
                EntangleLogger.Log($"Set challenge to {challenge.name}!");
#endif

                // Disgusting linq stuff but it works
                byte index = (byte)Arena_GameManager.instance.masterChallengeList.ToArray().ToList().FindIndex(o => o == challenge);

                NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.FantasyChal, new FantasyChallengeMessageData() {
                    index = index,
                });

                byte[] msgBytes = message.GetBytes();
                Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);
            }
        }
    }

    [HarmonyPatch(typeof(Control_UI_Arena), "SetEasyDifficulty")]
    public static class EasyDifficultyPatch {
        public static void Postfix() {
            if (!FantasyArena_Settings.m_invalidSettings) {
#if DEBUG
                EntangleLogger.Log($"Set easy difficulty!");
#endif

                FantasyArena_Settings.SendDifficulty(0);
            }
            FantasyArena_Settings.m_invalidSettings = false;
        }
    }

    [HarmonyPatch(typeof(Control_UI_Arena), "SetMediumDifficulty")]
    public static class MediumDifficultyPatch
    {
        public static void Postfix() {
            if (!FantasyArena_Settings.m_invalidSettings) {
#if DEBUG
                EntangleLogger.Log($"Set medium difficulty!");
#endif

                FantasyArena_Settings.SendDifficulty(1);
            }
            FantasyArena_Settings.m_invalidSettings = false;
        }
    }

    [HarmonyPatch(typeof(Control_UI_Arena), "SetHardDifficulty")]
    public static class HardDifficultyPatch {
        public static void Postfix() {
            if (!FantasyArena_Settings.m_invalidSettings) {
#if DEBUG
                EntangleLogger.Log($"Set hard difficulty!");
#endif

                FantasyArena_Settings.SendDifficulty(2);
            }
            FantasyArena_Settings.m_invalidSettings = false;
        }
    }

    [HarmonyPatch(typeof(Control_UI_Arena), "ToggleEnemyCount")]
    public static class ToggleEnemyCountPatch {
        public static void Postfix(Control_UI_Arena __instance) {
            bool isLow = __instance.arenaStats.arenaDataPlayer.playerStats.isLowEnemyCount;

#if DEBUG
            EntangleLogger.Log($"Toggling Enemy Count to {(isLow ? "LOW" : "HIGH")}!");
#endif

            FantasyArena_Settings.SendEnemyCount(isLow);
        }
    }

    [HarmonyPatch(typeof(Control_UI_Arena), "OnLoadSaveFile")]
    public static class LoadSaveFilePatch {
        public static void Postfix(Control_UI_Arena __instance) {
            if (Server.instance != null) {
                bool isLow = __instance.arenaStats.arenaDataPlayer.playerStats.isLowEnemyCount;

#if DEBUG
                EntangleLogger.Log($"Loading saved enemy count of {(isLow ? "LOW" : "HIGH")}!");
#endif

                FantasyArena_Settings.SendEnemyCount(isLow);
            }
        }
    }
}
