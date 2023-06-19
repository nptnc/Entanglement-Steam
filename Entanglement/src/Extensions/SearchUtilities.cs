using UnityEngine;

using StressLevelZero.Pool;

namespace Entanglement.Extensions {
    public static class SearchUtilities {
        public static Rigidbody[] GetChildBodies(this Transform transform) => ComponentCacheExtensions.m_RigidbodyCache.GetOrAddChildren(transform.gameObject);

        public static Rigidbody[] GetJointedBodies(this Transform transform) => transform.GetJointedRoot().GetChildBodies();

        public static Transform GetJointedRoot(this Transform transform) {
            Transform objRoot = transform.root;
            if (Poolee.Cache.Get(objRoot.gameObject)) return objRoot;

            Transform connectedRoot = transform;
            Rigidbody[] parentRigidbodies = connectedRoot.GetComponentsInParent<Rigidbody>();
            if (parentRigidbodies.Length == 0)
                return connectedRoot;

            Rigidbody rootBody = parentRigidbodies[parentRigidbodies.Length - 1];
            connectedRoot = rootBody.transform;
            return connectedRoot;
        }

    }
}
