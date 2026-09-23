using System;
using CricketGame.Cricket;

namespace CricketGame.Career.MatchIntegration
{
    [Serializable]
    public class CareerMatchData
    {
        public string tournamentId;
        public string fixtureId;
        public string tournamentName;
        public CareerLevel level;
        public string opponentTeamId;
        public string opponentTeamName;
        public string venueName;
        public PitchType pitchCondition;
        public float matchImportance; // 1.0 = standard, 1.25 = derby, 1.5 = semi-final, 2.0 = final
        public PlayingRole userRoleInMatch;
        public int battingPosition;
        public bool isUserPlaying;

        public CareerMatchData()
        {
            tournamentId = "U16_CUP_S1";
            fixtureId = "FIX_1";
            tournamentName = "National Under-16 Cup";
            level = CareerLevel.Under16Cup;
            opponentTeamId = "KAR_U16";
            opponentTeamName = "Karachi Kings U-16";
            venueName = "Gaddafi Stadium, Lahore";
            pitchCondition = PitchType.Standard;
            matchImportance = 1.0f;
            userRoleInMatch = PlayingRole.Batsman;
            battingPosition = 3;
            isUserPlaying = true;
        }

        public CareerMatchData(string tourneyId, string tourneyName, string fixId, string opponent, CareerLevel lvl, int batPos)
        {
            tournamentId = tourneyId;
            tournamentName = tourneyName;
            fixtureId = fixId;
            opponentTeamId = opponent.Replace(" ", "_").ToUpper();
            opponentTeamName = opponent;
            venueName = "National Stadium";
            pitchCondition = PitchType.Standard;
            matchImportance = 1.0f;
            userRoleInMatch = PlayingRole.Batsman;
            battingPosition = batPos;
            level = lvl;
            isUserPlaying = true;
        }
    }
}
