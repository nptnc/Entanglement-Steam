using Entanglement.Representation;
using Entanglement.Extensions;
using Entanglement.Network;

using UnityEngine;
using UnityEngine.Rendering;

using ModThatIsNotMod;

using System.IO;

using MelonLoader;

namespace Entanglement.Compat.Playermodels
{
    public static class PlayerSkinLoader
    {
        // Removes an existing PlayerModel and reverts back to ford.
        public static void ClearPlayermodel(PlayerRepresentation rep)
        {
            // Make sure the rep has been created
            if (!rep.repFord)
                return;

            UnloadPlayermodel(rep);

            rep.repAnimationManager.animator = rep.repAnimator;
            rep.activeAnimator = rep.repAnimator;
            rep.repBody.ArtToBlender.bones = rep.repBody.ArtToBlender.bones.FillBones(rep.repAnimator.transform, rep.repAnimator);
            FetchFingerTransforms(rep);

            if (rep.currentSkinBundle)
                rep.currentSkinBundle.Unload(false);

            rep.currentSkinObject = null;
            rep.currentSkinPath = null;
            rep.currentSkinBundle = null;

            rep.repGeo.gameObject.SetActive(true);
            rep.repSHJnt.gameObject.SetActive(true);

            rep.repBody.OnStart();
            rep.isCustomSkinned = false;
        }

        // Applies a PlayerModel at the specified path relative to the game folder.
        // TODO: Load Async if its not stripped
        public static void ApplyPlayermodel(PlayerRepresentation rep, string path)
        {
            // Clear Model
            ClearPlayermodel(rep);
            // Save skin
            rep.currentSkinPath = path;

            AssetBundle newLoadedBundle = AssetBundleUtilities.TryLoadBundle(path, PlayermodelsPatch.playerModelsPath);
            if (newLoadedBundle == null) {
                EntangleLogger.Warn($"PlayerModel failed to load for player {rep.playerName}.\nIf the PlayerModel was sent over discord, spaces in the name may have been replaced by underscores.");
                return;
            }

            rep.currentSkinBundle = newLoadedBundle;
            rep.isCustomSkinned = true;

            // Make sure the rep has been created
            if (!rep.repFord)
                return;

            GameObject loadedModel = rep.currentSkinBundle.LoadAsset<GameObject>("Assets/PlayerModels/PlayerModel.prefab");
            if (loadedModel == null)
                return;

            rep.currentSkinObject = GameObject.Instantiate(loadedModel);
            CustomItems.FixObjectShaders(rep.currentSkinObject);
            rep.currentSkinObject.name = rep.repFord.name;
            rep.skinAnimator = rep.currentSkinObject.GetComponent<Animator>();
            rep.skinAnimator.runtimeAnimatorController = rep.repAnimator.runtimeAnimatorController;
            rep.skinAnimator.enabled = false;

            rep.repAnimationManager.animator = rep.skinAnimator;
            rep.activeAnimator = rep.skinAnimator;
            rep.repBody.ArtToBlender.bones = rep.repBody.ArtToBlender.bones.FillBones(rep.skinAnimator.transform, rep.skinAnimator);
            FetchFingerTransforms(rep);

            rep.repBody.OnStart();

            rep.repGeo.gameObject.SetActive(false);
            rep.repSHJnt.gameObject.SetActive(false);

            // Get Renderers
            Renderer[] renderers = rep.currentSkinObject.GetComponentsInChildren<Renderer>();
            bool hasHead = false;
            // Find Head
            foreach (Renderer rend in renderers)
                if (rend.name.ToLower().Contains("head")) {
                    if (rend.material && !rend.material.shader.name.Contains("ShadowOnly") && rend.shadowCastingMode != ShadowCastingMode.ShadowsOnly) {
                        hasHead = true;
                        break;
                    }
                }
            // Replace ShadowCasts
            if (!hasHead) {
                foreach (Renderer rend in renderers) {
                    if (rend.material && rend.material.shader.name.Contains("ShadowOnly"))
                        rend.material = rep.repHologram;
                    else if (rend.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                        rend.shadowCastingMode = ShadowCastingMode.On;
                }
            }
        }

        // Unloads currently loaded bundles
        public static void UnloadPlayermodel(PlayerRepresentation rep)
        {
            if (rep.currentSkinObject) {
                GameObject.Destroy(rep.currentSkinObject);
            }
            if (rep.currentSkinBundle) {
                rep.currentSkinBundle.Unload(false);
                rep.currentSkinBundle = null;
            }
        }

        // Regular AutoFillTransforms is hardcoded to use the Animator on the same GameObject, so this is taken from CPP2IL.
        public static void FetchFingerTransforms(PlayerRepresentation rep)
        {
            //Left Hand
            Transform leftBone = rep.activeAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform leftHand = TransformDeepChildExtension.FindDeepChild(leftBone, "l_Hand_2SHJnt");
            Transform leftHandle = TransformDeepChildExtension.FindDeepChild(leftHand, "l_GripPoint_AuxSHJnt");
            Transform leftOpen = TransformDeepChildExtension.FindDeepChild(leftHand.parent, "l_Hand_2SHJnt_open");
            rep.repAnimationManager.leftHandTransform = leftHand;
            rep.repAnimationManager.leftHandleTransform = leftHandle;
            rep.repAnimationManager.leftOpenHandTransform = leftOpen;
            rep.repAnimationManager.leftClosedHandTransform = leftHand;
            //Right Hand
            Transform rightBone = rep.activeAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform rightHand = TransformDeepChildExtension.FindDeepChild(rightBone, "r_Hand_2SHJnt");
            Transform rightHandle = TransformDeepChildExtension.FindDeepChild(rightHand, "r_GripPoint_AuxSHJnt");
            Transform rightOpen = TransformDeepChildExtension.FindDeepChild(rightHand.parent, "r_Hand_2SHJnt_open");
            rep.repAnimationManager.rightHandTransform = rightHand;
            rep.repAnimationManager.rightHandleTransform = rightHandle;
            rep.repAnimationManager.rightOpenHandTransform = rightOpen;
            rep.repAnimationManager.rightClosedHandTransform = rightHand;
            //Calculate
            rep.repAnimationManager.CalculateHandPoseRefs();
        }
    }
}
