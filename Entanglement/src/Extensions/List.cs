using System.Collections.Generic;
using System.Linq;

namespace Entanglement.Extensions
{
    public static class ListExtensions {
        public static bool Has<T>(this List<T> list, T obj) where T : UnityEngine.Object => list.Any(o => o == obj);
        
        public static bool Has<T>(this T[] array, T obj) where T : UnityEngine.Object => array.Any(o => o == obj);
    }
}
