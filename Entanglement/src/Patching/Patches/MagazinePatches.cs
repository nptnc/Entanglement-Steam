using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

using StressLevelZero.Interaction;
using StressLevelZero.Props.Weapons;

using HarmonyLib;

using Entanglement.Objects;
using Entanglement.Network;

using UnityEngine;

namespace Entanglement.Patching {
    public static class Magazine_Settings {
        public static bool InGun(this MagazinePlug plug) {
            if (!plug) return false;

            Socket lastSocket = plug._lastSocket;
            if (lastSocket && lastSocket.LockedPlug == plug) return true;
            return false;
        }

        public static bool EnteringOrInside(this MagazinePlug plug) => plug._isEnterTransition || plug.InGun();

        public static void ForceEject(this MagazinePlug plug) {
            try
            {
                plug.EjectPlug();
                plug.ClearFromSocket();
            } catch { }
            plug.magazine.gameObject.SetActive(true);
            plug.magazine.transform.parent = null;
            plug._isEnterTransition = false;
            plug._isExitTransition = false;
            plug._isExitComplete = true;
        }
    }

    [HarmonyPatch(typeof(MagazinePlug), "OnPlugExitComplete")]
    public static class PlugExitPatch {
        public static void Postfix(MagazinePlug __instance)
        {
            TransformSyncable magSync = TransformSyncable.cache.GetOrAdd(__instance.magazine.gameObject);
            if (!magSync || !magSync.IsOwner()) {
#if DEBUG
                EntangleLogger.Log("Not owner of mag or not synced!");
#endif
                return;
            }

            MagazineSocket magSocket = __instance._lastSocket.Cast<MagazineSocket>();
            Gun gun = magSocket.GetComponentInParent<Gun>();
            if (!gun) {
#if DEBUG
                EntangleLogger.Log("No gun found!");
#endif
                return;
            }

            TransformSyncable gunSync = TransformSyncable.cache.GetOrAdd(gun.gameObject);
            if (!gunSync || !gunSync.IsOwner()) {
#if DEBUG
                EntangleLogger.Log("Not owner of gun or not synced!");
#endif
                return;
            }

            MagazinePlugMessageData plugData = new MagazinePlugMessageData()
            {
                magId = magSync.objectId,
                gunId = gunSync.objectId,
                isInsert = false,
            };
            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.MagazinePlug, plugData);
            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, message.GetBytes());

#if DEBUG
            EntangleLogger.Log($"Magazine exited from {__instance.name}! Magazine id is {plugData.magId} and gun id is {plugData.gunId}.");
#endif
        }
    }

    [HarmonyPatch(typeof(MagazinePlug), "OnPlugInsertComplete")]
    public static class PlugEnterPatch {
        public static void Postfix(MagazinePlug __instance) {
            TransformSyncable magSync = TransformSyncable.cache.GetOrAdd(__instance.magazine.gameObject);
            if (!magSync || !magSync.IsOwner()) {
#if DEBUG
                EntangleLogger.Log("Not owner of mag or not synced!");
#endif
                return;
            }

            MagazineSocket magSocket = __instance._lastSocket.Cast<MagazineSocket>();
            Gun gun = magSocket.GetComponentInParent<Gun>();
            if (!gun) {
#if DEBUG
                EntangleLogger.Log("No gun found!");
#endif
                return; 
            }

            TransformSyncable gunSync = TransformSyncable.cache.GetOrAdd(gun.gameObject);
            if (!gunSync || !gunSync.IsOwner()) {
#if DEBUG
                EntangleLogger.Log("Not owner of gun or not synced!");
#endif
                return;
            }

            MagazinePlugMessageData plugData = new MagazinePlugMessageData() {
                magId = magSync.objectId,
                gunId = gunSync.objectId,
                isInsert = true
            };

            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.MagazinePlug, plugData);
            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, message.GetBytes());

#if DEBUG
            EntangleLogger.Log($"Magazine inserted into {__instance.name}! Magazine id is {plugData.magId} and gun id is {plugData.gunId}.");
#endif
        }
    }
}
