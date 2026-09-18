using System;
using UnityEngine;

namespace CricketGame.Stadiums
{
    [Serializable]
    public class StadiumLightingSettings
    {
        public Color sunColor = new Color(1f, 0.96f, 0.88f, 1f);
        public float sunIntensity = 1.2f;
        public Vector3 sunEulerAngles = new Vector3(50f, -30f, 0f);
        public Color ambientColor = new Color(0.25f, 0.3f, 0.35f, 1f);
        public bool enableFloodlights = false;
        public float floodlightIntensity = 0f;
    }

    [Serializable]
    public class StadiumCameraSettings
    {
        public Vector3 broadcastCameraPosition = new Vector3(0f, 14f, -28f);
        public Vector3 broadcastCameraRotation = new Vector3(22f, 0f, 0f);
        public float broadcastFieldOfView = 55f;
    }

    [CreateAssetMenu(fileName = "NewStadiumData", menuName = "CricketGame/StadiumData")]
    public class StadiumData : ScriptableObject
    {
        [Header("Identity")]
        public string stadiumId = "STADIUM_001";
        public string stadiumName = "Career Cricket Stadium";
        public string country = "Pakistan";
        public int capacity = 35000;

        [Header("Ground Dimensions")]
        public PitchCondition pitchType = PitchCondition.Balanced;
        public float boundaryRadiusMeters = 70f;
        public float pitchLengthMeters = 20.12f;
        public float pitchWidthMeters = 3.05f;

        [Header("Lighting Presets")]
        public StadiumLightingSettings dayLighting = new StadiumLightingSettings
        {
            sunIntensity = 1.2f,
            enableFloodlights = false,
            floodlightIntensity = 0f
        };

        public StadiumLightingSettings nightLighting = new StadiumLightingSettings
        {
            sunIntensity = 0.1f,
            enableFloodlights = true,
            floodlightIntensity = 2.0f,
            ambientColor = new Color(0.08f, 0.1f, 0.15f, 1f)
        };

        [Header("Camera Configuration")]
        public StadiumCameraSettings cameraSettings = new StadiumCameraSettings();

        public static StadiumData CreateDefaultStadium()
        {
            StadiumData data = ScriptableObject.CreateInstance<StadiumData>();
            data.stadiumId = "STADIUM_001";
            data.stadiumName = "Career Cricket Stadium";
            data.country = "Pakistan";
            data.capacity = 35000;
            data.pitchType = PitchCondition.Balanced;
            data.boundaryRadiusMeters = 70f;
            return data;
        }
    }
}
