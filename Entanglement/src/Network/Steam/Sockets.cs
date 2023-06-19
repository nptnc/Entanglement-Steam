﻿using Entanglement.Network;
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
        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            
        }

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            if (!NetworkSender.connections.ContainsKey(identity.steamid)) {
                NetworkSender.connections.Add(identity.steamid, connection);
            }
            
            var messageData = new byte[size];
            Marshal.Copy(data, messageData, 0, size);

            if (messageData.Length <= 0)
                throw new Exception("Data was invalid!");

            NetworkMessage message = new NetworkMessage();

            message.messageType = messageData[0];
            message.messageData = new byte[messageData.Length - sizeof(byte)];

            for (int b = sizeof(byte); b < messageData.Length; b++)
                message.messageData[b - sizeof(byte)] = messageData[b];

            NetworkMessage.ReadMessage(message, identity.steamid, true);
        }
    }

    public class ClientSocket : ConnectionManager {

        public override void OnConnecting(ConnectionInfo data)
        {

        }

        public override void OnConnected(ConnectionInfo data)
        {
            ConnectionMessageData connectionData = new ConnectionMessageData();
            connectionData.packedVersion = BitConverter.ToUInt16(new byte[] { EntanglementVersion.versionMajor, EntanglementVersion.versionMinor }, 0);
            connectionData.username = SteamClient.Name;
            connectionData.longId = SteamClient.SteamId;

            NetworkMessage conMsg = NetworkMessage.CreateMessage((byte) BuiltInMessageType.Connection, connectionData);
            NetworkSender.SendMessageToServer(SendType.Reliable, conMsg.GetBytes());
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

            NetworkMessage.ReadMessage(message, 0, false);
        }
    }
}
