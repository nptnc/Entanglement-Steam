using System;

namespace Entanglement.Data {
    [System.Serializable]
    public class DataCorruptionException : Exception {
        public DataCorruptionException(string message) : base(message) { }
        public DataCorruptionException(string message, Exception inner) : base(message, inner) { }
        protected DataCorruptionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
