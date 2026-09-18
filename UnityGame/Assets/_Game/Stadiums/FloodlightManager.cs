using System.Collections.Generic;
using UnityEngine;

namespace CricketGame.Stadiums
{
    public class FloodlightManager : MonoBehaviour
    {
        public static FloodlightManager Instance { get; private set; }

        [Header("Floodlight Towers")]
        [SerializeField] private List<Light> floodlightSources = new List<Light>();
        [SerializeField] private List<MeshRenderer> lightBulbEmissiveMeshes = new List<MeshRenderer>();

        [Header("Lighting State")]
        [SerializeField] private bool areFloodlightsOn = false;
        [SerializeField] private float defaultNightIntensity = 2.0f;
        [SerializeField] private Color lightColor = new Color(0.9f, 0.95f, 1.0f, 1.0f);

        public bool AreFloodlightsOn
        {
            get { return areFloodlightsOn; }
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

        public void SetFloodlights(bool turnOn, float intensity = -1f)
        {
            areFloodlightsOn = turnOn;
            float targetIntensity = turnOn ? (intensity >= 0 ? intensity : defaultNightIntensity) : 0f;

            foreach (Light lightSource in floodlightSources)
            {
                if (lightSource != null)
                {
                    lightSource.enabled = turnOn;
                    lightSource.intensity = targetIntensity;
                    lightSource.color = lightColor;
                }
            }

            Debug.Log(string.Format("[FloodlightManager] Floodlights state: {0} (Intensity: {1})", (turnOn ? "ON" : "OFF"), targetIntensity));
        }

        public void ToggleFloodlights()
        {
            SetFloodlights(!areFloodlightsOn);
        }

        public void RegisterLightSource(Light lightSource)
        {
            if (lightSource != null && !floodlightSources.Contains(lightSource))
            {
                floodlightSources.Add(lightSource);
                lightSource.enabled = areFloodlightsOn;
            }
        }
    }
}
