using System;

using HarmonyLib;

using UnhollowerBaseLib;

using StressLevelZero.SFX;
using StressLevelZero.Rig;
using StressLevelZero;
using StressLevelZero.Interaction;
using StressLevelZero.Combat;

using UnityEngine;

using Entanglement.Representation;
using Entanglement.Network;
using Entanglement.Data;
using Entanglement.Objects;
using Entanglement.Extensions;

using MelonLoader;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(Player_Health), "TAKEDAMAGE")]
    public static class PlayerDamagePatch
    {
        // Ignore damage that resets the death groan if currently dying.
        public static bool Prefix(Player_Health __instance, float damage, bool crit)
        {
            if (!__instance.alive)
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Hand), "AttachObject")]
    public static class GripAttachPatch
    {
        public static void Prefix(Hand __instance, GameObject objectToAttach) {
            ObjectSync.OnGripAttached(objectToAttach);
        }
    }

    [HarmonyPatch(typeof(Hand), "DetachObject")]
    public static class GripDetachPatch
    {
        public static void Prefix(Hand __instance, GameObject objectToDetach, bool restoreOriginalParent = true) {
            try { ObjectSync.OnGripDetached(__instance);
            }
            catch {
#if DEBUG
                EntangleLogger.Warn("Caught exception while detaching grip!");  
#endif   
            }
        }
    }

    [HarmonyPatch(typeof(HandSFX), "PunchAttack")]
    public static class PunchPatch
    {
        public static void Postfix(HandSFX __instance, Collision c, float impulse, float relVelSqr)
        {
            Transform root = c.gameObject.transform.root;
            string objName = root.name;
            if (!objName.Contains("PlayerRep"))
                return;
            string[] playerName = objName.Split('.');
            if (playerName.Length < 2)
                throw new IndexOutOfRangeException();
            long id = long.Parse(playerName[1]);
            NetworkMessage message = NetworkMessage.CreateMessage((byte)BuiltInMessageType.PlayerAttack, new PlayerAttackMessageData() { 
                attackType = AttackType.Blunt,
                attackDamage = (ushort)(impulse / 5f),
            });

            byte[] msgBytes = message.GetBytes();

            Node.activeNode.SendMessage(id, NetworkChannel.Attack, msgBytes);
        }
    }

    [HarmonyPatch(typeof(SkeletonHand), "SetHandPose")]
    public static class PosePatch
    {
        public static int prevLeftPose = 0;
        public static int prevRightPose = 0;

        public static void Postfix(SkeletonHand __instance, string handPoseName)
        {
            if (!__instance.GetCharacterAnimationManager()) return;

            int poseIndex = PlayerScripts.playerHandPoses.IndexOf(handPoseName);
            if (poseIndex <= -1)
                return;

            Handedness hand = __instance.handedness;

            // Skip same pose
            bool validMessage = true;
            if (hand == Handedness.LEFT) {
                validMessage = prevLeftPose != poseIndex;
                prevLeftPose = poseIndex;
            }
            else {
                validMessage = prevRightPose != poseIndex;
                prevRightPose = poseIndex;
            }

            if (!validMessage) return;

            HandPoseChangeMessageData poseData = new HandPoseChangeMessageData();

            poseData.userId = DiscordIntegration.currentUser.Id;
            poseData.hand = hand;
            poseData.poseIndex = (ushort)poseIndex;

            NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.HandPose, poseData);

            byte[] msgBytes = message.GetBytes();

            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);

#if DEBUG
            if (PlayerRepresentation.debugRepresentation != null) PlayerRepresentation.debugRepresentation.UpdatePose(poseData.hand, poseData.poseIndex);
#endif
        }
    }

    [HarmonyPatch(typeof(SkeletonHand), "SetCylinderRadius")]
    public static class GripRadiusPatch
    {
        public static float prevLeftRadius = 0f;
        public static float prevRightRadius = 0f;

        public static void Postfix(SkeletonHand __instance, float radius)
        {
            if (!__instance.GetCharacterAnimationManager()) return;

            Handedness hand = __instance.handedness;

            switch (hand) {
                case Handedness.LEFT:
                    if (radius == prevLeftRadius)
                        return;
                    prevLeftRadius = radius;
                    break;
                case Handedness.RIGHT:
                    if (radius == prevRightRadius)
                        return;
                    prevRightRadius = radius;
                    break;
            }

            GripRadiusMessageData radiusData = new GripRadiusMessageData();

            radiusData.userId = DiscordIntegration.currentUser.Id;
            radiusData.hand = hand;
            radiusData.radius = radius;

            NetworkMessage message = NetworkMessage.CreateMessage(BuiltInMessageType.GripRadius, radiusData);

            byte[] msgBytes = message.GetBytes();

            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, msgBytes);

#if DEBUG
            if (PlayerRepresentation.debugRepresentation != null) PlayerRepresentation.debugRepresentation.UpdatePoseRadius(radiusData.hand, radiusData.radius);
#endif
        }
    }
}
