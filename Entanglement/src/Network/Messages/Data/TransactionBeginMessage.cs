using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

using Entanglement.Data;

namespace Entanglement.Network
{
    // This is a WIP feature so it isn't gonna be registered for now
    public class TransactionBeginMessageHandler : NetworkMessageHandler<TransactionBeginMessageData> {
        public override NetworkMessage CreateMessage(TransactionBeginMessageData data) {
            NetworkMessage message = new NetworkMessage();

            List<byte> bytes = new List<byte>();

            bytes.Add((byte)data.transaction.filePath.Length);
            bytes.AddRange(Encoding.UTF8.GetBytes(data.transaction.filePath));
            bytes.AddRange(BitConverter.GetBytes(data.transaction.bytes.Count));

            message.messageData = bytes.ToArray();

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            TransactionBeginMessageData beginData = new TransactionBeginMessageData();

            byte fileNameLen = message.messageData[0];
            string fileName = Encoding.UTF8.GetString(message.messageData, 1, fileNameLen);
            uint fileLen = BitConverter.ToUInt32(message.messageData, fileNameLen + 1);

            if (fileLen >= DataTransaction.MAX_DATA_LENGTH) throw new ArgumentException("File transaction that was recieved has a file that is too big to stream!");

            new DataTransaction(fileName, 0, DataTransaction.Direction.Incoming);
        }
    }

    public class TransactionBeginMessageData : NetworkMessageData {
        public DataTransaction transaction;
    }
}
