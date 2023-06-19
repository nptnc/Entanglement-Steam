using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using StressLevelZero.Pool;

using Entanglement.Extensions;
using Entanglement.Data;

using MelonLoader;

namespace Entanglement.Objects
{
    [RegisterTypeInIl2Cpp]
    public class PooleeSyncable : MonoBehaviour {
        public static CustomComponentCache<PooleeSyncable> _Cache = new CustomComponentCache<PooleeSyncable>();

        public static Dictionary<ushort, PooleeSyncable> _PooleeLookup = new Dictionary<ushort, PooleeSyncable>(new UnityComparer());

        public PooleeSyncable(IntPtr intPtr) : base(intPtr) { }


        public Poolee Poolee;

        public ushort id;

        public TransformSyncable[] transforms;

        public void Awake() {
            Poolee = GetComponent<Poolee>();
            _Cache.Add(gameObject, this);
        }

        public void Start() {
            _PooleeLookup.Add(id, this);
        }

        public void OnDestroy() {
            _Cache.Remove(gameObject);
            _PooleeLookup.Remove(id);
        }

        public void OnSpawn(long ownerId, SimplifiedTransform simplifiedTransform) {
            MelonCoroutines.Start(CoOnSpawn(ownerId, simplifiedTransform));
        }

        public void SetOwner(long ownerId) {
            foreach (TransformSyncable sync in transforms)
                sync.ForceOwner(ownerId, false);
        }

        public IEnumerator CoOnSpawn(long ownerId, SimplifiedTransform simplifiedTransform) {
            gameObject.SetActive(false);
            yield return null;
            simplifiedTransform.Apply(transform);
            gameObject.SetActive(true);

            SetOwner(ownerId);
        }
    }
}
