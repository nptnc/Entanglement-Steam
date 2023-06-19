using System;

using UnityEngine;

using StressLevelZero.Interaction;

using Entanglement.Network;
using Entanglement.Representation;
using Entanglement.Extensions;

using HarmonyLib;

namespace Entanglement.Patching
{
    public static class ZombieMode_Settings {
        public static bool m_invalidSettings = false;

        public static void SetDifficulty(this Zombie_GameControl __instance, Zombie_GameControl.Difficulty dif) {
            __instance.difficulty = dif;
            __instance.gamePageDiffText.text = dif.ToString();
            __instance.diffText.text = $"DIFFICULTY: {dif}";
            string desc;
            switch (dif) {
                default:
                case Zombie_GameControl.Difficulty.EASY:
                    desc = __instance.easyDesc;
                    break;
                case Zombie_GameControl.Difficulty.MEDIUM:
                    desc = __instance.medDesc;
                    break;
                case Zombie_GameControl.Difficulty.HARD:
                    desc = __instance.hardDesc;
                    break;
                case Zombie_GameControl.Difficulty.HARDER:
                    desc = __instance.harderDesc;
                    break;
                case Zombie_GameControl.Difficulty.HARDEST:
                    desc = __instance.hardestDesc;
                    break;
            }
            __instance.diffDescriptionText.text = desc;
        }
    }

    [HarmonyPatch(typeof(Zombie_GameControl), "SetGameMode")]
    public class GameModePatch {
        public static void Postfix(Zombie_GameControl __instance, int mode) {
            if (!ZombieMode_Settings.m_invalidSettings) {
#if DEBUG
                EntangleLogger.Log($"Setting ZWH GameMode to {mode}!");
#endif

                NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.ZombieMode, new ZombieModeMessageData() {
                    mode = (byte)mode
                });

                byte[] msgBytes = message.GetBytes();

                Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);
            }
            ZombieMode_Settings.m_invalidSettings = false;
        }
    }

    [HarmonyPatch(typeof(Zombie_GameControl), "ToggleLoadout")]
    public class ToggleLoadoutPatch {
        public static void Postfix(Zombie_GameControl __instance, int loadIndex) {
            if (!ZombieMode_Settings.m_invalidSettings) {
#if DEBUG
                EntangleLogger.Log($"Switching to loadout {loadIndex}!");
#endif

                NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.ZombieLoadout, new ZombieLoadoutMessageData() {
                    loadIndex = (byte)loadIndex
                });

                byte[] msgBytes = message.GetBytes();

                Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);
            }
            ZombieMode_Settings.m_invalidSettings = false;
        }
    }

    [HarmonyPatch(typeof(Zombie_GameControl), "ToggleDifficulty")]
    public class ToggleDifficultyPatch {
        public static void Postfix(Zombie_GameControl __instance) {
#if DEBUG
            EntangleLogger.Log($"Switched to difficulty {__instance.difficulty}");
#endif

            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.ZombieDiff, new ZombieDifficultyMessageData() {
                difficulty = __instance.difficulty
            });

            byte[] msgBytes = message.GetBytes();

            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);
        }
    }

    [HarmonyPatch(typeof(Zombie_GameControl), "StartSelectedMode")]
    public class ZombieStartPatch {
        public static void Postfix(Zombie_GameControl __instance) {
            if (!ZombieMode_Settings.m_invalidSettings) {
#if DEBUG
                EntangleLogger.Log("Starting Zombie Warehouse!");
#endif

                NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.ZombieStart, new EmptyMessageData());
                byte[] msgBytes = message.GetBytes();

                Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);
            }

        }
    }
}
