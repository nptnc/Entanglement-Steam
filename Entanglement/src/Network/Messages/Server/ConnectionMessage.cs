using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;

using Entanglement.Data;
using Entanglement.src.Network;
using Steamworks.Data;
using ConsoleColor = Il2CppSystem.ConsoleColor;

namespace Entanglement.Network {
    public class ConnectionMessageHandler : NetworkMessageHandler<ConnectionMessageData> {
        public override byte? MessageIndex => BuiltInMessageType.Connection;

        public override NetworkMessage CreateMessage(ConnectionMessageData data) {
            NetworkMessage message = new NetworkMessage();

            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.WriteUShort(data.packedVersion);
            byteBuffer.WriteULong(data.longId);
            byteBuffer.WriteString(data.username);

            message.messageData = byteBuffer.GetBytes();
            return message;
        }
        
        // Connection messages are only handled by the server
        public override void HandleMessage(NetworkMessage message, ulong sender, bool isServerHandled) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            if (!isServerHandled) {
                return;
            }
            
            EntangleLogger.Log("Received connection message",System.ConsoleColor.Blue);

            byte clientVersionMajor = message.messageData[0];
            byte clientVersionMinor = message.messageData[1];
            ByteBuffer byteBuffer = new ByteBuffer(message.messageData);
            byteBuffer.ReadUShort();
            ulong longId = byteBuffer.ReadULong();
            string username = byteBuffer.ReadString();

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

            if (BanList.bannedUsers.Any(tuple => tuple.Item1 == sender))
                disconnectData.disconnectReason = (byte)DisconnectReason.Banned;

            if (disconnectData.disconnectReason != (byte)DisconnectReason.Unknown) {
                EntangleLogger.Log($"Disconnecting sender for reason {Enum.GetName(typeof(DisconnectReason), disconnectData.disconnectReason)}...");
                NetworkMessage disconnectMsg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.Disconnect, disconnectData);
                Server.instance?.SendMessage(sender, SendType.Reliable, disconnectMsg.GetBytes());
                return;
            }

            EntangleLogger.Log($"Client got through all checks!");

            // They made it through the checks
            

            EntangleLogger.Log($"Sent level change message to {sender}!");
            EntangleLogger.Log($"Is host: {SteamIntegration.isHost}");

            foreach (PlayerId playerId in PlayerIds.playerIds)
            {
                RegistrationMessageData addMessageData = new RegistrationMessageData()
                {
                    userId = playerId.LargeId,
                    byteId = playerId.SmallId,
                    username = playerId.Username
                };
                NetworkMessage addMessage = NetworkMessage.CreateMessage((byte) BuiltInMessageType.Registration, addMessageData);
                Server.instance?.SendMessage(sender, SendType.Reliable, addMessage.GetBytes());
            }

            RegistrationMessageData idMessageData = new RegistrationMessageData()
            {
                userId = sender,
                byteId = PlayerIds.TryGetAvailableSmallId(),
                username = username
            };

            EntangleLogger.Log($"Sent registration message to {sender}!");

            NetworkMessage idMessage = NetworkMessage.CreateMessage((byte) BuiltInMessageType.Registration, idMessageData);
            NetworkSender.BroadcastMessage(idMessage.GetBytes(), SendType.Reliable);

            LevelChangeMessageData levelChangeData = new LevelChangeMessageData() { sceneIndex = (byte) StressLevelZero.Utilities.BoneworksSceneManager.currentSceneIndex };
            NetworkMessage levelChangeMessage = NetworkMessage.CreateMessage(BuiltInMessageType.LevelChange, levelChangeData);
            Node.activeNode.SendMessage(sender, SendType.Reliable, levelChangeMessage.GetBytes());
        }
    }

    public class ConnectionMessageData : NetworkMessageData {
        public ushort packedVersion;
        public string username;
        public ulong longId;
    }
}
