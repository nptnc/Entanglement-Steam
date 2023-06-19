using System;
using System.Collections;

using UnityEngine;

using MelonLoader;

namespace Entanglement.Managers
{
    [RegisterTypeInIl2Cpp]
    public class RagdollBehaviour : MonoBehaviour
    {
        public RagdollBehaviour(IntPtr intPtr) : base(intPtr) { }

        public Rigidbody[] rbs;

        private float fixedTime;
        private bool isDespawning;

        public void Start() {
            rbs = GetComponentsInChildren<Rigidbody>(true);
            fixedTime = Time.fixedTime;
        }

        public void FixedUpdate() {
            if (isDespawning)
                return;

            if (Time.fixedTime - fixedTime >= 30f) {
                isDespawning = true;
                MelonCoroutines.Start(Despawn());
            }
        }

        public IEnumerator Despawn() {
            transform.position = transform.GetChild(0).position;
            transform.GetChild(0).localPosition = Vector3.zero;

            foreach (Rigidbody rb in rbs) {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            float elapsed = 0f;
            while (elapsed < 1f) {
                elapsed += Time.deltaTime;

                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
