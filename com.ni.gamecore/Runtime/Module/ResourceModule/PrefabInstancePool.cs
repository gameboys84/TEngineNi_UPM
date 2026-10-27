using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameCore
{
    /// <summary>Scene instances backed by TEngine's single-spawn pool. Caller owns the source prefab lease.</summary>
    public sealed class PrefabInstancePool : IDisposable
    {
        private sealed class Item : ObjectBase
        {
            public Item() { }
            public void Setup(GameObject target) { Initialize("instance", target); }
            protected override void Release(bool shutdown) { if (Target is GameObject go && go != null) UnityEngine.Object.Destroy(go); }
        }
        private readonly GameObject prefab;
        private readonly Transform parking;
        private readonly IObjectPoolModule module;
        private readonly IObjectPool<Item> pool;
        private readonly Dictionary<GameObject, Item> active = new Dictionary<GameObject, Item>();
        private bool disposed;
        public int Created { get; private set; }
        public int Reused { get; private set; }
        public int ActiveCount => EnsureAlive() ? active.Count : 0;
        public int CachedCount => EnsureAlive() ? pool.Count - active.Count : 0;
        public PrefabInstancePool(GameObject prefab, Transform parking)
        {
            this.prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            this.parking = parking;
            module = ModuleSystem.GetModule<IObjectPoolModule>();
            pool = module.CreateSingleSpawnObjectPool<Item>("PrefabInstances-" + Guid.NewGuid().ToString("N"));
            pool.AutoReleaseInterval = float.MaxValue;
            pool.ExpireTime = float.MaxValue;
        }
        public GameObject Rent(Transform parent)
        {
            if (!EnsureAlive()) throw new ObjectDisposedException(nameof(PrefabInstancePool));
            var item = pool.Spawn("instance");
            if (item == null)
            {
                var go = UnityEngine.Object.Instantiate(prefab, parent, false);
                item = MemoryPool.Acquire<Item>(); item.Setup(go); pool.Register(item, true); ++Created;
            }
            else ++Reused;
            var target = (GameObject)item.Target;
            target.transform.SetParent(parent, false); target.SetActive(true);
            active.Add(target, item); return target;
        }
        public void Return(GameObject target)
        {
            // The owning module may close the pool before Unity sends OnDisable.
            if (!EnsureAlive()) return;
            if (target == null || !active.TryGetValue(target, out var item)) throw new ArgumentException("Instance is not checked out from this pool.");
            active.Remove(target); target.SetActive(false); target.transform.SetParent(parking, false); pool.Unspawn(item);
        }
        public void Dispose()
        {
            if (!EnsureAlive()) return; disposed = true;
            active.Clear(); module.DestroyObjectPool(pool);
        }
        private bool EnsureAlive()
        {
            if (disposed) return false;
            if (module.HasObjectPool<Item>(pool.Name) && ReferenceEquals(module.GetObjectPool<Item>(pool.Name), pool)) return true;
            disposed = true;
            active.Clear();
            return false;
        }
    }
}
