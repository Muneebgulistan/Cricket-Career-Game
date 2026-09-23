using System;
using CricketGame.Cricket;
using CricketGame.Players;

namespace CricketGame.Career.Creation
{
    [Serializable]
    public class CareerCreationData
    {
        public string playerName;
        public int age;
        public string nationality;
        public PlayingRole role;
        public BattingStyle battingStyle;
        public BowlingStyle bowlingStyle;
        public int battingSkill;
        public int bowlingSkill;
        public int fieldingSkill;
        public int runningSkill;

        public CareerCreationData()
        {
            playerName = "Muneeb Gulistan";
            age = 16;
            nationality = "Pakistan";
            role = PlayingRole.Batsman;
            battingStyle = BattingStyle.RightHand;
            bowlingStyle = BowlingStyle.RightArmFast;
            battingSkill = 60;
            bowlingSkill = 45;
            fieldingSkill = 55;
            runningSkill = 58;
        }

        public static CareerCreationData CreateDefaultUnder16Prodigy()
        {
            return new CareerCreationData();
        }

        public static CareerCreationData CreateCustom(
            string name, 
            int playerAge, 
            PlayingRole playerRole, 
            BattingStyle batStyle, 
            BowlingStyle bowlStyle)
        {
            CareerCreationData data = new CareerCreationData();
            data.playerName = string.IsNullOrEmpty(name) ? "Young Cricketer" : name;
            data.age = (playerAge >= 14 && playerAge <= 19) ? playerAge : 16;
            data.role = playerRole;
            data.battingStyle = batStyle;
            data.bowlingStyle = bowlStyle;

            switch (playerRole)
            {
                case PlayingRole.Batsman:
                    data.battingSkill = 65;
                    data.bowlingSkill = 40;
                    data.fieldingSkill = 55;
                    data.runningSkill = 60;
                    break;
                case PlayingRole.Bowler:
                    data.battingSkill = 40;
                    data.bowlingSkill = 65;
                    data.fieldingSkill = 55;
                    data.runningSkill = 55;
                    break;
                case PlayingRole.AllRounder:
                    data.battingSkill = 58;
                    data.bowlingSkill = 58;
                    data.fieldingSkill = 58;
                    data.runningSkill = 58;
                    break;
                case PlayingRole.WicketKeeper:
                    data.battingSkill = 60;
                    data.bowlingSkill = 30;
                    data.fieldingSkill = 68;
                    data.runningSkill = 60;
                    break;
            }

            return data;
        }
    }
}
