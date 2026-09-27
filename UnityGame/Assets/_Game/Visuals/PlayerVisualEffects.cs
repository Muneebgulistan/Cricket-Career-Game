using System;
using UnityEngine;

namespace CricketGame.Visuals
{
    public class PlayerVisualEffects : MonoBehaviour
    {
        [Header("Trail Renderers (Bat FX)")]
        [SerializeField] private TrailRenderer batTrail;
        [SerializeField] private float aggressiveSwingSpeedThreshold = 100f; // km/h

        [Header("Particle Systems (Pitch & Turf FX)")]
        [SerializeField] private ParticleSystem bowlerFootstrikeParticles;
        [SerializeField] private ParticleSystem fielderSlideParticles;
        [SerializeField] private ParticleSystem wicketImpactParticles;

        [Header("Transform Anchors")]
        [SerializeField] private Transform frontFootAnchor;
        [SerializeField] private Transform batTipAnchor;

        private bool isTrailActive = false;

        public bool IsTrailActive { get { return isTrailActive; } }

        public event Action OnBatTrailActivated;
        public event Action OnBatTrailDeactivated;
        public event Action<Vector3> OnFootstrikeEffectSpawned;
        public event Action<Vector3> OnFielderSlideEffectSpawned;

        private void Awake()
        {
            if (batTrail == null) batTrail = GetComponentInChildren<TrailRenderer>();
            DisableBatTrailImmediate();
        }

        public void InitializeAnchors(Transform foot, Transform batTip)
        {
            frontFootAnchor = foot;
            batTipAnchor = batTip;
        }

        public void SetParticleSystems(ParticleSystem footstrike, ParticleSystem slide, ParticleSystem wicketImpact)
        {
            bowlerFootstrikeParticles = footstrike;
            fielderSlideParticles = slide;
            wicketImpactParticles = wicketImpact;
        }

        // Bat Trail Methods
        public void EnableBatTrail(float shotExitSpeedKph = 110f)
        {
            if (shotExitSpeedKph < aggressiveSwingSpeedThreshold) return;

            isTrailActive = true;
            if (batTrail != null)
            {
                batTrail.enabled = true;
                batTrail.emitting = true;
            }

            if (OnBatTrailActivated != null)
            {
                OnBatTrailActivated();
            }
        }

        public void DisableBatTrail()
        {
            isTrailActive = false;
            if (batTrail != null)
            {
                batTrail.emitting = false;
            }

            if (OnBatTrailDeactivated != null)
            {
                OnBatTrailDeactivated();
            }
        }

        public void DisableBatTrailImmediate()
        {
            isTrailActive = false;
            if (batTrail != null)
            {
                batTrail.enabled = false;
                batTrail.emitting = false;
            }
        }

        // Pitch & Footstrike Particle Effect
        public void TriggerFootstrikeParticles()
        {
            Vector3 spawnPos = frontFootAnchor != null ? frontFootAnchor.position : transform.position;
            spawnPos.y = 0.02f; // Ground level

            if (bowlerFootstrikeParticles != null)
            {
                bowlerFootstrikeParticles.transform.position = spawnPos;
                bowlerFootstrikeParticles.Play();
            }

            if (OnFootstrikeEffectSpawned != null)
            {
                OnFootstrikeEffectSpawned(spawnPos);
            }
        }

        // Fielder Dive / Slide Particle Effect
        public void TriggerFielderSlideParticles()
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y = 0.02f;

            if (fielderSlideParticles != null)
            {
                fielderSlideParticles.transform.position = spawnPos;
                fielderSlideParticles.Play();
            }

            if (OnFielderSlideEffectSpawned != null)
            {
                OnFielderSlideEffectSpawned(spawnPos);
            }
        }

        // Wicket Shatter / Impact Particle Effect
        public void TriggerWicketImpactParticles(Vector3 stumpPosition)
        {
            if (wicketImpactParticles != null)
            {
                wicketImpactParticles.transform.position = stumpPosition;
                wicketImpactParticles.Play();
            }
        }
    }
}
