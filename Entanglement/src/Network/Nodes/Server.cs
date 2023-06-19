using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using MelonLoader;

using Discord;

using Entanglement.Representation;
using Entanglement.Data;

using StressLevelZero;

using UnityEngine;

namespace Entanglement.Network
{
    public class Server : Node {
        // Static preferences
        public static byte maxPlayers = 8;
        public static bool isLocked = false;
        public static LobbyType lobbyType = LobbyType.Private;

        // Hard locked settings
        public const byte serverMinimum = 1;
        public const byte serverCapacity = 255;

        // The value of the dict increases with time. When a user sends a heartbeat reset it to 0.
        // If its ever greater than a certain amount of seconds we should disconnect them as they have likely lost connection.
        public Dictionary<long, float> userBeats = new Dictionary<long, float>();

        // There can only be one server, otherwise things will break
        public static Server instance = null;

        public static void StartServer() {
            if (instance != null)
                instance.Shutdown();

            if (DiscordIntegration.isConnected) {
                EntangleLogger.Error("Already in a server!");
                return;
            }

            EntangleLogger.Log($"Started a new server instance!");
            activeNode = instance = new Server();

            if (PlayerScripts.playerHealth)
                PlayerScripts.playerHealth.reloadLevelOnDeath = false;
        }

        //
        // Actual code below
        //

        private Server() {
            LobbyManager lobbyManager = DiscordIntegration.lobbyManager;

            LobbyTransaction createTransaction = lobbyManager.GetLobbyCreateTransaction();

            createTransaction.SetCapacity(maxPlayers);
            createTransaction.SetLocked(isLocked);
            createTransaction.SetType(lobbyType);

            EntangleLogger.Log($"Creating a Discord lobby with a capacity of {maxPlayers} players!");
            DiscordIntegration.lobbyManager.CreateLobby(createTransaction, DiscordLobbyCreateCallback);
        }

        public void DiscordLobbyCreateCallback(Result result, ref Lobby lobby)
        {
            if (result != Result.Ok)
                return;

            DiscordIntegration.lobby = lobby;

            DiscordIntegration.activity.Party = new ActivityParty()
            {
                Id = lobby.Id.ToString(),
                Size = new PartySize() { CurrentSize = 1, MaxSize = maxPlayers }
            };

            DiscordIntegration.activity.Secrets = new ActivitySecrets()
            {
                Join = DiscordIntegration.lobbyManager.GetLobbyActivitySecret(lobby.Id)
            };

            DiscordIntegration.activity.Details = $"Using v{EntanglementMod.VersionString}";
            DiscordIntegration.activity.State = "Hosting a server";

            DiscordIntegration.activity.Assets = DiscordIntegration.CreateAssets(true);

            DiscordIntegration.activity.Instance = true;

            DiscordIntegration.activityManager.UpdateActivity(DiscordIntegration.activity, (res) => { });

            ConnectToDiscordServer();

            DiscordIntegration.lobbyManager.OnNetworkMessage += OnDiscordMessageRecieved;
            DiscordIntegration.lobbyManager.OnMemberConnect += OnDiscordUserJoined;
            DiscordIntegration.lobbyManager.OnMemberDisconnect += OnDiscordUserLeft;
        }

        public override void Tick() {
            if (EntanglementMod.sceneChange != null) {
                EntangleLogger.Log($"Notifying clients of scene change to {EntanglementMod.sceneChange}...");

                LevelChangeMessageData levelChangeData = new LevelChangeMessageData() { sceneIndex = (byte)EntanglementMod.sceneChange, sceneReload = true };
                NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.LevelChange, levelChangeData);

                byte[] msgBytes = message.GetBytes();
                foreach (long user in connectedUsers)
                    SendMessage(user, NetworkChannel.Reliable, msgBytes);

                EntanglementMod.sceneChange = null;
            }

            base.Tick();
        }

        public void UpdateLobbyConfig() {
            LobbyTransaction update = DiscordIntegration.lobbyManager.GetLobbyUpdateTransaction(DiscordIntegration.lobby.Id);

            update.SetCapacity(maxPlayers);
            update.SetLocked(isLocked);
            update.SetType(lobbyType);
            DiscordIntegration.lobbyManager.UpdateLobby(DiscordIntegration.lobby.Id, update, DiscordLobbyUpdateCallback);
        }

