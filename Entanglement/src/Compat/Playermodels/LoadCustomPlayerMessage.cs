using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Entanglement.Representation;
using Entanglement.Network;

namespace Entanglement.Compat.Playermodels
{
    [Net.NoAutoRegister]
    public class LoadCustomPlayerMessageHandler : NetworkMessageHandler<LoadCustomPlayerMessageData>
    {
        public override byte? MessageIndex => CompatMessageType.PlayerModel;

        public override NetworkMessage CreateMessage(LoadCustomPlayerMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            byte[] utf8 = Encoding.UTF8.GetBytes(data.modelPath);
            message.messageData = new byte[sizeof(byte) + sizeof(long) + utf8.Length];

            int index = 0;
            byte[] userId = BitConverter.GetBytes(data.userId);
            for (int i = 0; i < sizeof(long); i++)
                message.messageData[index++] = userId[i];

            message.messageData[index++] = Convert.ToByte(data.requestCallback);

            for (int i = 0; i < utf8.Length; i++)
                message.messageData[index++] = utf8[i];

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            int index = 0;
            long userId = BitConverter.ToInt64(message.messageData, index);
            index += sizeof(long);

            bool requestCallback = Convert.ToBoolean(message.messageData[index]);
            index += sizeof(byte);

            if (requestCallback) {
                // Send PlayerModel
                if (PlayermodelsPatch.lastLoadedPath != null)
                {
                    string path = PlayermodelsPatch.lastLoadedPath;
                    LoadCustomPlayerMessageData msgData = new LoadCustomPlayerMessageData();
                    msgData.userId = DiscordIntegration.currentUser.Id;
                    msgData.modelPath = Path.GetFileName(path);
                    Node.activeNode.SendMessage(userId, NetworkChannel.Reliable, NetworkMessage.CreateMessage(CompatMessageType.PlayerModel, msgData).GetBytes());
                }
            }

            if (PlayerRepresentation.representations.ContainsKey(userId))
            {
                PlayerRepresentation rep = PlayerRepresentation.representations[userId];

                int remaining = message.messageData.Length - index;
                byte[] stringBytes = new byte[remaining];
                for (int i = 0; i < remaining; i++)
                    stringBytes[i] = message.messageData[index++];

                string modelPath = Encoding.UTF8.GetString(stringBytes);

                if (string.IsNullOrWhiteSpace(modelPath))
                    PlayerSkinLoader.ClearPlayermodel(rep);
                else
                    PlayerSkinLoader.ApplyPlayermodel(rep, Path.Combine(PlayermodelsPatch.playerModelsPath, modelPath));
            }

            if (Server.instance != null)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, userId);
            }
        }
    }

    public class LoadCustomPlayerMessageData : NetworkMessageData
    {
        public long userId;
        public bool requestCallback = false;
        public string modelPath;
    }
}
