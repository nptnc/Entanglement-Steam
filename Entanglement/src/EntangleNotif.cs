using System;

using ModThatIsNotMod;

using Entanglement.Network;

namespace Entanglement
{
    public static class EntangleNotif {
        public static void PlayerJoin(string username) {
            Notifications.SendNotification($"{username} has joined the server!", 4f);
        }

        public static void PlayerLeave(string username) {
            Notifications.SendNotification($"{username} has left the server!", 4f);
        }

        public static void PlayerDisconnect(DisconnectReason reason) {
            Notifications.SendNotification($"You were disconnected for reason {reason}.", 4f);
        }

        public static void JoinServer(string username) {
            Notifications.SendNotification($"Joined {username}'s server!", 4f);
        }

        public static void LeftServer() {
            Notifications.SendNotification("You left the server.", 4f);
        }

        public static void InvalidDiscord() {
            Notifications.SendNotification("Failed to initialize the Discord Client! Continuing without Entanglement!\nDid you make sure to start the game with Discord open and have Discord installed?\nIf both are true, try to reinstall or sign out of Discord.", 4f);
        }
    }
}
