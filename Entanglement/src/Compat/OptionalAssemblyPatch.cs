using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Entanglement.Extensions;

using MelonLoader;

namespace Entanglement.Compat
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class OptionalAssemblyTarget : Attribute {
        public readonly string targetAssembly;
        public OptionalAssemblyTarget(string targetAssembly) => this.targetAssembly = targetAssembly;
    }

    public abstract class OptionalAssemblyPatch {
        public static void AttemptPatches() {
#if DEBUG
            EntangleLogger.Log("Logging loaded assemblies...", ConsoleColor.DarkMagenta);
            AppDomain.CurrentDomain.GetAssemblies().ForEach((asm) => { EntangleLogger.Log(asm.GetName().Name, ConsoleColor.DarkMagenta); });
            EntangleLogger.Log("Done!", ConsoleColor.DarkMagenta);
#endif

            IEnumerable<Type> types = EntanglementMod.entanglementAssembly.GetTypes()
                .Where(type => typeof(OptionalAssemblyPatch).IsAssignableFrom(type) && !type.IsAbstract);

            foreach (Type type in types) {
                foreach (object attribute in type.GetCustomAttributes())
                {
                    OptionalAssemblyTarget targetAttribute = attribute as OptionalAssemblyTarget;

                    if (targetAttribute != null) {
                        OptionalAssemblyPatch patch = Activator.CreateInstance(type) as OptionalAssemblyPatch;
                        patch.TryPatch(targetAttribute.targetAssembly);
                    }
                }
            }
        }

        public bool TryPatch(string assemblyName) {
            try {
                Assembly found = AppDomain.CurrentDomain.GetAssemblies().First((asm) => asm.GetName().Name == assemblyName);

                if (found != null) {
                    EntangleLogger.Log($"Optional assembly {assemblyName} was found! Patching methods for compatibility!");
                    DoPatches(found);
                }
                
                return true;
            }
            catch (Exception e) {
                if (e is InvalidOperationException)
                    EntangleLogger.Warn($"{assemblyName} is not installed! If you aren't using {assemblyName} compatibility ignore this.");
                else
                    EntangleLogger.Error($"Failed patching {assemblyName} because {e.Message}\n Trace:\n{e.StackTrace}");
                return false;
            }
        }

        public abstract void DoPatches(Assembly target);
    }
}
