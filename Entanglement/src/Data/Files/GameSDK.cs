using System.IO;

using Entanglement.Modularity;

using MelonLoader;

namespace Entanglement.Data
{
    public class GameSDK 
    {
        public static void LoadGameSDK()
        {
            // Extracts discord game sdk if its missing
            //string sdkPath = Directory.GetCurrentDirectory() + "/discord_game_sdk.dll";
            string sdkPath = PersistentData.GetPath("discord_game_sdk.dll");
            if (!File.Exists(sdkPath))
            {
                EntangleLogger.Log("Discord Game SDK was missing, autoextracting it!");
                File.WriteAllBytes(sdkPath, EmbeddedResource.LoadFromAssembly(EntanglementMod.entanglementAssembly, "Entanglement.resources.discord_game_sdk.dll"));
            }

            // SUPER SKETCHY but this is a fix for R2ModMan, instead of waiting for DllImport we just invoke it ourselves :)
            _ = DllTools.LoadLibrary(sdkPath);
        }
    }
}
