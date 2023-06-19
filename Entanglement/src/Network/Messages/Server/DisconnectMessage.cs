using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

namespace Entanglement.Network
{
    public class DisconnectMessageHandler : NetworkMessageHandler<DisconnectMessageData> {
        public override byte? MessageIndex => BuiltInMessageType.Disconnect;

        public override NetworkMessage CreateMessage(DisconnectMessageData data) {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[1 + data.additionalReason.Length];

            message.messageData[0] = data.disconnectReason;
            int index = 1;
            foreach (byte b in Encoding.ASCII.GetBytes(data.additionalReason))
                message.messageData[index] = b;

            return message;
        }
        
        // Disconnect messages are only handled by clients
        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            Client.instance?.DisconnectFromServer(false);

            byte reason = message.messageData[0];
            string additionalReason = "";

            for (int b = 1; b < message.messageData.Length; b++)
                additionalReason += Encoding.ASCII.GetChars(message.messageData, b, 1);

            EntangleLogger.Log($"You were disconnected for reason {Enum.GetName(typeof(DisconnectReason), reason)}");

            EntangleNotif.PlayerDisconnect((DisconnectReason)reason);

            if (additionalReason != string.Empty)
                EntangleLogger.Log($"Additional reason: {additionalReason}");
        }
    }

    public enum DisconnectReason : byte {
        Unknown = 0,
        
        ServerFull   = 19,
        ServerClosed = 20,
        
        Kicked = 50,
        Banned = 51,
        
        OutdatedClient = 100,
        OutdatedServer = 101,
    }

    public class DisconnectMessageData : NetworkMessageData {
        public byte disconnectReason = (byte)DisconnectReason.Unknown;
        public string additionalReason = "";
    }
}
