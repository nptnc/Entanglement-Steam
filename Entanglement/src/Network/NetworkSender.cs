using Entanglement.Network;
using Entanglement.src.Network.Messages.Redirect;
using Entanglement.src.Network.Steam;
using Oculus.Platform;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.src.Network
{
    public class NetworkSender
    {
        // Server has these
        public static Dictionary<ulong, Connection> connections = new Dictionary<ulong, Connection>();

        public static ServerSocket serverSocket;
        public static ClientSocket clientSocket;

        public static void BroadcastMessage(byte[] packet, SendType sendType = SendType.Reliable)
        {
            if (SteamIntegration.isHost)
            {
                foreach (var connection in connections)
                {
                    connection.Value.SendMessage(packet);
                }
            }
            else
            {
                clientSocket?.Connection.SendMessage(packet, sendType);
            }
        }

        public static void BroadcastMessageDirectRelay(byte[] packet, SendType sendType = SendType.Reliable)
        {
            if (SteamIntegration.isHost)
            {
                foreach (var connection in connections)
                {
                    connection.Value.SendMessage(packet);
                }
            }
            else
            {
                clientSocket?.Connection.SendMessage(packet, sendType);
            }
        }

        public static void BroadcastMessageExceptSelf(byte[] packet, SendType sendType = SendType.Reliable)
        {
            if (SteamIntegration.isHost)
            {
                foreach (var connection in connections)
                {
                    if (connection.Key != SteamClient.SteamId) {
                        connection.Value.SendMessage(packet);
                    }
                }
            }
            else
            {
                List<ulong> ulongs = new List<ulong>();

                foreach (PlayerId playerId in PlayerIds.playerIds) {
                    ulongs.Add(playerId.LargeId);
                }

                MessageRedirectData redirectData = new MessageRedirectData()
                {
                    toSendTo = ulongs,
                    data = packet
                };

                NetworkMessage idMessage = NetworkMessage.CreateMessage((byte) BuiltInMessageType.MessageRelay, redirectData);
                SendMessageToServer(SendType.Reliable, idMessage.GetBytes());
            }
        }

        public static void BroadcastMessageExcept(byte[] packet, SteamId steamId, SendType sendType = SendType.Reliable)
        {
            if (SteamIntegration.isHost)
            {
                foreach (var connection in connections)
                {
                    if (connection.Key != steamId)
                    {
                        connection.Value.SendMessage(packet, sendType);
                    }
                }
            }
        }

        public static void SendMessageToClient(ulong userId, SendType sendType, byte[] packet) {
            if (SteamIntegration.isHost)
            {
                if (connections.ContainsKey(userId))
                {
                    connections[userId].SendMessage(packet, sendType);
                }
            }
            else {

                List<ulong> ulongs = new List<ulong>
                {
                    userId
                };

                MessageRedirectData redirectData = new MessageRedirectData()
                {
                    toSendTo = ulongs,
                    data = packet
                };

                NetworkMessage idMessage = NetworkMessage.CreateMessage((byte) BuiltInMessageType.MessageRelay, redirectData);
                SendMessageToServer(SendType.Reliable, idMessage.GetBytes());
            }
        }

        public static void SendMessageToSelfClient(SendType sendType, byte[] packet)
        {
            if (SteamIntegration.isHost)
            {
                SendMessageToClient(SteamClient.SteamId, sendType, packet);
            }
        }

        public static void SendMessageToServer(SendType sendType, byte[] packet)
        {
            clientSocket?.Connection.SendMessage(packet, sendType);
        }
    }
}
