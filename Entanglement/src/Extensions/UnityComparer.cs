using System.Collections.Generic;

namespace Entanglement.Extensions {
    public class UnityComparer : IEqualityComparer<UnityEngine.Object>, IEqualityComparer<ushort> {
        public bool Equals(UnityEngine.Object lft, UnityEngine.Object rht) => lft == rht;

        public bool Equals(ushort lft, ushort rht) => lft == rht;

        public int GetHashCode(UnityEngine.Object obj) => obj.GetInstanceID();

        public int GetHashCode(ushort sh) => sh.GetHashCode(); 
    }
}
