using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;

using Discord;

using Entanglement.Representation;
using Entanglement.Compat.Playermodels;
using Entanglement.Objects;
using Entanglement.Compat;
using Entanglement.Data;

namespace Entanglement.Network {
    public abstract class Node {
        public List<long> connectedUsers = new List<long>();
        public Dictionary<long, User> userDatas = new Dictionary<long, User>();

        // Reset per frame, but used in Entanglement -> Stats to see the network load
        public uint sentByteCount, recievedByteCount;

        public static Node activeNode;

        public static bool isServer => activeNode is Server;

        public void ConnectToDiscordServer() {
            DiscordIntegration.lobbyManager.ConnectNetwork(DiscordIntegration.lobby.Id);

            // Opens all the network channels for sending messages
            DiscordIntegration.lobbyManager.OpenNetworkChannel(DiscordIntegration.lobby.Id, (byte)NetworkChannel.Reliable, true);
            DiscordIntegration.lobbyManager.OpenNetworkChannel(DiscordIntegration.lobby.Id, (byte)NetworkChannel.Unreliable, false);
            DiscordIntegration.lobbyManager.OpenNetworkChannel(DiscordIntegration.lobby.Id, (byte)NetworkChannel.Attack, true);
            DiscordIntegration.lobbyManager.OpenNetworkChannel(DiscordIntegration.lobby.Id, (byte)NetworkChannel.Object, true);
            DiscordIntegration.lobbyManager.OpenNetworkChannel(DiscordIntegration.lobby.Id, (byte)NetworkChannel.Transaction, true);

            DiscordIntegration.UpdateVoice(DiscordIntegration.voiceStatus);
        }

        public void OnDiscordUserJoined(long lobbyId, long userId) {
            CreatePlayerRep(userId);

            // Send PlayerModel
            if (PlayermodelsPatch.lastLoadedPath != null) {
                string path = PlayermodelsPatch.lastLoadedPath;
                LoadCustomPlayerMessageData msgData = new LoadCustomPlayerMessageData();
                msgData.userId = DiscordIntegration.currentUser.Id;
                msgData.modelPath = Path.GetFileName(path);
                msgData.requestCallback = true;
                SendMessage(userId, NetworkChannel.Reliable, NetworkMessage.CreateMessage(CompatMessageType.PlayerModel, msgData).GetBytes());
            }

            UserConnectedEvent(lobbyId, userId);
        }

        public void OnDiscordUserLeft(long lobbyId, long userId) {
            EntangleNotif.PlayerLeave($"{PlayerRepresentation.representations[userId].playerName}");

            PlayerRepresentation.representations[userId].DeleteRepresentations();
            PlayerRepresentation.representations.Remove(userId);
            userDatas.Remove(userId);
            connectedUsers.Remove(userId);
            DiscordIntegration.RemoveUser(userId);

            UserDisconnectEvent(lobbyId, userId);
        }

        public void CreatePlayerRep(long userId)
        {
            if (connectedUsers.Contains(userId))
                return;

            connectedUsers.Add(userId);
            DiscordIntegration.userManager.GetUser(userId, OnDiscordUserFetched);
        }

        public void CleanData() {
            connectedUsers.Clear();
            userDatas.Clear();
            ObjectSync.OnCleanup();

            foreach (PlayerRepresentation playerRep in PlayerRepresentation.representations.Values)
                playerRep.DeleteRepresentations();

            PlayerRepresentation.representations.Clear();
            DiscordIntegration.byteIds.Clear();
            DiscordIntegration.localByteId = 0;
            DiscordIntegration.lastByteId = 1;

            if (PlayerScripts.playerHealth)
                PlayerScripts.playerHealth.reloadLevelOnDeath = PlayerScripts.reloadLevelOnDeath;

            DiscordIntegration.lobbyManager.OnNetworkMessage -= OnDiscordMessageRecieved;
            DiscordIntegration.lobbyManager.OnMemberConnect -= OnDiscordUserJoined;
            DiscordIntegration.lobbyManager.OnMemberDisconnect -= OnDiscordUserLeft;

            CleanupEvent();
        }

        public void OnDiscordUserFetched(Result result, ref User user) {
            PlayerRepresentation.representations.Add(user.Id, new PlayerRepresentation(user.Username, user.Id));
            userDatas.Add(user.Id, user);

            EntangleNotif.PlayerJoin($"{user.Username}");
        }

        public void OnDiscordMessageRecieved(long lobbyId, long userId, byte channelId, byte[] data)
        {
            if (data.Length <= 0) // Idk
                throw new Exception("Data was invalid!");

            NetworkMessage message = new NetworkMessage();

            message.messageType = data[0];
            message.messageData = new byte[data.Length - sizeof(byte)];

            for (int b = sizeof(byte); b < data.Length; b++)
                message.messageData[b - sizeof(byte)] = data[b];

            recievedByteCount += (uint)data.Length;
            NetworkMessage.ReadMessage(message, userId);
        }

        public void SendMessage(long userId, NetworkChannel channel, byte[] data) {
            if (DiscordIntegration.lobby.Id != 0) { 
                DiscordIntegration.lobbyManager.SendNetworkMessage(DiscordIntegration.lobby.Id, userId, (byte)channel, data);
                sentByteCount += (uint)data.Length;
            }
        }

        // Sends to owner if client
        // Sends to all if server
        public virtual void BroadcastMessage(NetworkChannel channel, byte[] data) { }

        // Forces send in every direction (for P2P-like messages, lowers latency but not good for certain things!)
        public void BroadcastMessageP2P(NetworkChannel channel, byte[] data) { 
            connectedUsers.ForEach((user) => { SendMessage(user, channel, data); });

            if (!isServer)
                SendMessage(DiscordIntegration.lobby.OwnerId, channel, data);
        }

        public virtual void Tick() { }

        public virtual void UserConnectedEvent(long lobbyId, long userId) { }

        public virtual void UserDisconnectEvent(long lobbyId, long userId) { }

        public virtual void CleanupEvent() { }

        // The active node's shutdown is called upon closing the game
        public virtual void Shutdown() { }
    }
}
