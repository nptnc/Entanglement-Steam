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
    public static class EmebeddedAssetBundle {
        public static AssetBundle LoadFromAssembly(Assembly assembly, string name) {
            string[] manifestResources = assembly.GetManifestResourceNames();

            if (manifestResources.Contains(name)) {
                EntangleLogger.Log($"Loading embedded bundle data {name}...", ConsoleColor.DarkCyan);

                byte[] bytes;
                using (Stream str = assembly.GetManifestResourceStream(name))
                using (MemoryStream memoryStream = new MemoryStream()) {
                    str.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }

                EntangleLogger.Log($"Loading bundle from data {name}, please be patient...", ConsoleColor.DarkCyan);
                var temp = AssetBundle.LoadFromMemory(bytes);
                EntangleLogger.Log($"Done!", ConsoleColor.DarkCyan);
                return temp;
            }

            return null;
        }
    }
}
