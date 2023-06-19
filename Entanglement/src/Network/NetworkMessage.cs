using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Entanglement.Extensions;

using MelonLoader;

namespace Entanglement.Network
{
    public enum NetworkChannel : byte {
        Reliable    = 0,
        Unreliable  = 1,
        Attack      = 2,
        Object      = 3,
        Transaction = 4
    }

    public class NetworkMessage {
        public byte messageType;
        public byte[] messageData = new byte[0];

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[1 + messageData.Length];

            bytes[0] = messageType;

            for (int b = 1; b < bytes.Length; b++)
                bytes[b] = messageData[b - 1]; // Let's hope this doesn't break

            return bytes;
        }



        //
        // Static stuff
        //

        // Handlers are created up front, they're not static
        public static void RegisterHandlersFromAssembly(Assembly targetAssembly) {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            EntangleLogger.Log($"Populating MessageHandler list from {targetAssembly.GetName().Name}!");

            // I am aware LINQ is kinda gross but this is works!
            targetAssembly.GetTypes()
                .Where(type => typeof(NetworkMessageHandler).IsAssignableFrom(type) && !type.IsAbstract)
                .Where(type => type.GetCustomAttribute<Net.NoAutoRegister>() == null)
                .ForEach(type => {
                    try
                    {
                        RegisterHandler(type);
                    }
                    catch (Exception e)
                    {
                        EntangleLogger.Error(e.Message);
                    }
                });
        }

        public static void RegisterHandler<T>() where T : NetworkMessageHandler => RegisterHandler(typeof(T));

        protected static void RegisterHandler(Type type)
        {
            NetworkMessageHandler handler = Activator.CreateInstance(type) as NetworkMessageHandler;

            if (handler.MessageIndex == null)
            {
                EntangleLogger.Warn($"Didn't register {type.Name} because its message index was null!");
            }
            else
            {
                byte index = handler.MessageIndex.Value;

                if (handlers[index] != null) throw new Exception($"{type.Name} has the same index as {handlers[index].GetType().Name}, we can't replace handlers!");

                EntangleLogger.Log($"Registered {type.Name}");

                // We give the handler access to all known attributes so we can put them into effect
                var attributes = type.GetCustomAttributes();
                List<Type> types = new List<Type>();
                foreach (Attribute attribute in attributes) {
                    if (attribute is Net.NoAutoRegister)
                        continue;
                    types.Add(attribute.GetType());
                }
                handler.Attributes = types.ToArray();

                handlers[index] = handler;
            }
        } 

        public static NetworkMessage CreateMessage(byte type, NetworkMessageData data) {
            try {
                return handlers[type].CreateMessage(data);
            }
            catch (Exception e) {
                EntangleLogger.Error($"Failed creating network message with reason: {e.Message}\nTrace:{e.StackTrace}");
            }

            return null;
        }

        public static void ReadMessage(NetworkMessage message, long sender) {
            try {
                handlers[message.messageType].ReadMessage(message, sender);
            }
            catch (Exception e) {
                EntangleLogger.Error($"Failed handling network message with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }

        public static readonly NetworkMessageHandler[] handlers = new NetworkMessageHandler[byte.MaxValue];
    }
}
