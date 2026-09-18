using UnityEngine;

namespace CricketGame.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CricketGame/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Match Settings")]
        public int defaultT20Overs = 20;
        public int powerplayOvers = 6;
        public float pitchLengthMeters = 20.12f;

        [Header("Gameplay Difficulty Scaling")]
        public float sweetSpotWindowMsEasy = 120f;
        public float sweetSpotWindowMsNormal = 80f;
        public float sweetSpotWindowMsHard = 50f;

        [Header("Save System")]
        public string saveFileVersion = "1.0.0";
        public bool autoSaveAfterMatch = true;
    }
}
