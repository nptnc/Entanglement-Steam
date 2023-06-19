using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

namespace Entanglement.Network
{
    // Previously used by reflection to generate the handler table
    // Instead we now use a virtual getter and register a different way!
    [Obsolete("Please use the new method of registering methods, without a decorator! Check the example message for the new method!", true)]
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NetworkMessageHandlerIndex : Attribute
    {
        public byte messageIndex;
        public NetworkMessageHandlerIndex(byte messageType) => messageIndex = messageType;
    }

    // Must be of the type a network message is expecting, otherwise we throw an error
    public abstract class NetworkMessageData { }

    public abstract class NetworkMessageHandler
    {
        public virtual byte? MessageIndex { get; } = null; // Virtual getter hell yeah!

        public Type[] Attributes { get; set; }

        // This is like super messy but i'll clean it up later
        // I'm not good at modular stuff - Lakatrazz
        public void ReadMessage(NetworkMessage message, long sender) {
            if (SceneLoader.loading) {
                if (Attributes.Contains(typeof(Net.SkipHandleOnLoading)))
                    return;
                else if (Attributes.Contains(typeof(Net.HandleOnLoaded)))
                    MelonCoroutines.Start(HandleOnLoaded(message, sender));
            }
            else
                HandleMessage(message, sender);
        }

        public IEnumerator HandleOnLoaded(NetworkMessage message, long sender) {
            while (SceneLoader.loading)
                yield return null;

            HandleMessage(message, sender);
        }

        public abstract void HandleMessage(NetworkMessage message, long sender);

        public abstract NetworkMessage CreateMessage(NetworkMessageData data);
    }

    public abstract class NetworkMessageHandler<TData> : NetworkMessageHandler where TData : NetworkMessageData {
        public sealed override NetworkMessage CreateMessage(NetworkMessageData data) {
            if (data is TData tdata) {
                if (!MessageIndex.HasValue) throw new ArgumentNullException("MessageIndex is null, we can't write messages without an index!");

                NetworkMessage message = CreateMessage(tdata);
                message.messageType = MessageIndex.Value;
                return message;
            }
            else
                throw new Exception($"Provided message data was not of type {typeof(TData).Name} or was null!");
        }

        public abstract NetworkMessage CreateMessage(TData data);
    }

}
