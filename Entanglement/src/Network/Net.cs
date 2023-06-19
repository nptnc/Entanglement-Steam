using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.Network
{
    public static class Net
    {
        /// <summary>
        /// Prevents this message handler from being automatically registered by the mod.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class NoAutoRegister : Attribute { }

        /// <summary>
        /// Waits to handle any recieved messages until the scene has finished loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class HandleOnLoaded : Attribute { }

        /// <summary>
        /// Skips any recieved messages if the scene is currently loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public class SkipHandleOnLoading : Attribute { }
    }
}
