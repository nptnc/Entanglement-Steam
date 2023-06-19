using System;
using System.Collections;

using Entanglement.Network;
using Entanglement.Data;
using Entanglement.Representation;

using UnityEngine;

using MelonLoader;

namespace Entanglement.Managers
{
    public static class PlayerDeathManager
    {
        public static bool hasDied = false;

        public static void Initialize()
        {
            Player_Health.add_OnPlayerDeath(new Action(DeathHook));
        }

        public static void DeathHook()
        {
            if (hasDied)
                return;

            hasDied = true;

            MelonCoroutines.Start(OnDeathFinished());
        }

        public static IEnumerator OnDeathFinished() {
            yield return new WaitForSeconds(1f);

#if DEBUG
            EntangleLogger.Log("Died! Sending Death event to all players!");
#endif

            PlayerEventMessageData data = new PlayerEventMessageData()
            {
                type = PlayerEventType.Death,
            };

            NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.PlayerEvent, data);
            Node.activeNode.BroadcastMessageP2P(NetworkChannel.Reliable, message.GetBytes());

#if DEBUG
            if (PlayerRepresentation.debugRepresentation != null)
                PlayerRepresentation.debugRepresentation.CreateRagdoll();
#endif

            // Wait for us to teleport
            while (!PlayerScripts.playerHealth.alive)
                yield return null;

            hasDied = false;
        }
    }
}
