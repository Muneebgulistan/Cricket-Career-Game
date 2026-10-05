using System;
using System.Collections.Generic;
using UnityEngine;

namespace CricketGame.Core
{
    public enum PoolKind
    {
        Ball,
        ParticleEffect,
        FloatingText
    }

    /// <summary>
    /// Scene-wide reusable object pools for frequently spawned match objects.
    /// Pools grow only up to their configured capacity; once warm, Rent/Release
    /// cycles do not create or destroy GameObjects.
    /// </summary>
    public sealed class ObjectPoolManager : MonoBehaviour
    {
        [Serializable]
        private sealed class PoolDefinition
        {
            public PoolKind kind = PoolKind.Ball;
            public GameObject prefab = null;
            [Min(0)] public int prewarmCount = 4;
            [Min(1)] public int maxSize = 32;
        }

        private sealed class Pool
        {
            public GameObject prefab;
            public Transform root;
            public int maxSize;
            public int createdCount;
            public readonly Stack<GameObject> available = new Stack<GameObject>();
            public readonly HashSet<GameObject> instances = new HashSet<GameObject>();
        }

        public static ObjectPoolManager Instance { get; private set; }

        [SerializeField] private PoolDefinition[] pools = new PoolDefinition[0];
        private readonly Dictionary<PoolKind, Pool> poolsByKind = new Dictionary<PoolKind, Pool>();
        private readonly Dictionary<GameObject, Pool> poolByInstance = new Dictionary<GameObject, Pool>();

        public int TotalCreated { get; private set; }
        public int ActiveCount
        {
            get
            {
                int active = 0;
                foreach (Pool pool in poolsByKind.Values) active += pool.createdCount - pool.available.Count;
                return active;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            for (int i = 0; i < pools.Length; i++)
            {
                PoolDefinition definition = pools[i];
                if (definition == null || definition.prefab == null) continue;
                RegisterPool(definition.kind, definition.prefab, definition.prewarmCount, definition.maxSize);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterPool(PoolKind kind, GameObject prefab, int prewarmCount, int maxSize)
        {
            if (prefab == null) throw new ArgumentNullException("prefab");
            if (poolsByKind.ContainsKey(kind)) return;

            GameObject rootObject = new GameObject(kind + " Pool");
            rootObject.transform.SetParent(transform, false);
            Pool pool = new Pool
            {
                prefab = prefab,
                root = rootObject.transform,
                maxSize = Mathf.Max(1, maxSize)
            };
            poolsByKind.Add(kind, pool);

            int count = Mathf.Clamp(prewarmCount, 0, pool.maxSize);
            for (int i = 0; i < count; i++) CreateInstance(pool);
        }

        public GameObject Rent(PoolKind kind, Vector3 position, Quaternion rotation)
        {
            Pool pool;
            if (!poolsByKind.TryGetValue(kind, out pool)) return null;

            GameObject instance;
            if (pool.available.Count > 0)
            {
                instance = pool.available.Pop();
            }
            else
            {
                instance = CreateInstance(pool);
                if (instance != null) pool.available.Pop();
            }
            if (instance == null) return null;

            instance.transform.SetParent(null, true);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            PooledObject pooled = instance.GetComponent<PooledObject>();
            if (pooled != null) pooled.OnRented();
            ParticleSystem particles = instance.GetComponentInChildren<ParticleSystem>();
            if (particles != null)
            {
                particles.Clear(true);
                particles.Play(true);
            }
            return instance;
        }

        public GameObject Rent(PoolKind kind, Vector3 position)
        {
            return Rent(kind, position, Quaternion.identity);
        }

        public GameObject RentBall(Vector3 position, Quaternion rotation)
        {
            return Rent(PoolKind.Ball, position, rotation);
        }

        public GameObject RentParticleEffect(Vector3 position, Quaternion rotation)
        {
            return Rent(PoolKind.ParticleEffect, position, rotation);
        }

        public CricketGame.UI.Match.PooledFloatingText ShowFloatingText(string message, Vector3 screenPosition, Color color)
        {
            GameObject instance = Rent(PoolKind.FloatingText, screenPosition, Quaternion.identity);
            if (instance == null) return null;
            CricketGame.UI.Match.PooledFloatingText floatingText = instance.GetComponent<CricketGame.UI.Match.PooledFloatingText>();
            if (floatingText == null)
            {
                Release(instance);
                return null;
            }

            floatingText.Show(message, screenPosition, color);
            return floatingText;
        }

        public bool Release(GameObject instance)
        {
            if (instance == null) return false;
            Pool pool;
            if (!poolByInstance.TryGetValue(instance, out pool)) return false;
            if (!instance.activeSelf) return false;

            PooledObject pooled = instance.GetComponent<PooledObject>();
            if (pooled != null) pooled.OnReturned();
            ParticleSystem particles = instance.GetComponentInChildren<ParticleSystem>();
            if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            instance.SetActive(false);
            instance.transform.SetParent(pool.root, false);
            pool.available.Push(instance);
            return true;
        }

        public bool TryGetCounts(PoolKind kind, out int created, out int available)
        {
            Pool pool;
            if (poolsByKind.TryGetValue(kind, out pool))
            {
                created = pool.createdCount;
                available = pool.available.Count;
                return true;
            }

            created = 0;
            available = 0;
            return false;
        }

        private GameObject CreateInstance(Pool pool)
        {
            if (pool.createdCount >= pool.maxSize) return null;

            GameObject instance = Instantiate(pool.prefab, pool.root);
            instance.name = pool.prefab.name + " (Pooled)";
            instance.SetActive(false);
            PooledObject pooled = instance.GetComponent<PooledObject>();
            if (pooled == null) pooled = instance.AddComponent<PooledObject>();
            pooled.Initialize(this);

            pool.instances.Add(instance);
            poolByInstance.Add(instance, pool);
            pool.createdCount++;
            TotalCreated++;
            pool.available.Push(instance);
            return instance;
        }
    }
}
