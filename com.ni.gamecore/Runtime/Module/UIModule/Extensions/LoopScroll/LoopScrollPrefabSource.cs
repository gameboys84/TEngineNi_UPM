using UnityEngine;
using Object = UnityEngine.Object;
using System;
using System.Collections.Generic;
namespace GameCore
{
    /// <summary>Per-list recycling of preloaded prefabs. No legacy global resource manager.</summary>
    [Serializable]
    public class LoopScrollPrefabSource : IDisposable
    {
        public GameObject item_obj;
        public string item_name;
        public int poolSize = 5;
        private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Stack<GameObject>> inactive = new Dictionary<string, Stack<GameObject>>();
        private readonly Dictionary<GameObject, string> owned = new Dictionary<GameObject, string>();
        private Transform poolRoot;
        public void RegisterPrefab(GameObject prefab)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (prefabs.TryGetValue(prefab.name, out var existing) && existing != prefab)
                throw new ArgumentException("Loop-list prefab names must be unique.", nameof(prefab));
            if (prefabs.ContainsKey(prefab.name)) return;
            prefabs.Add(prefab.name, prefab);
            inactive.Add(prefab.name, new Stack<GameObject>());
        }
        public virtual GameObject GetObject() => GetObject(item_obj);
        public virtual GameObject GetObject(GameObject prefab) { RegisterPrefab(prefab); return GetObject(prefab.name); }
        public virtual GameObject GetObject(string itemName)
        {
            if (!prefabs.TryGetValue(itemName, out var prefab))
            {
                if (item_obj == null || (itemName != item_name && itemName != item_obj.name))
                    throw new ArgumentException("Register a preloaded prefab before filling data.", nameof(itemName));
                RegisterPrefab(item_obj);
                prefab = item_obj;
            }
            var stack = inactive[prefab.name];
            GameObject cell = null;
            while (stack.Count > 0 && cell == null) cell = stack.Pop();
            if (cell == null) { cell = Object.Instantiate(prefab); owned.Add(cell, prefab.name); }
            cell.SetActive(true);
            return cell;
        }
        public virtual void ReturnObject(Transform cell)
        {
            if (cell == null) return;
            if (!owned.TryGetValue(cell.gameObject, out string key)) throw new ArgumentException("Cell belongs to another source.", nameof(cell));
            var stack = inactive[key];
            if (stack.Contains(cell.gameObject)) return;
            if (stack.Count >= Math.Max(0, poolSize)) { owned.Remove(cell.gameObject); Object.Destroy(cell.gameObject); return; }
            if (poolRoot == null) { poolRoot = new GameObject("LoopScrollPool").transform; poolRoot.gameObject.SetActive(false); }
            cell.SetParent(poolRoot, false);
            cell.gameObject.SetActive(false);
            stack.Push(cell.gameObject);
        }
        public void Dispose()
        {
            foreach (var cell in owned.Keys) if (cell != null) Object.Destroy(cell);
            owned.Clear(); inactive.Clear(); prefabs.Clear();
            if (poolRoot != null) Object.Destroy(poolRoot.gameObject);
            poolRoot = null;
        }
    }
}
