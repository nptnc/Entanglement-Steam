using System;
using System.IO;
using System.Security.Cryptography;

namespace Entanglement.Data {
    public class Checksum {
        public byte[] hash;
        public string base64;
        
        public Checksum(byte[] hash) {
            this.hash = hash;
            base64 = Convert.ToBase64String(hash);
        }
    }

    public static class ChecksumTool {
        public static Checksum current;

        public static Checksum Calculate(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
                return new Checksum(md5.ComputeHash(data));
        }

        public static void CalculateMod() {
            using (MD5 md5 = MD5.Create()) {
                string entanglementPath = Directory.GetCurrentDirectory();
                entanglementPath += "/Mods/Entanglement.dll";

                current = Calculate(File.ReadAllBytes(entanglementPath));
                EntangleLogger.Log($"Hash Base64: {current.base64}", ConsoleColor.DarkCyan);
            }
        }
    }
}
