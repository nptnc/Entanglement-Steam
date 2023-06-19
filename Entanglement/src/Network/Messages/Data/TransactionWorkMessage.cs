using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

namespace Entanglement.Network {
    // This is a WIP feature so it isn't gonna be registered for now
    public class TransactionWorkMessageHandler : NetworkMessageHandler<TransactionWorkMessageData> {
        public override NetworkMessage CreateMessage(TransactionWorkMessageData data) {
            NetworkMessage message = new NetworkMessage();

            List<byte> bytes = new List<byte>();

            bytes.Add((byte)data.name.Length);
            bytes.AddRange(Encoding.UTF8.GetBytes(data.name));
            bytes.AddRange(BitConverter.GetBytes((uint)data.data.Length));
            bytes.AddRange(data.data);

            message.messageData = bytes.ToArray();

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            TransactionWorkMessageData data = new TransactionWorkMessageData();

            byte nameLen = message.messageData[0];
            EntangleLogger.Log(nameLen);

            string fileName = Encoding.UTF8.GetString(message.messageData, 1, nameLen);
            EntangleLogger.Log(fileName);

            uint dataLen = BitConverter.ToUInt32(message.messageData, nameLen + 1);
            EntangleLogger.Log(dataLen);

            uint dataOffset = (uint)nameLen + 1 + sizeof(uint);
            byte[] recvData = new byte[dataLen];
            for (int r = 0; r < dataLen; r++)
                recvData[r] = message.messageData[r + dataOffset];
        }
    }

    public class TransactionWorkMessageData : NetworkMessageData {
        public uint read;
        public byte[] data;
        public string name;
    }
}
