using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Network;

namespace Entanglement.Compat.CustomMaps {
    // We can't register this automatically! It reserves an index of 80 which is where first party compat messages start
    [Net.NoAutoRegister]
    public class LoadCustomMapMessageHandler : NetworkMessageHandler<LoadCustomMapMessageData> {
        public override byte? MessageIndex => CompatMessageType.CustomMap;

        public override NetworkMessage CreateMessage(LoadCustomMapMessageData data) {
            NetworkMessage message = new NetworkMessage();

            message.messageData = Encoding.UTF8.GetBytes(data.mapPath);

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            CustomMapsPatch.TryLoadMap(Encoding.UTF8.GetString(message.messageData));
        }
    }

    public class LoadCustomMapMessageData : NetworkMessageData {
        public string mapPath;
    }
}
