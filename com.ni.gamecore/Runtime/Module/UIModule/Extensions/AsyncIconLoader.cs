using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    /// <summary>IconLoader adaptation: one Image, async location, cancellation and explicit asset ownership.</summary>
    [RequireComponent(typeof(Image))]
    public sealed class AsyncIconLoader : MonoBehaviour
    {
        [SerializeField] private string location;
        private Image image;
        private CancellationTokenSource pending;
        private AssetLease<Sprite> lease;
        private int version;

        private void Awake() { image = GetComponent<Image>(); }
        private void OnEnable()
        {
            if (!string.IsNullOrWhiteSpace(location)) LoadConfiguredIconAsync().Forget(Debug.LogException);
        }
        private async UniTask LoadConfiguredIconAsync()
        {
            try { await SetLocationAsync(location); }
            catch (OperationCanceledException) { }
        }
        public async UniTask SetLocationAsync(string value, CancellationToken cancellationToken = default)
        {
            if (!isActiveAndEnabled) throw new InvalidOperationException("Icon loader is inactive.");
            image = image != null ? image : GetComponent<Image>();
            location = value;
            int request = ++version;
            pending?.Cancel();
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());
            pending = source;
            AssetLease<Sprite> loaded = null;
            try
            {
                loaded = await new ResourceReference(value).LoadAsync<Sprite>(source.Token);
                if (source.IsCancellationRequested || request != version || this == null || !isActiveAndEnabled) return;
                image.sprite = loaded.Asset;
                lease?.Dispose();
                lease = loaded;
                loaded = null;
            }
            finally
            {
                loaded?.Dispose();
                if (ReferenceEquals(pending, source)) pending = null;
                source.Dispose();
            }
        }
        private void OnDisable()
        {
            ++version;
            pending?.Cancel();
            if (image != null && lease != null && image.sprite == lease.Asset) image.sprite = null;
            lease?.Dispose();
            lease = null;
        }
    }
}
