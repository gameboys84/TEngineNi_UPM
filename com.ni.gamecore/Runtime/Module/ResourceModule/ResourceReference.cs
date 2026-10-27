using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameCore
{
    /// <summary>Replaces the old ABManager GUID reference with a YooAsset location.</summary>
    [Serializable]
    public sealed class ResourceReference
    {
        [SerializeField] private string location;
        public string Location => location;
        public ResourceReference(string location) { this.location = location; }

        public async UniTask<AssetLease<T>> LoadAsync<T>(CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(location)) throw new InvalidOperationException("Resource location is empty.");
            cancellationToken.ThrowIfCancellationRequested();
            IResourceModule resource = ModuleSystem.GetModule<IResourceModule>();
            T asset = await resource.LoadAssetAsync<T>(location, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                if (asset != null) resource.UnloadAsset(asset);
                cancellationToken.ThrowIfCancellationRequested();
            }
            if (asset == null) throw new InvalidOperationException($"Resource load failed: {location}");
            return new AssetLease<T>(resource, asset);
        }
    }

    /// <summary>Owns one successful load. Dispose once when the consumer stops using the asset.</summary>
    public sealed class AssetLease<T> : IDisposable where T : UnityEngine.Object
    {
        private IResourceModule resource;
        public T Asset { get; private set; }
        internal AssetLease(IResourceModule resource, T asset) { this.resource = resource; Asset = asset; }
        public void Dispose()
        {
            if (resource == null) return;
            if (Asset != null) resource.UnloadAsset(Asset);
            resource = null;
            Asset = null;
        }
    }
}
