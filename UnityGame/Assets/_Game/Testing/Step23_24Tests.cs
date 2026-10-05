using CricketGame.Core;
using UnityEngine;

namespace CricketGame.Testing
{
    /// <summary>
    /// Lightweight in-editor regression checks that do not require a test package.
    /// Configure and prewarm each pool, then invoke RunWarmPoolCycles from a test
    /// scene or an editor debugger to verify the warmed Get/Release path.
    /// </summary>
    public static class Step23_24Tests
    {
        public static bool RunWarmPoolCycles(ObjectPoolManager pool, PoolKind kind, int cycles, out string message)
        {
            if (pool == null)
            {
                message = "ObjectPoolManager is missing.";
                return false;
            }

            int created;
            int available;
            if (!pool.TryGetCounts(kind, out created, out available) || available == 0)
            {
                message = string.Format("{0} pool is not registered and prewarmed.", kind);
                return false;
            }

            int createdBefore = pool.TotalCreated;
            for (int i = 0; i < Mathf.Max(1, cycles); i++)
            {
                GameObject instance = pool.Rent(kind, Vector3.zero, Quaternion.identity);
                if (instance == null || !pool.Release(instance))
                {
                    message = string.Format("{0} pool could not complete cycle {1}.", kind, i + 1);
                    return false;
                }
            }

            int createdAfter = pool.TotalCreated;
            message = createdAfter == createdBefore
                ? string.Format("{0}: {1} warm cycles created no additional GameObjects.", kind, Mathf.Max(1, cycles))
                : string.Format("{0}: created {1} additional GameObject(s) during warm cycles.", kind, createdAfter - createdBefore);
            return createdAfter == createdBefore;
        }
    }
}
