using System.IO;

using Entanglement.Modularity;

using MelonLoader;

namespace Entanglement.Data
{
    public class SteamAPILoader 
    {
        public static void LoadSteamAPI()
        {
            // Extracts steam api if its missing
        
            string sdkPath = PersistentData.GetPath("steam_api64.dll");
            if (!File.Exists(sdkPath))
            {
                MelonLogger.Log("Steam API was missing, autoextracting it!");
                File.WriteAllBytes(sdkPath, EmbeddedResource.LoadFromAssembly(EntanglementMod.entanglementAssembly, "Entanglement.resources.steam_api64.dll"));
            }

            // SUPER SKETCHY but this is a fix for R2ModMan, instead of waiting for DllImport we just invoke it ourselves :)
            _ = DllTools.LoadLibrary(sdkPath);
        }
    }
}