        public void DiscordLobbyUpdateCallback(Result res) {
            if (res == Result.Ok) {
                DiscordIntegration.activity.Party.Size.MaxSize = maxPlayers;
                DiscordIntegration.activity.Secrets.Join = isLocked ? null : DiscordIntegration.lobbyManager.GetLobbyActivitySecret(DiscordIntegration.lobby.Id);
                DiscordIntegration.activityManager.UpdateActivity(DiscordIntegration.activity, (value) => { });

                if (maxPlayers < connectedUsers.Count) {
                    uint usersToDisconnect = (uint)connectedUsers.Count - maxPlayers;

                    DisconnectMessageData disconnectData = new DisconnectMessageData();
                    disconnectData.disconnectReason = (byte)DisconnectReason.ServerFull;

                    NetworkMessage disconnectMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Disconnect, disconnectData);
                    byte[] disconnectBytes = disconnectMsg.GetBytes();

                    for (int i = 0; i < usersToDisconnect; i++)
                        SendMessage(connectedUsers[i], NetworkChannel.Reliable, disconnectBytes);
                }
            }
        }

        public void CloseLobby() {
            DisconnectMessageData disconnectData = new DisconnectMessageData();
            disconnectData.disconnectReason = (byte)DisconnectReason.ServerClosed;

            NetworkMessage disconnectMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Disconnect, disconnectData);
            byte[] disconnectBytes = disconnectMsg.GetBytes();
            foreach (long user in connectedUsers) {
                SendMessage(user, NetworkChannel.Reliable, disconnectBytes);
            }

            DiscordIntegration.Tick();
            DiscordIntegration.lobbyManager.DeleteLobby(DiscordIntegration.lobby.Id, (result) => { });
            DiscordIntegration.lobby = new Lobby();

            CleanData();
        }

        public override void Shutdown() {
            if (DiscordIntegration.hasLobby && !DiscordIntegration.isHost) {
                EntangleLogger.Error("Unable to close the server as a client!");
                return;
            }

            CloseLobby();
            DiscordIntegration.DefaultRichPresence();

            instance = null;
            activeNode = Client.instance;
        }

        public override void UserConnectedEvent(long lobbyId, long userId) {
            // currentSceneIndex shouldn't ever be larger than 255 so a byte is fine
            LevelChangeMessageData levelChangeData = new LevelChangeMessageData() { sceneIndex = (byte)StressLevelZero.Utilities.BoneworksSceneManager.currentSceneIndex };
            NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.LevelChange, levelChangeData);
            SendMessage(userId, NetworkChannel.Reliable, message.GetBytes());

            DiscordIntegration.activity.Party.Size.CurrentSize = 1 + connectedUsers.Count;
            DiscordIntegration.activityManager.UpdateActivity(DiscordIntegration.activity, (res) => { });

            foreach (KeyValuePair<byte, long> valuePair in DiscordIntegration.byteIds) {
                if (valuePair.Value == userId) continue;

                ShortIdMessageData addMessageData = new ShortIdMessageData()
                {
                    userId = valuePair.Value,
                    byteId = valuePair.Key,
                };
                NetworkMessage addMessage = NetworkMessage.CreateMessage((byte)BuiltInMessageType.ShortId, addMessageData);
                SendMessage(userId, NetworkChannel.Reliable, addMessage.GetBytes());
            }

            ShortIdMessageData idMessageData = new ShortIdMessageData() {
                userId = userId,
                byteId = DiscordIntegration.RegisterUser(userId)
            };
            NetworkMessage idMessage = NetworkMessage.CreateMessage((byte)BuiltInMessageType.ShortId, idMessageData);
            BroadcastMessage(NetworkChannel.Reliable, idMessage.GetBytes());

            userBeats.Add(userId, 0f);
        }

        public override void UserDisconnectEvent(long lobbyId, long userId) {
            DiscordIntegration.activity.Party.Size.CurrentSize = 1 + connectedUsers.Count;
            DiscordIntegration.activityManager.UpdateActivity(DiscordIntegration.activity, (res) => { });

            userBeats.Remove(userId);
        }

        public override void BroadcastMessage(NetworkChannel channel, byte[] data) => BroadcastMessageP2P(channel, data);

        // Unique to a server host; allows preventing a message sent to the host being sent back
        public void BroadcastMessageExcept(NetworkChannel channel, byte[] data, long toIgnore) => connectedUsers.ForEach((user) => { 
            if (user != toIgnore) { 
                SendMessage(user, channel, data); 
            } 
        });

        public void KickUser(long userId, string userName = null, DisconnectReason reason = DisconnectReason.Kicked) {
            DisconnectMessageData disconnectData = new DisconnectMessageData();
            disconnectData.disconnectReason = (byte)reason;

            NetworkMessage disconnectMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Disconnect, disconnectData);
            byte[] disconnectBytes = disconnectMsg.GetBytes();

            SendMessage(userId, NetworkChannel.Reliable, disconnectBytes);

            if (userName != null)
                EntangleLogger.Log($"Kicked {userName} from the server.");
        }

        public void TeleportTo(long userId) {
            if (PlayerRepresentation.representations.ContainsKey(userId)) {
                PlayerRepresentation rep = PlayerRepresentation.representations[userId];

                PlayerScripts.playerRig.Teleport(rep.repRoot.position);
                PlayerScripts.playerRig.physicsRig.ResetHands(Handedness.BOTH);
            }
        }
    }
}
