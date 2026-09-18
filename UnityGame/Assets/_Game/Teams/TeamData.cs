using System;
using System.Collections.Generic;
using CricketGame.Players;

namespace CricketGame.Teams
{
    [Serializable]
    public class TeamData
    {
        public string teamId;
        public string teamName;
        public string shortName;
        public string region;
        public string primaryColorHex;
        public string secondaryColorHex;
        public List<PlayerProfile> squad = new List<PlayerProfile>();
        public List<string> playingXIIds = new List<string>();

        public TeamData()
        {
            teamId = Guid.NewGuid().ToString();
            teamName = "Regional Team";
            shortName = "REG";
            region = "Pakistan";
            primaryColorHex = "#1E88E5";
            secondaryColorHex = "#FFFFFF";
        }

        public TeamData(string id, string name, string abbr, string reg)
        {
            teamId = id;
            teamName = name;
            shortName = abbr;
            region = reg;
            primaryColorHex = "#1E88E5";
            secondaryColorHex = "#FFFFFF";
        }
    }
}
