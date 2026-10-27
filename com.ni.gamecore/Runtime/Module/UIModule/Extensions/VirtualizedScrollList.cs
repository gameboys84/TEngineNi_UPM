using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    /// <summary>Fixed-height vertical rows with bounded live instances; data ownership stays with the caller.</summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class VirtualizedScrollList : MonoBehaviour
    {
        public float rowHeight = 90;
        private ScrollRect scroll;
        private PrefabInstancePool pool;
        private Transform parking;
        private Action<GameObject, int> bind;
        private readonly Dictionary<int, GameObject> visible = new Dictionary<int, GameObject>();
        private readonly List<int> expired = new List<int>();
        public int DataCount { get; private set; }
        public int LiveCount => visible.Count;
        public int CreatedCount => pool?.Created ?? 0;
        public ScrollRect Scroll => scroll;
        public void Initialize(GameObject prefab, Action<GameObject, int> binder)
        {
            if (pool != null) throw new InvalidOperationException("List already initialized.");
            scroll = GetComponent<ScrollRect>(); bind = binder;
            parking = new GameObject("InactiveRows", typeof(RectTransform)).transform;
            parking.SetParent(transform, false); parking.gameObject.SetActive(false);
            pool = new PrefabInstancePool(prefab, parking);
            scroll.onValueChanged.AddListener(OnScroll);
        }
        public void SetCount(int count)
        {
            DataCount = Mathf.Max(0, count);
            ClearVisible(); scroll.content.sizeDelta = new Vector2(0, DataCount * rowHeight);
            scroll.content.anchoredPosition = Vector2.zero;
            Canvas.ForceUpdateCanvases(); Refresh();
        }
        private void OnScroll(Vector2 position) => Refresh();
        public void Refresh()
        {
            if (pool == null || !isActiveAndEnabled) return;
            int first = Mathf.Clamp(Mathf.FloorToInt(scroll.content.anchoredPosition.y / rowHeight) - 1, 0, Mathf.Max(0, DataCount - 1));
            int end = Mathf.Min(DataCount, first + Mathf.CeilToInt(scroll.viewport.rect.height / rowHeight) + 3);
            expired.Clear();
            foreach (var pair in visible) if (pair.Key < first || pair.Key >= end) expired.Add(pair.Key);
            foreach (int index in expired) { pool.Return(visible[index]); visible.Remove(index); }
            for (int i = first; i < end; i++)
            {
                if (visible.ContainsKey(i)) continue;
                var go = pool.Rent(scroll.content); var rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, 1);
                rect.sizeDelta = new Vector2(0, rowHeight - 6); rect.anchoredPosition = new Vector2(0, -i * rowHeight);
                bind(go, i); visible.Add(i, go);
            }
        }
        public void ClearVisible() { if (pool == null) return; foreach (var go in visible.Values) pool.Return(go); visible.Clear(); }
        private void OnEnable() { if (pool != null) Refresh(); }
        private void OnDisable() => ClearVisible();
        public void Dispose()
        {
            if (scroll != null) scroll.onValueChanged.RemoveListener(OnScroll);
            ClearVisible(); pool?.Dispose(); pool = null;
            if (parking != null) Destroy(parking.gameObject);
        }
        private void OnDestroy() => Dispose();
    }
}
