using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Entanglement.Extensions;

using MelonLoader;
using Steamworks;
using Entanglement.src.Network;
using Il2CppSystem;
using Steamworks.Data;

namespace Entanglement.Network
{
    public class RegistrationMessageHandler : NetworkMessageHandler<RegistrationMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.Registration;

        public override NetworkMessage CreateMessage(RegistrationMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.WriteByte(data.byteId);
            byteBuffer.WriteULong(data.userId);
            byteBuffer.WriteString(data.username);

            message.messageData = byteBuffer.GetBytes();

            return message;
        }

        public override void HandleMessage(NetworkMessage message, ulong sender, bool isServerHandled)
        {
            if (message.messageData.Length <= 0)
                throw new System.IndexOutOfRangeException();

            if (isServerHandled)
            {
                EntangleLogger.Log("Received registration",System.ConsoleColor.Magenta);
                
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(SendType.Reliable, msgBytes, sender);
                return;
            }
            else
                EntangleLogger.Log("Received registration",System.ConsoleColor.Blue);

            ByteBuffer byteBuffer = new ByteBuffer(message.messageData);
            byte byteId = byteBuffer.ReadByte();
            ulong longId = byteBuffer.ReadULong();
            string username = byteBuffer.ReadString();

            PlayerId playerId = PlayerIds.Add(longId, byteId, username);

            if (playerId != null) {
                if (longId == SteamClient.SteamId)
                {
                    SteamIntegration.localId = playerId;
                }
                else
                {
                    Node.activeNode.OnUserRegistered(playerId);
                }
            }
        }
    }

    public class RegistrationMessageData : NetworkMessageData
    {
        public ulong userId;
        public byte byteId;
        public string username;
    }
}
