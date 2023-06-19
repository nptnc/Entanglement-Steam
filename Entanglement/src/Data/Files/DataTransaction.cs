using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;

using Entanglement.Network;

namespace Entanglement.Data {
    public class DataTransaction {
        public static Queue<DataTransaction> outgoingTransactions = new Queue<DataTransaction>();
        public static Queue<DataTransaction> incomingTransactions = new Queue<DataTransaction>();

        public enum TransactionState {
            Begin,
            Waiting,
            Working,
            Done
        }

        public enum Direction { Incoming, Outgoing }

        public TransactionState state = TransactionState.Begin;

        public string filePath;
        public uint offset = 0;
        public long target = 0;
        public List<byte> bytes = new List<byte>();

        public const uint MAX_DATA_LENGTH = (uint)1e+9; // 1 GB is the limit
        public const uint FIXED_DATA_READ = 200000; // 0.2 MB/frame (ex 3.88 MB/s @ 144FPS)

        public DataTransaction(string filePath, long target, Direction direction = Direction.Outgoing) {
            this.filePath = filePath;

            if (direction == Direction.Outgoing)
            {
                if (!File.Exists(filePath)) throw new FileNotFoundException("Outgoing file wasn't found, this is usually an internal error!");

                bytes.AddRange(File.ReadAllBytes(filePath));

                if (bytes.Count > MAX_DATA_LENGTH) throw new ArgumentException("Can't send files greater than 1GB!");

                EntangleLogger.Log($"Added outgoing transaction for file {filePath}", ConsoleColor.DarkCyan);
                outgoingTransactions.Enqueue(this);
            }
            else {
                EntangleLogger.Log($"Added incoming transaction for file {filePath}", ConsoleColor.DarkCyan);
                incomingTransactions.Enqueue(this);
            }
        }

        public static void Process() {
            if (Node.activeNode == null) return;
            if (!DiscordIntegration.hasLobby) return;
            if (outgoingTransactions.Count() <= 0) return;

            DataTransaction outgoing = outgoingTransactions.Peek();

            if (outgoing != null) {
                if (outgoing.state == TransactionState.Begin) {
                    TransactionBeginMessageData data = new TransactionBeginMessageData() { transaction = outgoing };

                    NetworkMessage msg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.TransactionBegin, data);
                    Node.activeNode.SendMessage(outgoing.target, NetworkChannel.Transaction, msg.GetBytes());

                    outgoing.state = TransactionState.Waiting; // We wait for a confirm message to start sending data

                    //TEMP
                    outgoing.state = TransactionState.Working;
                }

                if (outgoing.state == TransactionState.Working) {
                    TransactionWorkMessageData data = new TransactionWorkMessageData();

                    data.read = outgoing.Read(out data.data);
                    data.name = outgoing.filePath;

                    NetworkMessage msg = NetworkMessage.CreateMessage((byte)BuiltInMessageType.TransactionWork, data);
                    Node.activeNode.SendMessage(outgoing.target, NetworkChannel.Transaction, msg.GetBytes());
                }

                if (outgoing.state == TransactionState.Done) {
                    // TODO: Send done message
                    _ = outgoingTransactions.Dequeue();
                }
            }
        }
        
        public uint Read(out byte[] readBytes) {
            readBytes = new byte[FIXED_DATA_READ];

            uint read = 0;
            for (read = 0; read < FIXED_DATA_READ; read++)
            {
                if (offset + read >= bytes.Count) {
                    state = TransactionState.Done;
                    break;
                }

                readBytes[read] = bytes[(int)(offset + read)];
            }

            offset += FIXED_DATA_READ;
            return read;
        }

        public void Write() {
        
        }
    }
}
