using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StressLevelZero;

namespace Entanglement.Network {
    public class LevelChangeMessageHandler : NetworkMessageHandler<LevelChangeMessageData> {
        public override byte? MessageIndex => BuiltInMessageType.LevelChange;

        public override NetworkMessage CreateMessage(LevelChangeMessageData data) {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { data.sceneIndex, Convert.ToByte(data.sceneReload), };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender) {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            byte index = message.messageData[0];

            bool reload = Convert.ToBoolean(message.messageData[1]);

            if (index == Client.instance.currentScene && !reload)
                return;

            StressLevelZero.Utilities.BoneworksSceneManager.LoadScene(index); // The scene loader only seems to work with an index, but at least it has less weight on the network
        }
    }

    public class LevelChangeMessageData : NetworkMessageData {
        public byte sceneIndex;
        public bool sceneReload;
    }
}
