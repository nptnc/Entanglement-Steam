using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using MelonLoader;

using Entanglement.Representation;
using Entanglement.Data;

using StressLevelZero;

using UnityEngine;
using Entanglement.src.Network.Steam;
using Steamworks.Data;
using Steamworks;
using Entanglement.src.Network;

namespace Entanglement.Network
{
    public class Server : Node {
        // Static preferences
        public static byte maxPlayers = 8;
        public static bool isLocked = false;

        // Hard locked settings
        public const byte serverMinimum = 1;
        public const byte serverCapacity = 255;

        // There can only be one server, otherwise things will break
        public static Server instance = null;

        public static void StartServer() {
            if (instance != null)
                instance.Shutdown();

            if (DiscordIntegration.hasServer) {
                EntangleLogger.Error("Already in a server!");
                return;
            }
            NetworkSender.serverSocket = SteamNetworkingSockets.CreateRelaySocket<ServerSocket>();

            EntangleLogger.Log($"Started a new server instance!");
            activeNode = instance = new Server();

            if (PlayerScripts.playerHealth)
                PlayerScripts.playerHealth.reloadLevelOnDeath = false;
        }

        //
        // Actual code below
        //

        private Server() {
            DiscordIntegration.isHost = true;
            DiscordIntegration.hasServer = true;
        }

        public override void Tick() {
            if (EntanglementMod.sceneChange != null) {
                EntangleLogger.Log($"Notifying clients of scene change to {EntanglementMod.sceneChange}...");

                LevelChangeMessageData levelChangeData = new LevelChangeMessageData() { sceneIndex = (byte)EntanglementMod.sceneChange, sceneReload = true };
                NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.LevelChange, levelChangeData);

                byte[] msgBytes = message.GetBytes();
                BroadcastMessage(SendType.Reliable, msgBytes);

                EntanglementMod.sceneChange = null;
            }

            base.Tick();
        }

        public void CloseLobby() {
            DisconnectMessageData disconnectData = new DisconnectMessageData();
            disconnectData.disconnectReason = (byte)DisconnectReason.ServerClosed;

            NetworkMessage disconnectMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Disconnect, disconnectData);
            byte[] disconnectBytes = disconnectMsg.GetBytes();
            NetworkSender.BroadcastMessageExceptSelf(disconnectBytes);

            NetworkSender.serverSocket.Close();
            NetworkSender.serverSocket = null;

            CleanData();
        }

        public override void Shutdown() {
            if (DiscordIntegration.hasServer && !DiscordIntegration.isHost) {
                EntangleLogger.Error("Unable to close the server as a client!");
                return;
            }

            CloseLobby();
            NetworkSender.serverSocket.Close();

            instance = null;
            activeNode = Client.instance;
        }

        public override void BroadcastMessage(SendType channel, byte[] data) => BroadcastMessageP2P(channel, data);

        // Unique to a server host; allows preventing a message sent to the host being sent back
        public void BroadcastMessageExcept(SendType channel, byte[] data, ulong toIgnore)
        {
            NetworkSender.BroadcastMessageExcept(data, toIgnore, channel);
        }

        public void KickUser(ulong userId, string userName = null, DisconnectReason reason = DisconnectReason.Kicked) {
            DisconnectMessageData disconnectData = new DisconnectMessageData();
            disconnectData.disconnectReason = (byte)reason;

            NetworkMessage disconnectMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Disconnect, disconnectData);
            byte[] disconnectBytes = disconnectMsg.GetBytes();

            SendMessage(userId, SendType.Reliable, disconnectBytes);

            if (userName != null)
                EntangleLogger.Log($"Kicked {userName} from the server.");
        }

        public void TeleportTo(ulong userId) {
            if (PlayerRepresentation.representations.ContainsKey(userId)) {
                PlayerRepresentation rep = PlayerRepresentation.representations[userId];

                PlayerScripts.playerRig.Teleport(rep.repRoot.position);
                PlayerScripts.playerRig.physicsRig.ResetHands(Handedness.BOTH);
            }
        }
    }
}
