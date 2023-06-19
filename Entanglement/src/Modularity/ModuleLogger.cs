using System;
using System.Diagnostics;
using System.Reflection;

using MelonLoader;

namespace Entanglement.Modularity {
    // Stolen from MelonLogger source, sorry herp :(
    public static class ModuleLogger {
        public static string GetCallerName() {
            StackTrace trace = new StackTrace(3, true);
            for (int i = 0; i < 3; i++) {
                var frame = trace.GetFrame(i);
                var info = frame.GetMethod().DeclaringType.Assembly.GetCustomAttribute<EntanglementModuleInfo>();

                if (info != null)
                    if (string.IsNullOrEmpty(info.abbreviation))
                        return info.name;
                    else
                        return info.abbreviation;
            }

            return "Unknown";
        }

        internal static string GetFullMsg(string message) => $"-> [{GetCallerName()}] {message}";

        public static void Msg(string message) => Msg(ConsoleColor.White, message);
        public static void Msg(ConsoleColor color, string message) => EntangleLogger.Log(GetFullMsg(message), color);

        public static void Warn(string message) => EntangleLogger.Warn(GetFullMsg(message));

        public static void Error(string message) => EntangleLogger.Error(GetFullMsg(message));
    }
}
