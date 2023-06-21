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
using Entanglement.src.Network;
using Steamworks;

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
            Instance = this;
            entanglementAssembly = Assembly.GetExecutingAssembly();

            SteamAPILoader.LoadSteamAPI();
            SteamIntegration.Initialize();
            MelonLogger.Log("Steam api initialized");

            
            

            VersionString = $"{EntanglementVersion.versionMajor}.{EntanglementVersion.versionMinor}.{EntanglementVersion.versionPatch}";

            EntangleLogger.Log($"Current Entanglement version is {VersionString}");
            EntangleLogger.Log($"Minimum supported Entanglement version is {EntanglementVersion.minVersionMajorSupported}.{EntanglementVersion.minVersionMinorSupported}.*");

            // ModThatIsNotMod version checking tools, so people know when to update!
            VersionChecking.CheckModVersion(this, "https://boneworks.thunderstore.io/package/Entanglement/Entanglement/");

            PersistentData.Initialize();
            

#if DEBUG
            EntangleLogger.Log("Entanglement Debug Build!", ConsoleColor.Blue);
#endif

            

            // This checks if Steam has an invalid instance, so that the game can proceed without freezing
            if (SteamIntegration.isInvalid) {
                EntangleNotif.InvalidSteam();
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
            if (SteamIntegration.isInvalid) {
                HarmonyInstance.UnpatchSelf();
                hasUnpatched = true;
            }
            else {
                PlayerDeathManager.Initialize();
            }
        }

        public override void OnUpdate() {
            if (SteamIntegration.isInvalid) {
                if (!hasUnpatched) {
                    HarmonyInstance.UnpatchSelf();
                    hasUnpatched = true;
                }
                return; 
            }

            NetworkSender.clientSocket?.Receive(256);
            NetworkSender.serverSocket?.Receive(256);
            SteamClient.RunCallbacks();

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
            if (SteamIntegration.isInvalid) return;

            ModuleHandler.FixedUpdate();

            // Updates the VRIK of all the players
            PlayerRepresentation.UpdatePlayerReps();
        }

        public override void OnLateUpdate() {
            if (SteamIntegration.isInvalid) return;
            
            ModuleHandler.LateUpdate();

            

            Server.instance?.Tick();

            // This will update and flush discords callbacks, not needed for steam
            //DiscordIntegration.Tick();
        }

        private ulong? id = null;
        public override void OnGUI() {
            // TODO: temporary ui until we create lobby system

            int offset = 0;
            int offsetBetweenButtons = 5;
            int defaultOffset = 10;
            
            GUI.Label(new Rect(10,defaultOffset + offset + 20/2 + offsetBetweenButtons,200,20),"steam id");
            offset += 20;
            
            string text = GUI.TextField(new Rect(10,defaultOffset + offset + 20/2 + offsetBetweenButtons,200,20),"");
            offset += 20;
            
            try {
                id = ulong.Parse(text);
            }
            catch { }
            
            if (GUI.Button(new Rect(10, defaultOffset+20/2+offset+offsetBetweenButtons, 200, 20), "Connect")) {
                EntangleLogger.Log("trying to connect...");
                if (id != null)
                    Client.ConnectToServer((ulong)id);
            }
            offset += 20+30;

            GUI.Label(new Rect(10,defaultOffset + offset + 20/2 + offsetBetweenButtons,200,20),"your steam id");
            offset += 20;
            
            GUI.TextField(new Rect(10,defaultOffset + offset + 20/2 + offsetBetweenButtons,200,20),SteamClient.SteamId.ToString());
            offset += 20;
        }
        
        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            if (SteamIntegration.isInvalid) return;

            ModuleHandler.OnSceneWasInitialized(buildIndex, sceneName);

            SpawnableData.GetData();

            PlayerScripts.GetPlayerScripts();

            PlayerRepresentation.GetPlayerTransforms();

            foreach (var rep in PlayerRepresentation.representations.Values)
                rep.RecreateRepresentations();

            Client.instance.currentScene = (byte)buildIndex;
            sceneChange = (byte)buildIndex;
        }

        public override void BONEWORKS_OnLoadingScreen() {
            if (SteamIntegration.isInvalid) return;

            ModuleHandler.OnLoadingScreen();

            LoadingScreen.OverrideScreen();

            ObjectSync.OnCleanup();
            ObjectSync.poolPairs.Clear();

#if DEBUG
            PlayerRepresentation.debugRepresentation = null;
#endif
        }

        public override void OnApplicationQuit() {
            if (SteamIntegration.isInvalid) return;

            ModuleHandler.OnApplicationQuit();

            Node.activeNode.Shutdown();
            SteamIntegration.Shutdown();
        }
    }
}
