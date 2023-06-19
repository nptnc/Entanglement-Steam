using UnityEngine;

using System;

namespace Entanglement.Data
{
    public static class PhysicsData {
        public const float Deg2Rad = (float)(Math.PI * 2) / 360;

        public static Vector3 GetVelocity(Vector3 current, Vector3 last, float time) => (current - last) / time;
        public static Vector3 GetVelocity(Vector3 current, Vector3 last) => GetVelocity(current, last, Time.deltaTime);

        public static Vector3 GetAngularDisplacement(Quaternion from, Quaternion to) {
            Vector3 x;
            float xMag;

            Quaternion q = to * Quaternion.Inverse(from);
            
            if (q.w < 0) {
                q.x = -q.x;
                q.y = -q.y;
                q.z = -q.z;
                q.w = -q.w;
            }

            q.ToAngleAxis(out xMag, out x);
            x.Normalize();

            x *= Deg2Rad;
            x *= xMag;

            return x;
        }

        public static Vector3 GetAngularVelocity(Quaternion from, Quaternion to, float time) => GetAngularDisplacement(from, to) / time;
        public static Vector3 GetAngularVelocity(Quaternion from, Quaternion to) => GetAngularVelocity(from, to, Time.deltaTime);
    }
}
