using System;

namespace Entanglement {
    // Made a shortcut logger for Entanglement since MelonLoader loves to change the logging format every 5 seconds
    // Instead I'll only have to change this up :)
    public static class EntangleLogger {
        public static void Log(string txt, ConsoleColor txt_color = ConsoleColor.White) {
            EntanglementMod.Instance.LoggerInstance.Msg(txt_color, txt);
        }

        public static void Log(object obj, ConsoleColor txt_color = ConsoleColor.White) {
            EntanglementMod.Instance.LoggerInstance.Msg(txt_color, obj);
        }

        public static void Warn(string txt) {
            EntanglementMod.Instance.LoggerInstance.Warning(txt);
        }

        public static void Warn(object obj) {
            EntanglementMod.Instance.LoggerInstance.Warning(obj);
        }

        public static void Error(string txt) {
            EntanglementMod.Instance.LoggerInstance.Error(txt);
        }

        public static void Error(object obj) {
            EntanglementMod.Instance.LoggerInstance.Error(obj);
        }
    }
}
