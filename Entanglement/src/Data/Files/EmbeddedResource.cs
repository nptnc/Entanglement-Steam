using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using UnityEngine;

using Entanglement.Extensions;

using MelonLoader;

namespace Entanglement.Data {
    public static class EmbeddedResource {
        public static void ListResourcesFromAssembly(Assembly assembly) {
            foreach (string resource in assembly.GetManifestResourceNames())
                EntangleLogger.Log("Resource: " + resource, ConsoleColor.DarkCyan);
        }

        public static byte[] LoadFromAssembly(Assembly assembly, string name) {
            string[] manifestResources = assembly.GetManifestResourceNames();

            if (manifestResources.Contains(name)) {
                EntangleLogger.Log($"Loading embedded resource data {name}...", ConsoleColor.DarkCyan);
                using (Stream str = assembly.GetManifestResourceStream(name))
                using (MemoryStream memoryStream = new MemoryStream()) {
                    str.CopyTo(memoryStream);
                    EntangleLogger.Log("Done!", ConsoleColor.DarkCyan);
                    return memoryStream.ToArray();
                }
            }

            return null;
        }
    }
}
