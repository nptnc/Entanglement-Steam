using System;
using System.Reflection; // We need reflection to invoke Entanglement features without actually using the mod

using MelonLoader;

//
// OptionalDependency Example
//

// This example demonstrates how to integrate Entanglement into your mod optionally
// We can do this by breaking up the main mod and its compatibility module into 2 libraries
// This library is the one loaded by MelonLoader! From here if we detect Entanglement we will load our module!

namespace Entanglement.Examples
{
    public class OptionalDependencyMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            // First we check if Entanglement is loaded, without actually touching the mod itself
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            Assembly entanglementAsm = null;
            foreach (Assembly asm in loadedAssemblies)
                if (asm.FullName == "Entanglement")
                {
                    entanglementAsm = asm;
                    break;
                }

            // If we've found entanglement, we will pull out the methods we need using reflection
            if (entanglementAsm != null)
            {
                // The main one we need here is LoadEmbeddedModule();
                // Give this method the embedded resource path (ex: Entanglement.Modules.Example.dll)
                // Usually the path is structured as "Assembly Name + Relative Folder + File Name", each + representing a '.'

                // LoadEmbeddedModule() is the simplest way to load a module
                // You give Entanglement your mod assembly and the embedded resource path and the mod handles the loading process!
                // LoadEmbeddedModule()'s full path is 'Entanglement.Modularity.ModuleHandler.LoadEmbeddedModule()'

                Type handlerType = entanglementAsm.GetType("Entanglement.Modularity.ModuleHandler");
                MethodInfo loadMethod = handlerType.GetMethod("LoadEmbeddedModule", BindingFlags.Static | BindingFlags.Public);

                // Remember, this takes the args (Assembly, string), those being (this Assembly, the resource path)
                Assembly thisAsm = Assembly.GetExecutingAssembly();

                // 1st parameter here is null, telling .NET that we're invoking a static method
                // 2nd parameter is the arguments, which .NET takes "object"'s as input!
                loadMethod.Invoke(null, new object[] { thisAsm, "ExampleOptionalDependency.Modules.ExampleModule.dll" });
            } 
            else
            {
                LoggerInstance.Msg("Entanglement wasn't found, not loading optional module!");
            }
        }
    }
}
