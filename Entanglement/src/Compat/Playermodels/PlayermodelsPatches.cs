using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using HarmonyLib;

using Entanglement.Representation;
using Entanglement.Patching;
using Entanglement.Network;
using Entanglement.Extensions;

using UnityEngine;

using MelonLoader;

namespace Entanglement.Compat.Playermodels
{
    [OptionalAssemblyTarget("PlayerModels")]
    public class PlayermodelsPatch : OptionalAssemblyPatch {

        //Rtas: Path.Combine() doesn't seem to work correctly in this scenario - this commented out version
        //of the function seems to only return "\PlayerModels\", at-least on my build environment
        //Path.Combine(MelonUtils.UserDataDirectory, @"\PlayerModels\");
        
        //I've opted to substitute it with String.Join instead
        public static string playerModelsPath => String.Join("", MelonUtils.UserDataDirectory, @"\PlayerModels\");

        public static Type skinLoadingType;
        public static FieldInfo currentBundleInfo;
        public static string lastLoadedPath = null;

        public override void DoPatches(Assembly target) {
            skinLoadingType = target.GetType("PlayerModels.SkinLoading");

            currentBundleInfo = skinLoadingType.GetField("currentLoadedBundle", BindingFlags.Public | BindingFlags.Static);

            MethodBase applyModelMethod = skinLoadingType.GetMethod("ApplyPlayerModel", BindingFlags.Public | BindingFlags.Static);
            MethodInfo applyPostfixMethod = typeof(PlayermodelsPatch).GetMethod("ApplyPlayerModel_Postfix", BindingFlags.Public | BindingFlags.Static);
            MethodInfo applyPrefixMethod = typeof(PlayermodelsPatch).GetMethod("ApplyPlayerModel_Prefix", BindingFlags.Public | BindingFlags.Static);

            MethodBase clearModelMethod = skinLoadingType.GetMethod("ClearPlayerModel", BindingFlags.Public | BindingFlags.Static);
            MethodInfo clearPrefixMethod = typeof(PlayermodelsPatch).GetMethod("ClearPlayerModel_Prefix", BindingFlags.Public | BindingFlags.Static);

            Patcher.Patch(applyModelMethod, new HarmonyMethod(applyPrefixMethod), new HarmonyMethod(applyPostfixMethod));
            Patcher.Patch(clearModelMethod, new HarmonyMethod(clearPrefixMethod));

            NetworkMessage.RegisterHandler<LoadCustomPlayerMessageHandler>();
        }

        public static void ClearPlayerModel_Prefix() {
            //Unload Without Destroying Assets
            object obj = currentBundleInfo.GetValue(null);
            if (obj != null)
                ((AssetBundle)obj)?.Unload(false);
            //Broadcast
            BroadcastPlayermodel(" ");
            //Remove
            lastLoadedPath = null;
#if DEBUG
            if (PlayerRepresentation.debugRepresentation != null) PlayerSkinLoader.ClearPlayermodel(PlayerRepresentation.debugRepresentation);
#endif
        }

        public static void ApplyPlayerModel_Prefix(string path) => AssetBundleUtilities.TryUnloadBundle(path, false);

        public static void ApplyPlayerModel_Postfix(string path) {
            //Rename to Path Name
            lastLoadedPath = path;
            string fileName = Path.GetFileName(path).ToLower();
            object obj = currentBundleInfo.GetValue(null);
            if (obj != null) {
                AssetBundle loadedBundle = (AssetBundle)obj;
                loadedBundle.name = fileName;
            }
            //Broadcast
            BroadcastPlayermodel(path);
#if DEBUG
            if (PlayerRepresentation.debugRepresentation != null) PlayerSkinLoader.ApplyPlayermodel(PlayerRepresentation.debugRepresentation, Path.Combine(playerModelsPath, path));
#endif
        }

        public static void BroadcastPlayermodel(string path) {
            LoadCustomPlayerMessageData msgData = new LoadCustomPlayerMessageData();
            msgData.userId = DiscordIntegration.currentUser.Id;
            msgData.modelPath = Path.GetFileName(path);

            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, NetworkMessage.CreateMessage(CompatMessageType.PlayerModel, msgData).GetBytes());
        }
    }
}
