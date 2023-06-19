using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

using Entanglement.Data;
using Entanglement.Representation;
using Entanglement.Objects;

using Discord;

using UnityEngine;

namespace Entanglement.Network {
    public class Client : Node {
        // Static preferences
        public static bool nameTagsVisible = true;

        // There can only be one client, otherwise things will break
        public static Client instance = null;

        public static void StartClient()
        {
            if (instance != null)
                throw new Exception("Can't create another client instance!");

            EntangleLogger.Log($"Started client!");
            activeNode = instance = new Client();
        }

        //
        // Actual functionality
        //

        public User hostUser;

        public byte currentScene = 0;

        // The value of the dict increases with time. When the server sends a heartbeat reset it to 0.
        // If its ever greater than a certain amount of seconds we should exit the server as the host has likely lost connection.
        public float hostHeartbeat;

        private Client() {
            DiscordIntegration.activityManager.OnActivityJoin += (secret) => DiscordIntegration.lobbyManager.ConnectLobbyWithActivitySecret(secret, DiscordJoinLobby);
        }

        public void DiscordJoinLobby(Result result, ref Lobby lobby) {
            if (DiscordIntegration.hasLobby) {
                EntangleLogger.Error("You are already in a lobby!");
                return;
            }

            if (result != Result.Ok)
                return;

            DiscordIntegration.lobby = lobby;
            ConnectToDiscordServer();

            DiscordIntegration.userManager.GetUser(lobby.OwnerId, OnDiscordHostUserFetched);

            DiscordIntegration.lobbyManager.OnNetworkMessage += OnDiscordMessageRecieved;
            DiscordIntegration.lobbyManager.OnMemberConnect += OnDiscordUserJoined;
            DiscordIntegration.lobbyManager.OnMemberDisconnect += OnDiscordUserLeft;

            IEnumerable<User> users = DiscordIntegration.lobbyManager.GetMemberUsers(lobby.Id);

            foreach (User user in users)
                if (user.Id != DiscordIntegration.currentUser.Id && user.Id != lobby.OwnerId)
                    CreatePlayerRep(user.Id);

            DiscordIntegration.activity.Party = new ActivityParty()
            {
                Id = lobby.Id.ToString(),
                Size = new PartySize() { CurrentSize = users.Count(), MaxSize = (int)lobby.Capacity }
            };

            DiscordIntegration.activity.Secrets = new ActivitySecrets()
            {
                Join = DiscordIntegration.lobbyManager.GetLobbyActivitySecret(lobby.Id)
            };

            DiscordIntegration.activity.Details = $"Using v{EntanglementMod.VersionString}";
            DiscordIntegration.activity.State = "Playing in a server";

            DiscordIntegration.activity.Assets = DiscordIntegration.CreateAssets(true);

            DiscordIntegration.activity.Instance = true;

            DiscordIntegration.UpdateActivity();

            ObjectSync.OnCleanup();

            if (PlayerScripts.playerHealth)
                PlayerScripts.playerHealth.reloadLevelOnDeath = false;
        }

        public void OnDiscordHostUserFetched(Result result, ref User user) {
            PlayerRepresentation.representations.Add(user.Id, new PlayerRepresentation(user.Username, user.Id));
            userDatas.Add(user.Id, user);

            hostUser = user;
            EntangleLogger.Log($"Joined {hostUser.Username}'s server!");
            EntangleNotif.JoinServer(hostUser.Username);

            // Test our connection by sending our connection message
            ConnectionMessageData connectionData = new ConnectionMessageData();
            connectionData.packedVersion = BitConverter.ToUInt16(new byte[] { EntanglementVersion.versionMajor, EntanglementVersion.versionMinor }, 0);

            NetworkMessage conMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Connection, connectionData);
            SendMessage(hostUser.Id, NetworkChannel.Reliable, conMsg.GetBytes());

            DiscordIntegration.RegisterUser(hostUser.Id, 0);
        }

        public override void UserConnectedEvent(long lobbyId, long userId) {
            DiscordIntegration.activity.Party.Size.CurrentSize = 1 + connectedUsers.Count;
            DiscordIntegration.activityManager.UpdateActivity(DiscordIntegration.activity, (res) => { });
        }

        public override void UserDisconnectEvent(long lobbyId, long userId) {
            DiscordIntegration.activity.Party.Size.CurrentSize = 1 + connectedUsers.Count;
            DiscordIntegration.activityManager.UpdateActivity(DiscordIntegration.activity, (res) => { });
        }

        public void DisconnectFromServer(bool notif = true) {
            if (notif)
                EntangleNotif.LeftServer();

            DiscordIntegration.lobbyManager.DisconnectLobby(DiscordIntegration.lobby.Id, (res) => { });
            DiscordIntegration.lobby = new Lobby(); // Clear the lobby
            DiscordIntegration.DefaultRichPresence();
            CleanData();
        }

        public override void BroadcastMessage(NetworkChannel channel, byte[] data) => SendMessage(hostUser.Id, channel, data);

        // Client.Shutdown is ran on closing the game
        public override void Shutdown() {
            DisconnectFromServer();
        }
    }
}
