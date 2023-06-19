namespace Entanglement.Extensions
{
    public static class ArrayExtensions {
        // Shortcut to append a byte array to another
        public static byte[] AddBytes(this byte[] self, byte[] array, ref int index) {
            for (int i = 0; i < array.Length; i++)
                self[index++] = array[i];
            return self;
        }

        public static byte[] AddBytes(this byte[] self, byte[] array, int index) {
            for (int i = 0; i < array.Length; i++)
                self[index++] = array[i];
            return self;
        }
    }
}
