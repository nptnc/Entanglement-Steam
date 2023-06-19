using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;
using Entanglement.Representation;
using Entanglement.Compat.Playermodels;
using Entanglement.Objects;
using Entanglement.Compat;
using Entanglement.Data;
using Oculus.Platform.Models;
using Entanglement.src.Network;
using Steamworks.Data;
using Entanglement.src.Network.Steam;
using Steamworks;

namespace Entanglement.Network {
    public abstract class Node {
        // Reset per frame, but used in Entanglement -> Stats to see the network load
        public uint sentByteCount, recievedByteCount;

        public static Node activeNode;

        public static bool isServer => activeNode is Server;


        public void ConnectToServer(ulong steamId) {
            NetworkSender.clientSocket = SteamNetworkingSockets.ConnectRelay<ClientSocket>(steamId);
        }

        public void OnUserRegistered(PlayerId playerId) {
            CreatePlayerRep(playerId);
        }

        public void OnUserLeft(PlayerId playerId) {
            EntangleNotif.PlayerLeave($"{PlayerRepresentation.representations[playerId.LargeId].playerName}");

            PlayerRepresentation.representations[playerId.LargeId].DeleteRepresentations();
            PlayerRepresentation.representations.Remove(playerId.LargeId);
            NetworkSender.connections.Remove(playerId.LargeId);
            PlayerIds.Dispose(playerId);
        }

        public static void CreatePlayerRep(PlayerId playerId)
        {
            if (PlayerIds.GetPlayerFromLargeId(playerId.LargeId) != null)
                return;

            PlayerRepresentation.representations.Add(playerId.LargeId, new PlayerRepresentation(playerId.Username, playerId.LargeId));
            EntangleNotif.PlayerJoin($"{playerId.Username}");
        }

        public void CleanData() {
            NetworkSender.connections.Clear();
            PlayerIds.ClearAll();
            ObjectSync.OnCleanup();

            foreach (PlayerRepresentation playerRep in PlayerRepresentation.representations.Values)
                playerRep.DeleteRepresentations();

            PlayerRepresentation.representations.Clear();

            if (PlayerScripts.playerHealth)
                PlayerScripts.playerHealth.reloadLevelOnDeath = PlayerScripts.reloadLevelOnDeath;

            CleanupEvent();
        }

        public void SendMessage(ulong userId, SendType channel, byte[] data) {
            if (DiscordIntegration.hasServer) {
                NetworkSender.SendMessageToClient(userId, channel,  data);
                sentByteCount += (uint)data.Length;
            }
        }

        // Sends to owner if client
        // Sends to all if server
        public virtual void BroadcastMessage(SendType channel, byte[] data) { }

        // Forces send in every direction (for P2P-like messages, lowers latency but not good for certain things!)
        public void BroadcastMessageP2P(SendType channel, byte[] data) { 
            NetworkSender.BroadcastMessage(data, channel);
        }

        public virtual void Tick() {
            NetworkSender.clientSocket?.Receive(255);
            NetworkSender.serverSocket?.Receive(255);
            SteamClient.RunCallbacks();
        }

        public virtual void CleanupEvent() { }

        // The active node's shutdown is called upon closing the game
        public virtual void Shutdown() { }
    }
}
