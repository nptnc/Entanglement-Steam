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

using UnityEngine;
using Entanglement.src.Network.Steam;
using Steamworks.Data;
using Entanglement.src.Network;

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

        public byte currentScene = 0;

        // The value of the dict increases with time. When the server sends a heartbeat reset it to 0.
        // If its ever greater than a certain amount of seconds we should exit the server as the host has likely lost connection.
        public float hostHeartbeat;

        private Client() {
            
        }

        public void DiscordJoinLobby() {
            ObjectSync.OnCleanup();

            if (PlayerScripts.playerHealth)
                PlayerScripts.playerHealth.reloadLevelOnDeath = false;
        }

        public void DisconnectFromServer(bool notif = true) {
            if (notif)
                EntangleNotif.LeftServer();
            NetworkSender.clientSocket.Close();
            NetworkSender.clientSocket = null;

            CleanData();
        }

        public override void BroadcastMessage(SendType channel, byte[] data) { 
            BroadcastMessage(channel, data);
        }

        // Client.Shutdown is ran on closing the game
        public override void Shutdown() {
            DisconnectFromServer();
        }
    }
}
