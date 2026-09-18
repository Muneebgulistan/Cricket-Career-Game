using System.Collections.Generic;
using UnityEngine;

namespace CricketGame.Stadiums
{
    public enum CrowdDensity
    {
        Low,
        Medium,
        High,
        SoldOut
    }

    public enum CrowdReactionType
    {
        Cheer,
        BoundaryRoar,
        SixCelebration,
        WicketGasp,
        AppealMurmur,
        MexicanWave
    }

    public class CrowdManager : MonoBehaviour
    {
        public static CrowdManager Instance { get; private set; }

        [Header("Crowd Settings")]
        [SerializeField] private CrowdDensity currentDensity = CrowdDensity.High;
        [SerializeField] private List<GameObject> crowdStandSections = new List<GameObject>();

        [Header("Atmosphere Audio / Effect Hooks")]
        [SerializeField] private float crowdExcitementLevel = 0.5f; // 0.0 to 1.0

        public CrowdDensity CurrentDensity
        {
            get { return currentDensity; }
        }

        public float ExcitementLevel
        {
            get { return crowdExcitementLevel; }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ApplyCrowdDensity(currentDensity);
        }

        public void ApplyCrowdDensity(CrowdDensity density)
        {
            currentDensity = density;
            int activeCount = crowdStandSections.Count;

            switch (density)
            {
                case CrowdDensity.Low:
                    activeCount = Mathf.Max(1, crowdStandSections.Count / 3);
                    break;
                case CrowdDensity.Medium:
                    activeCount = Mathf.Max(1, (crowdStandSections.Count * 2) / 3);
                    break;
                case CrowdDensity.High:
                case CrowdDensity.SoldOut:
                    activeCount = crowdStandSections.Count;
                    break;
            }

            for (int i = 0; i < crowdStandSections.Count; i++)
            {
                if (crowdStandSections[i] != null)
                {
                    crowdStandSections[i].SetActive(i < activeCount);
                }
            }

            Debug.Log(string.Format("[CrowdManager] Crowd density set to {0} ({1}/{2} sections active).", density, activeCount, crowdStandSections.Count));
        }

        public void TriggerReaction(CrowdReactionType reaction)
        {
            switch (reaction)
            {
                case CrowdReactionType.BoundaryRoar:
                    crowdExcitementLevel = Mathf.Min(1.0f, crowdExcitementLevel + 0.3f);
                    Debug.Log("[CrowdManager] Crowd roars for FOUR!");
                    break;
                case CrowdReactionType.SixCelebration:
                    crowdExcitementLevel = 1.0f;
                    Debug.Log("[CrowdManager] Massive celebration for MAXIMUM SIX!");
                    break;
                case CrowdReactionType.WicketGasp:
                    crowdExcitementLevel = Mathf.Max(0.2f, crowdExcitementLevel - 0.2f);
                    Debug.Log("[CrowdManager] Stunned silence / roar for WICKET!");
                    break;
                case CrowdReactionType.AppealMurmur:
                    Debug.Log("[CrowdManager] Crowd joins the massive appeal!");
                    break;
            }
        }
    }
}
