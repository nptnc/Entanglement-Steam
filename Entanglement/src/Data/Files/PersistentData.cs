﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;

namespace Entanglement.Data {
    public static class PersistentData {
        public static string persistentPath { get; private set; }

        public static void Initialize() {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            persistentPath = appdata + "/EntanglementMod/";

            EntangleLogger.Log($"Data is at %AppData%/EntanglementMod/", ConsoleColor.DarkCyan);
            ValidateDirectory(persistentPath);
        }

        public static void ValidateDirectory(string path) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string GetPath(string appended) => persistentPath + appended;
    }
}
