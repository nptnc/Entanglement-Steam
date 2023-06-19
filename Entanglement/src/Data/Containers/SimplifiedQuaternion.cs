using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Entanglement.Network;

namespace Entanglement.Data {
    public struct SimplifiedQuaternion {
        public short c1, c2, c3;
        public byte loss; // Lost component in compression

        public const ushort size = sizeof(short) * 3 + sizeof(byte);

        // The amount we multiply / divide by to preserve precision when using shorts
        public const float PRECISION_OFFSET = 10000.0f;

        public static SimplifiedQuaternion SimplifyQuat(Quaternion quat) {
            SimplifiedQuaternion simplified;

            // Based on https://gafferongames.com/post/snapshot_compression/
            // Basically compression works by dropping a component that is the lowest absolute value
            // We first add each component to an array, then sort said array from largest to smallest absolute value

            float[] components = { quat.x, quat.y, quat.z, quat.w };

            byte dropped = 0;
            float biggest = 0.0f;
            float sign = 0.0f;
            for (byte c = 0; c < 4; c++) {
                if (Math.Abs(components[c]) > biggest) {
                    sign = (components[c] < 0) ? -1 : 1;
                    
                    dropped = c;
                    biggest = components[c];
                }
            }

            short[] compressed = new short[3];

            int compIndex = 0;
            for (int c = 0; c < 4; c++) {
                if (c == dropped)
                    continue;

                compressed[compIndex++] = (short)(components[c] * sign * PRECISION_OFFSET);
            }

            simplified.c1 = compressed[0];
            simplified.c2 = compressed[1];
            simplified.c3 = compressed[2];
            simplified.loss = dropped;

            return simplified;
        }

        public Quaternion ExpandQuat() {
            if (loss >= 4)
                throw new DataCorruptionException($"Expanding a quaternion led to a lost component of {loss}!");

            float Pow(float x) => x * x;

            float f1 = c1 / PRECISION_OFFSET;
            float f2 = c2 / PRECISION_OFFSET;
            float f3 = c3 / PRECISION_OFFSET;

            float f4 = Mathf.Sqrt(1f - Pow(f1) - Pow(f2) - Pow(f3));

            // Still dumb...
            switch (loss) {
                case 0:
                    return new Quaternion(f4, f1, f2, f3);

                case 1:
                    return new Quaternion(f1, f4, f2, f3);

                case 2:
                    return new Quaternion(f1, f2, f4, f3);

                case 3:
                    return new Quaternion(f1, f2, f3, f4);
            }

            return Quaternion.identity;
        }
    }
}
