

namespace Entanglement.Network
{
    /// <summary>
    /// Server HeartBeat to make sure a user is still in the server.
    /// Sent from Server to Clients so they know the server still exists.
    /// Sent from Clients to Server so the Server knows they are still connected.
    /// Should be sent maybe every 5 seconds, if the heartbeat has not been received in over 30-40 seconds terminate server/user.
    /// </summary>
    public class HeartbeatMessageHandler : NetworkMessageHandler<EmptyMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.Heartbeat;

        public override NetworkMessage CreateMessage(EmptyMessageData data) => new NetworkMessage();

        // Can be handled by either clients or the server, but are not sent back.
        public override void HandleMessage(NetworkMessage message, long sender) {
            if (Node.activeNode is Server server) {
                // Reset the heart beat of this user.
                if (server.userBeats.ContainsKey(sender))
                    server.userBeats[sender] = 0f;
            } 
            else if (Node.activeNode is Client client) {
                // A check incase for some reason another client is sending a heartbeat to us.
                if (client.hostUser.Id == sender)
                    client.hostHeartbeat = 0f;
            }
        }
    }
}
