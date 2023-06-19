using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using MelonLoader;

using Entanglement.Representation;
using Entanglement.Network;
using Entanglement.Data;
using Entanglement.Patching;
using Entanglement.UI;
using Entanglement.Objects;
using Entanglement.Compat;
using Entanglement.Extensions;
using Entanglement.Modularity;
using Entanglement.Managers;

using ModThatIsNotMod;

using UnityEngine;

// This mod is not a rewrite of the multiplayer mod!
// It is another MP mod made by an ex developer of the MP mod that was unsatisfied with the original mod's codebase
// There is no shared code between the two projects and any similar code is accidental / coincidental

namespace Entanglement {
    // We can compare with peers to see if they are on a supported version
    public struct EntanglementVersion {
        public const byte versionMajor = 0;
        public const byte versionMinor = 3;
        public const short versionPatch = 0;

        // Patches don't matter too much when supporting old versions
        // Although we don't support anything newer than the current version, just in case
        public const byte minVersionMajorSupported = 0;
        public const byte minVersionMinorSupported = 3;
    }

    public class EntanglementMod : MelonMod {
        public static byte? sceneChange = null;
        public static Assembly entanglementAssembly;

        public static EntanglementMod Instance { get; protected set; }
        public static string VersionString { get; protected set; }

        public static bool hasUnpatched = false;

        public override void OnApplicationStart() {
            entanglementAssembly = Assembly.GetExecutingAssembly();
            Instance = this;

            VersionString = $"{EntanglementVersion.versionMajor}.{EntanglementVersion.versionMinor}.{EntanglementVersion.versionPatch}";

            EntangleLogger.Log($"Current Entanglement version is {VersionString}");
            EntangleLogger.Log($"Minimum supported Entanglement version is {EntanglementVersion.minVersionMajorSupported}.{EntanglementVersion.minVersionMinorSupported}.*");

            // ModThatIsNotMod version checking tools, so people know when to update!
            VersionChecking.CheckModVersion(this, "https://boneworks.thunderstore.io/package/Entanglement/Entanglement/");

            PersistentData.Initialize();
            GameSDK.LoadGameSDK();

#if DEBUG
            EntangleLogger.Log("Entanglement Debug Build!", ConsoleColor.Blue);
#endif

            DiscordIntegration.Initialize();

            // This checks if Discord has an invalid instance, so that the game can proceed without freezing
            if (DiscordIntegration.isInvalid) {
                EntangleNotif.InvalidDiscord();
                return; 
            }

            Patcher.Initialize();

            NetworkMessage.RegisterHandlersFromAssembly(entanglementAssembly);

            Client.StartClient();

            PlayerRepresentation.LoadBundle();
            LoadingScreen.LoadBundle();

            EntanglementUI.CreateUI();

            BanList.PullFromFile();

            // TODO: Remove this upon full release
            EntangleLogger.Log("Welcome to the Entanglement pre-release!", ConsoleColor.DarkYellow);

        }

        // Unpatch methods if discord isn't found
        public override void OnApplicationLateStart() {
            if (DiscordIntegration.isInvalid) {
                HarmonyInstance.UnpatchSelf();
                hasUnpatched = true;
            }
            else {
                PlayerDeathManager.Initialize();
            }
        }

        public override void OnUpdate() {
            if (DiscordIntegration.isInvalid) {
                if (!hasUnpatched) {
                    HarmonyInstance.UnpatchSelf();
                    hasUnpatched = true;
                }
                return; 
            }

            ModuleHandler.Update();

#if DEBUG
            if (Input.GetKeyDown(KeyCode.S))
                Server.StartServer();

            if (Input.GetKeyDown(KeyCode.K))
                Server.instance?.Shutdown();

            if (Input.GetKeyDown(KeyCode.R)) {
                if (PlayerRepresentation.debugRepresentation == null)
                    PlayerRepresentation.debugRepresentation = new PlayerRepresentation("Dummy", 0);
                else
                    PlayerRepresentation.debugRepresentation.CreateRagdoll();

            }
#endif

            StatsUI.UpdateUI();
            PlayerRepresentation.SyncPlayerReps();
            DataTransaction.Process();
        }

        public override void OnFixedUpdate() {
            if (DiscordIntegration.isInvalid) return;

            ModuleHandler.FixedUpdate();

            // Updates the VRIK of all the players
            PlayerRepresentation.UpdatePlayerReps();
        }

        public override void OnLateUpdate() {
            if (DiscordIntegration.isInvalid) return;
            
            ModuleHandler.LateUpdate();

            Client.instance?.Tick();
            Server.instance?.Tick();

            // This will update and flush discords callbacks
            DiscordIntegration.Tick();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            if (DiscordIntegration.isInvalid) return;

            ModuleHandler.OnSceneWasInitialized(buildIndex, sceneName);

            SpawnableData.GetData();

            PlayerScripts.GetPlayerScripts();

            PlayerRepresentation.GetPlayerTransforms();

            foreach (var rep in PlayerRepresentation.representations.Values)
                rep.RecreateRepresentations();

            Client.instance.currentScene = (byte)buildIndex;
            sceneChange = (byte)buildIndex;

            DiscordIntegration.targetScene = sceneName.ToLower();
            DiscordIntegration.activity.Assets = DiscordIntegration.CreateAssets(DiscordIntegration.hasLobby);
            DiscordIntegration.UpdateActivity();
        }

        public override void BONEWORKS_OnLoadingScreen() {
            if (DiscordIntegration.isInvalid) return;

            ModuleHandler.OnLoadingScreen();

            LoadingScreen.OverrideScreen();

            ObjectSync.OnCleanup();
            ObjectSync.poolPairs.Clear();

#if DEBUG
            PlayerRepresentation.debugRepresentation = null;
#endif
        }

        public override void OnApplicationQuit() {
            if (DiscordIntegration.isInvalid) return;

            ModuleHandler.OnApplicationQuit();

            Node.activeNode.Shutdown();
            DiscordIntegration.Shutdown();
        }
    }
}
