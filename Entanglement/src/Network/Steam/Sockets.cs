using Entanglement.Network;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.src.Network.Steam
{
    public class ServerSocket : SocketManager
    {
        public Dictionary<ulong, Connection> steamIdConnections = new Dictionary<ulong, Connection>();
        
        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            base.OnConnecting(connection, info);
        }

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            base.OnConnected(connection, info);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            base.OnDisconnected(connection, info);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            if (!steamIdConnections.ContainsKey(identity.steamid)) {
                steamIdConnections.Add(identity.steamid,connection);
            }
            
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
        }
    }

    public class ClientSocket : ConnectionManager {

        public override void OnConnecting(ConnectionInfo data)
        {

        }

        public override void OnConnected(ConnectionInfo data)
        {
            
        }

        public override void OnDisconnected(ConnectionInfo data)
        {
            
        }

        public override void OnMessage(IntPtr olddata, int size, Int64 messageNum, Int64 recvTime, int channel)
        {
            var data = new byte[size];
            Marshal.Copy(olddata, data, 0, size);

            if (data.Length <= 0)
                throw new Exception("Data was invalid!");

            NetworkMessage message = new NetworkMessage();

            message.messageType = data[0];
            message.messageData = new byte[data.Length - sizeof(byte)];

            for (int b = sizeof(byte); b < data.Length; b++)
                message.messageData[b - sizeof(byte)] = data[b];

            NetworkMessage.ReadMessage(message, 0);
        }
    }
}
