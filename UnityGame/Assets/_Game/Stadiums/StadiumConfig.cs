using UnityEngine;

namespace CricketGame.Stadiums
{
    public enum PitchCondition
    {
        GreenSeam,
        HardBouncy,
        DustySpin,
        Balanced
    }

    public enum WeatherType
    {
        SunnyClear,
        OvercastHumid,
        EveningFloodlights
    }

    [CreateAssetMenu(fileName = "NewStadiumConfig", menuName = "CricketGame/StadiumConfig")]
    public class StadiumConfig : ScriptableObject
    {
        public string stadiumName = "Gaddafi Stadium";
        public string city = "Lahore";
        public string country = "Pakistan";
        public int capacity = 27000;
        public PitchCondition defaultPitch = PitchCondition.Balanced;
        public WeatherType weather = WeatherType.SunnyClear;
        public float boundaryDistanceMeters = 72f;
    }
}
