using UnityEngine;

namespace CricketGame.Core
{
    /// <summary>Returns an object to its owner when its particle effect finishes.</summary>
    public sealed class PooledObject : MonoBehaviour
    {
        private ObjectPoolManager owner;
        private bool isReturning;

        public void Initialize(ObjectPoolManager poolOwner)
        {
            owner = poolOwner;
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particles = particleSystems[i];
                ParticleSystem.MainModule main = particles.main;
                main.stopAction = ParticleSystemStopAction.Callback;
                if (particles.gameObject != gameObject)
                {
                    PooledParticleReturn callback = particles.GetComponent<PooledParticleReturn>();
                    if (callback == null) callback = particles.gameObject.AddComponent<PooledParticleReturn>();
                    callback.Initialize(this);
                }
            }
        }

        public void OnRented() { isReturning = false; }
        public void OnReturned() { isReturning = true; }

        private void OnParticleSystemStopped()
        {
            ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (!isReturning && owner != null && gameObject.activeInHierarchy) owner.Release(gameObject);
        }
    }

    /// <summary>Forwards child particle completion callbacks to the pooled root.</summary>
    public sealed class PooledParticleReturn : MonoBehaviour
    {
        private PooledObject pooledRoot;

        public void Initialize(PooledObject root)
        {
            pooledRoot = root;
        }

        private void OnParticleSystemStopped()
        {
            if (pooledRoot != null && pooledRoot.gameObject.activeInHierarchy)
                pooledRoot.ReturnToPool();
        }
    }
}
