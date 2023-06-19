using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Entanglement.Extensions;

namespace Entanglement.Data
{
    public struct SimplifiedTransform {
        public const ushort size = (sizeof(float) * 3 + SimplifiedQuaternion.size);
        public const ushort size_small = (sizeof(short) * 3 + SimplifiedQuaternion.size);

        public Vector3 position;
        public SimplifiedQuaternion rotation;

        public SimplifiedTransform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = SimplifiedQuaternion.SimplifyQuat(rotation);
        }

        public SimplifiedTransform(Transform transform)
        {
            this.position = transform.position;
            this.rotation = SimplifiedQuaternion.SimplifyQuat(transform.rotation);
        }

        public byte[] GetBytes() {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(position.x));
            bytes.AddRange(BitConverter.GetBytes(position.y));
            bytes.AddRange(BitConverter.GetBytes(position.z));

            bytes.AddRange(BitConverter.GetBytes(rotation.c1));
            bytes.AddRange(BitConverter.GetBytes(rotation.c2));
            bytes.AddRange(BitConverter.GetBytes(rotation.c3));
            bytes.Add(rotation.loss);

            return bytes.ToArray();
        }

        public byte[] GetSmallBytes(Vector3 root) {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(root.InverseTransformPosition(position).GetShortBytes());

            bytes.AddRange(BitConverter.GetBytes(rotation.c1));
            bytes.AddRange(BitConverter.GetBytes(rotation.c2));
            bytes.AddRange(BitConverter.GetBytes(rotation.c3));
            bytes.Add(rotation.loss);

            return bytes.ToArray();
        }

        public static SimplifiedTransform SimplyTransform(Transform transform) => SimplyTransform(transform.position, transform.rotation);

        public static SimplifiedTransform SimplyTransform(Vector3 position, Quaternion rotation) {
            SimplifiedTransform simplified = new SimplifiedTransform();

            simplified.position = position;
            simplified.rotation = SimplifiedQuaternion.SimplifyQuat(rotation);

            return simplified;
        }

        public static SimplifiedTransform FromBytes(byte[] bytes) {
            SimplifiedTransform transform = new SimplifiedTransform();

            int index = 0;
            transform.position.x = BitConverter.ToSingle(bytes, index);
            index += sizeof(float);
            transform.position.y = BitConverter.ToSingle(bytes, index);
            index += sizeof(float);
            transform.position.z = BitConverter.ToSingle(bytes, index);
            index += sizeof(float);

            transform.rotation.c1 = BitConverter.ToInt16(bytes, index);
            index += sizeof(short);
            transform.rotation.c2 = BitConverter.ToInt16(bytes, index);
            index += sizeof(short);
            transform.rotation.c3 = BitConverter.ToInt16(bytes, index);
            index += sizeof(short);
            transform.rotation.loss = bytes[index];

            return transform;
        }

        public static SimplifiedTransform FromSmallBytes(byte[] bytes, Vector3 root) {
            SimplifiedTransform transform = new SimplifiedTransform();

            int index = 0;
            transform.position = root.TransformPosition(bytes.FromShortBytes(index));
            index += sizeof(short) * 3;

            transform.rotation.c1 = BitConverter.ToInt16(bytes, index);
            index += sizeof(short);
            transform.rotation.c2 = BitConverter.ToInt16(bytes, index);
            index += sizeof(short);
            transform.rotation.c3 = BitConverter.ToInt16(bytes, index);
            index += sizeof(short);
            transform.rotation.loss = bytes[index];

            return transform;
        }

        public void Apply(Transform target) {
            Vector3 pos = position;
            Quaternion rot = rotation.ExpandQuat();

            target.position = pos;
            target.rotation = rot;
        }

        public void Apply(Rigidbody target)
        {
            Vector3 pos = position;
            Quaternion rot = rotation.ExpandQuat();

            target.MovePosition(pos);
            target.MoveRotation(rot);
        }
    }
}
