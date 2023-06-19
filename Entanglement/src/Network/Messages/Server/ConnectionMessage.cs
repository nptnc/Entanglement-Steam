using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;

using Entanglement.Data;

namespace Entanglement.Network {
    public class ConnectionMessageHandler : NetworkMessageHandler<ConnectionMessageData> {
        public override byte? MessageIndex => BuiltInMessageType.Connection;

        public override NetworkMessage CreateMessage(ConnectionMessageData data) {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[sizeof(ushort) + 16];

            int index = 0;

            foreach (byte b in BitConverter.GetBytes(data.packedVersion))
                message.messageData[index++] = b;

            return message;
        }
        
        // Connection messages are only handled by the server
        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            byte clientVersionMajor = message.messageData[0];
            byte clientVersionMinor = message.messageData[1];

            bool isSameVersion = clientVersionMajor == EntanglementVersion.versionMajor && clientVersionMinor == EntanglementVersion.versionMinor;

            EntangleLogger.Log($"A client connected with version '{clientVersionMajor}.{clientVersionMinor}.*'...");

            DisconnectMessageData disconnectData = new DisconnectMessageData();

            if (!isSameVersion) {
                if (clientVersionMajor < EntanglementVersion.minVersionMajorSupported || clientVersionMinor < EntanglementVersion.minVersionMinorSupported) {
                    EntangleLogger.Log($"A client was removed for having an outdated client!");
                    disconnectData.disconnectReason = (byte)DisconnectReason.OutdatedClient;
                }

                if (clientVersionMajor > EntanglementVersion.versionMajor || clientVersionMinor > EntanglementVersion.versionMinor) {
                    EntangleLogger.Log($"A client was removed for having too new of a client! Please update your mod!");
                    disconnectData.disconnectReason = (byte)DisconnectReason.OutdatedServer;
                }
            }
            else {
                if (Server.instance.connectedUsers.Count >= Server.maxPlayers) {
                    EntangleLogger.Log("A client was removed since the server is full!");
                    disconnectData.disconnectReason = (byte)DisconnectReason.ServerFull;
                }
            }

            if (BanList.bannedUsers.Any(tuple => tuple.Item1 == sender))
                disconnectData.disconnectReason = (byte)DisconnectReason.Banned;

            if (disconnectData.disconnectReason != (byte)DisconnectReason.Unknown) {
                EntangleLogger.Log($"Disconnecting sender for reason {Enum.GetName(typeof(DisconnectReason), disconnectData.disconnectReason)}...");
                NetworkMessage disconnectMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Disconnect, disconnectData);
                Server.instance?.SendMessage(sender, NetworkChannel.Reliable, disconnectMsg.GetBytes());
            }
        }
    }

    public class ConnectionMessageData : NetworkMessageData {
        public ushort packedVersion;
    }
}
