using UnityEngine;
using CricketGame.Cricket;

namespace CricketGame.Players
{
    public class PlayerAttributes : MonoBehaviour
    {
        [Header("Player Identity")]
        public string playerName = "Unknown Player";
        public int age = 18;
        public string nationality = "Pakistan";
        public PlayingRole playingRole = PlayingRole.Batsman;

        [Header("Ratings")]
        public int overallRating = 60;
        public int battingRating = 60;
        public int bowlingRating = 40;
        public int fieldingRating = 50;

        [Header("Condition")]
        public int fitness = 100;
        public int form = 75;
        public int experience = 0;

        [Header("Physical & Team Info")]
        public PlayerHand playerHand = PlayerHand.Right;
        public float height = 1.80f;
        public float weight = 75.0f;
        public int jerseyNumber = 10;
        public bool isCaptain = false;
        public bool isWicketKeeper = false;

        public PlayerProfile SourceProfile { get; private set; }
        public PlayerProfile Profile { get { return ToProfile(); } }

        public PlayerProfile ToProfile()
        {
            if (SourceProfile != null) return SourceProfile;
            PlayerProfile p = new PlayerProfile(playerName, age, nationality, playingRole, BattingStyle.RightHand, BowlingStyle.None);
            p.overallRating = overallRating;
            p.battingRating = battingRating;
            p.bowlingRating = bowlingRating;
            p.fieldingRating = fieldingRating;
            p.fitness = fitness;
            p.form = form;
            p.experience = experience;
            p.playerHand = playerHand;
            p.height = height;
            p.weight = weight;
            p.jerseyNumber = jerseyNumber;
            p.isCaptain = isCaptain;
            p.isWicketKeeper = isWicketKeeper;
            return p;
        }

        public void Initialize(PlayerProfile profile)
        {
            if (profile == null) return;

            SourceProfile = profile;
            playerName = profile.name;
            age = profile.age;
            nationality = profile.nationality;
            playingRole = profile.playingRole;

            overallRating = profile.overallRating;
            battingRating = profile.battingRating;
            bowlingRating = profile.bowlingRating;
            fieldingRating = profile.fieldingRating;

            fitness = profile.fitness;
            form = profile.form;
            experience = profile.experience;

            playerHand = profile.playerHand;
            height = profile.height;
            weight = profile.weight;
            jerseyNumber = profile.jerseyNumber;
            isCaptain = profile.isCaptain;
            isWicketKeeper = profile.isWicketKeeper;
        }
    }
}
