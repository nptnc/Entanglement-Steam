using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Entanglement.Patching;
using Steamworks.Data;

namespace Entanglement.Network
{
    [Net.HandleOnLoaded]
    public class ZombieStartMessageHandler : NetworkMessageHandler<EmptyMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.ZombieStart;

        public override NetworkMessage CreateMessage(EmptyMessageData data) => new NetworkMessage();

        public override void HandleMessage(NetworkMessage message, ulong sender, bool isServerHandled) {

            if (isServerHandled)
            {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(SendType.Reliable, msgBytes, sender);
                return;
            }

            Zombie_GameControl instance = Zombie_GameControl.instance;
            if (instance) {
                ZombieMode_Settings.m_invalidSettings = true;
                instance.StartSelectedMode();
                ZombieMode_Settings.m_invalidSettings = false;
                instance.uiGameDisplayPageObj.SetActive(true);
                instance.uiSelectPageObj.SetActive(false);
                Transform parent = instance.uiGameDisplayPageObj.transform.parent;
                parent.Find("LoadoutPage").gameObject.SetActive(false);
                parent.Find("CustomModePage").gameObject.SetActive(false);
                parent.Find("DifficultySelectPage").gameObject.SetActive(false);
            }
        }
    }
}
