using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

using Discord;

using MelonLoader;

using UnityEngine;

using Entanglement.UI;
using Entanglement.Extensions;

namespace Entanglement.Network
{
    public enum VoiceStatus {
        Disabled,
        Enabled,
    }

    public static class DiscordIntegration {
        public const string multiplayerIcon = "entanglement";
        public const string singleplayerIcon = "boneworks";
        public const string notHosting = "This user isn't hosting a game!";

        public static string targetScene = "undefined";


        public static Discord.Discord discord;

        public static ActivityManager activityManager;
        public static LobbyManager lobbyManager;
        public static UserManager userManager;
        public static ImageManager imageManager;
        public static VoiceManager voiceManager;

        public static Activity activity;
        public static Lobby lobby;

        public static User currentUser;

        public static bool isInvalid;

        public static bool hasLobby => lobby.Id != 0;

        public static bool isHost => hasLobby && lobby.OwnerId == currentUser.Id;

        public static bool isConnected => hasLobby && lobby.OwnerId != currentUser.Id;

        public static VoiceStatus voiceStatus = VoiceStatus.Disabled;

        public static Dictionary<byte, long> byteIds = new Dictionary<byte, long>();

        // An ID of 0 is reserved for the host
        public static byte localByteId = 0;
        public static byte lastByteId = 1;

        public static long GetLongId(byte shortId) {
            if (shortId == 0) return lobby.OwnerId;

            return byteIds.TryIdx(shortId);
        }

        public static byte GetByteId(long longId) {
            if (longId == currentUser.Id) return localByteId;
            
            return byteIds.FirstOrDefault(o => o.Value == longId).Key;
        }

        public static byte CreateByteId() => lastByteId++;

        public static void RegisterUser(long userId, byte byteId) => byteIds.Add(byteId, userId);

        public static byte RegisterUser(long userId) {
            byte byteId = CreateByteId();
            RegisterUser(userId, byteId);
            return byteId;
        }

        public static void RemoveUser(long userId) => byteIds.Remove(GetByteId(userId));

        public static void Initialize() {
            try {
                discord = new Discord.Discord(883939020269293618, (ulong)CreateFlags.NoRequireDiscord);
                activityManager = discord.GetActivityManager();

                lobbyManager = discord.GetLobbyManager();

                userManager = discord.GetUserManager();

                userManager.OnCurrentUserUpdate += () =>
                {
                    currentUser = userManager.GetCurrentUser();
                    EntangleLogger.Log($"Current Discord User: {currentUser.Username}");
                };

                imageManager = discord.GetImageManager();

                voiceManager = discord.GetVoiceManager();

                DefaultRichPresence();
            }
            catch (ResultException e) {
                EntangleLogger.Error($"Failed to initialize the Discord Client! Continuing without Entanglement!\nDid you make sure to start the game with discord open and have discord installed?\nIf both are true, try to reinstall discord.\nFailed with reason: {e.Message}\nTrace: {e.StackTrace}");
                isInvalid = true;
            }

        }

        public static void DefaultRichPresence()
        {
            activity = new Activity()
            {
                Name = "Entanglement",
                State = "Playing solo",
                Details = $"Using v{EntanglementMod.VersionString}",
                Assets = CreateAssets(false)
            };

            activity.Instance = false;

            UpdateActivity();
        }

        public static ActivityAssets CreateAssets(string largeImage = "placeholder", bool inServer = false)
        {
            string smallImage = inServer ? multiplayerIcon : singleplayerIcon;
            string largeText = inServer ? ParseScene(targetScene) : notHosting;

            return new ActivityAssets() { LargeImage = largeImage, SmallImage = smallImage, LargeText = largeText };
        }

        public static ActivityAssets CreateAssets(bool inServer) => CreateAssets($"level_{targetScene}", inServer);

        /// <summary>
        /// Parses the scene name to fit the actual name in the menu
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static string ParseScene(string scene) {
            switch (scene.ToLower()) {
                default:
                    return scene;
                case "sandbox_blankbox":
                    return "Blankbox";
                case "scene_redactedchamber":
                    return "[REDACTED] Chamber";
                case "scene_mainmenu":
                    return "Main Menu";
                case "scene_breakroom":
                    return "Breakroom";
                case "scene_streets":
                    return "Streets";
                case "scene_tuscany":
                    return "Tuscany";
                case "zombie_warehouse":
                    return "Zombie Warehouse";
                case "scene_throneroom":
                    return "Throne Room";
                case "scene_runoff":
                    return "Runoff";
                case "scene_arena":
                    return "Arena Campaign";
                case "arena_fantasy":
                    return "Arena Gamemode";
                case "sandbox_museumbasement":
                    return "Museum Basement";
                case "sandbox_handgunbox":
                    return "Handgun Range";
                case "scene_hoverjunkers":
                    return "Hover Junkers";
                case "scene_tower":
                    return "Tower";
                case "scene_warehouse":
                    return "Warehouse";
                case "scene_towerboss":
                    return "Time Tower";
                case "scene_sewerstation":
                    return "Sewers";
                case "scene_museum":
                    return "Museum";
                case "scene_dungeon":
                    return "Dungeon";
                case "scene_subwaystation":
                    return "Central Station";
            }
        }

        // Shortcut method for refreshing the user's Discord RP
        public static void UpdateActivity() => activityManager.UpdateActivity(activity, (x) => { });

        public static void Update() => discord.RunCallbacks();

        public static void Flush() => lobbyManager.FlushNetwork();

        public static void Tick() {
            Update();
            Flush();
        }

        public static void Shutdown() {
            discord.ActivityManagerInstance.ClearActivity((x) => { });
            discord.Dispose();
        }

        public static void UpdateVoice(VoiceStatus status) {
            voiceStatus = status;

            if (!hasLobby) return;

            switch (status) {
                default:
                case VoiceStatus.Disabled:
                    lobbyManager.DisconnectVoice(lobby.Id, (res) => { });
                    break;
                case VoiceStatus.Enabled:
                    lobbyManager.ConnectVoice(lobby.Id, (res) => { });
                    break;
            }
        }
    }
}
