using System;

using UnityEngine;

using StressLevelZero.Interaction;

using Entanglement.Network;
using Entanglement.Representation;
using Entanglement.Extensions;

using HarmonyLib;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(PowerPuncher), "OnCollisionEnter")]
    public class GadgetPatches
    {
        public static void Prefix(PowerPuncher __instance, Collision collision) {
            if (!collision.rigidbody)
                return;

            Transform root = collision.gameObject.transform.root;
            string objName = root.name;
            if (!objName.Contains("PlayerRep"))
                return;
            string[] playerName = objName.Split('.');
            if (playerName.Length < 2)
                throw new IndexOutOfRangeException();
            long id = long.Parse(playerName[1]);

            ContactPoint contact = collision.GetContact(0);
            // TODO: Remove this hardcode, just tired of trying to make this goddamn thing work
            if (contact.thisCollider.name != "col_mainBody (1)")
                return;
            float dot = Vector3.Dot(collision.relativeVelocity.normalized, __instance.transform.TransformDirection(__instance.forward));
            dot = Mathf.Min(0f, dot);
            Vector3 force = collision.relativeVelocity * dot * __instance._triggerStartTime;
            force = Vector3.ClampMagnitude(force, 30f) * 7f;

            if (force == Vector3.zero)
                return;

            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.PowerPunch, new PowerPunchMessageData()
            {
                force = force,
                localPosition = PlayerRepresentation.syncedRoot.InverseTransformPosition(__instance.transform.position)
            });

            byte[] msgBytes = message.GetBytes();

            Node.activeNode.SendMessage(id, NetworkChannel.Attack, msgBytes);
        }
    }
}
