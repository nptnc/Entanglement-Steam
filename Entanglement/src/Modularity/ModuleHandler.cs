using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

using MelonLoader;

using Entanglement.Network;
using Entanglement.Data;

namespace Entanglement.Modularity
{
    public static class ModuleHandler
    {
        public static readonly List<EntanglementModule> loadedModules = new List<EntanglementModule>();

        public static string modulePath = Directory.GetCurrentDirectory().Replace('\\', '/') + "/UserData/Entanglement/Modules/";
        public static string moduleDependencyPath = modulePath + "Dependencies/";

        // Easiest way someone can load their module from a mod
        public static void LoadEmbeddedModule(Assembly holder, string resPath)
        {
            byte[] asmBytes = EmbeddedResource.LoadFromAssembly(holder, resPath);

            if (asmBytes == null)
                throw new Exception($"Failed to load resource at '{resPath}'");

            Assembly loadedAsm = Assembly.Load(asmBytes);
            SetupModule(loadedAsm);
        }

        // Should be called using reflection
        public static void SetupModule(Assembly moduleAssembly)
        {
            if (DiscordIntegration.isInvalid)
                return;

            if (moduleAssembly != null)
            {
                var moduleInfo = moduleAssembly.GetCustomAttribute<EntanglementModuleInfo>();

                // *sigh* reflection is ugly at times ngl
                if (moduleInfo != null)
                {
                    if (moduleInfo.moduleType != null)
                    {
                        PrintSpacer(moduleInfo);

                        if (typeof(EntanglementModule).IsAssignableFrom(moduleInfo.moduleType) && !moduleInfo.moduleType.IsAbstract)
                        {
                            EntanglementModule module = Activator.CreateInstance(moduleInfo.moduleType) as EntanglementModule;

                            if (module != null)
                            {
                                loadedModules.Add(module);
                                module.OnModuleLoaded();
                            }
                        }
                    }
                }
            }
        }

        // Ugly but idc enough to make it look pretty
        internal static void PrintSpacer(EntanglementModuleInfo moduleInfo)
        {
            EntangleLogger.Log("--==== Entanglement Module ====--", ConsoleColor.Magenta);

            EntangleLogger.Log($"{moduleInfo.name} - v{moduleInfo.version}");

            if (!string.IsNullOrEmpty(moduleInfo.abbreviation))
                EntangleLogger.Log($"aka [{moduleInfo.abbreviation}]");

            EntangleLogger.Log($"by {moduleInfo.author}");

            EntangleLogger.Log("--=============================--", ConsoleColor.Magenta);
        }

        //
        // MelonLoader wrappers
        //
        public static void Update()
        {
            foreach (var module in loadedModules)
                module.Update();
        }

        public static void FixedUpdate()
        {
            foreach (var module in loadedModules)
                module.FixedUpdate();
        }

        public static void LateUpdate()
        {
            foreach (var module in loadedModules)
                module.LateUpdate();
        }

        public static void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var module in loadedModules)
                module.OnSceneWasInitialized(buildIndex, sceneName);
        }

        public static void OnLoadingScreen()
        {
            foreach (var module in loadedModules)
                module.OnLoadingScreen();
        }

        public static void OnApplicationQuit()
        {
            foreach (var module in loadedModules)
                module.OnApplicationQuit();
        }
    }
}