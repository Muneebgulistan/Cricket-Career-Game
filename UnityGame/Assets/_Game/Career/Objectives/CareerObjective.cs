using System;
using UnityEngine;
using CricketGame.Career.Rewards;

namespace CricketGame.Career.Objectives
{
    public enum ObjectiveType
    {
        ScoreFifty,
        ScoreHundred,
        ScoreRuns,
        TakeThreeWickets,
        TakeWickets,
        MaintainBattingAverage,
        MaintainBowlingEconomy,
        WinMatch,
        CompleteTournament,
        EarnPlayerOfMatch
    }

    public enum ObjectiveStatus
    {
        Active,
        Completed,
        Failed
    }

    [Serializable]
    public class CareerObjective
    {
        public string id;
        public string title;
        public string description;
        public ObjectiveType type;
        public ObjectiveStatus status;
        public float currentValue;
        public float targetValue;
        public CareerReward reward;

        public bool IsCompleted
        {
            get { return status == ObjectiveStatus.Completed; }
        }

        public bool IsActive
        {
            get { return status == ObjectiveStatus.Active; }
        }

        public float ProgressNormalized
        {
            get
            {
                if (targetValue <= 0f) return 0f;
                return Mathf.Clamp01(currentValue / targetValue);
            }
        }

        public CareerObjective()
        {
            id = Guid.NewGuid().ToString();
            title = "Score a Half-Century";
            description = "Score 50 runs in any tournament match";
            type = ObjectiveType.ScoreFifty;
            status = ObjectiveStatus.Active;
            currentValue = 0f;
            targetValue = 50f;
            reward = new CareerReward(
                "REW_OBJ_50", 
                "Fifty Bonus", 
                "+10 Skill Points", 
                RewardType.SkillPoints, 
                10f, 
                string.Empty, 
                string.Empty);
        }

        public CareerObjective(
            string id,
            string title,
            string desc,
            ObjectiveType type,
            float target,
            CareerReward reward)
        {
            this.id = id;
            this.title = title;
            this.description = desc;
            this.type = type;
            this.status = ObjectiveStatus.Active;
            this.currentValue = 0f;
            this.targetValue = target;
            this.reward = reward;
        }

        public void SetProgress(float val)
        {
            if (status != ObjectiveStatus.Active) return;

            currentValue = val;
            if (currentValue >= targetValue)
            {
                Complete();
            }
        }

        public void AddProgress(float delta)
        {
            if (status != ObjectiveStatus.Active) return;

            currentValue += delta;
            if (currentValue >= targetValue)
            {
                Complete();
            }
        }

        public void Complete()
        {
            status = ObjectiveStatus.Completed;
            currentValue = targetValue;
        }

        public void Fail()
        {
            status = ObjectiveStatus.Failed;
        }

        public static CareerObjective CreateScore50Objective()
        {
            return new CareerObjective(
                "OBJ_SCORE_50",
                "Score 50 Runs",
                "Score 50 or more runs in a single innings",
                ObjectiveType.ScoreFifty,
                50f,
                new CareerReward("REW_50", "Half-Century Milestone", "+10 SP", RewardType.SkillPoints, 10f, "", ""));
        }

        public static CareerObjective CreateScore100Objective()
        {
            return new CareerObjective(
                "OBJ_SCORE_100",
                "Score 100 Runs",
                "Score a maiden century for your squad",
                ObjectiveType.ScoreHundred,
                100f,
                new CareerReward("REW_100", "Century Milestone", "+25 SP, +2 Batting", RewardType.AttributeIncrease, 2f, "Batting", ""));
        }

        public static CareerObjective CreateTake3WicketsObjective()
        {
            return new CareerObjective(
                "OBJ_TAKE_3_WKTS",
                "Take 3 Wickets",
                "Take 3 or more wickets in a single match",
                ObjectiveType.TakeThreeWickets,
                3f,
                new CareerReward("REW_3WKTS", "Bowling Spell Hero", "+15 SP", RewardType.SkillPoints, 15f, "", ""));
        }

        public static CareerObjective CreateWinMatchObjective()
        {
            return new CareerObjective(
                "OBJ_WIN_MATCH",
                "Lead Team to Victory",
                "Win the upcoming tournament fixture",
                ObjectiveType.WinMatch,
                1f,
                new CareerReward("REW_WIN", "Match Winner", "+5 Reputation", RewardType.Reputation, 5f, "", ""));
        }

        public static CareerObjective CreateCompleteTournamentObjective()
        {
            return new CareerObjective(
                "OBJ_COMPLETE_TOURNAMENT",
                "Complete the Tournament",
                "Play all scheduled fixtures in the current tournament",
                ObjectiveType.CompleteTournament,
                1f,
                new CareerReward("REW_TOUR_COMP", "Tournament Finished", "Unlock Next Tournament", RewardType.TournamentUnlock, 1f, "", "U19_WORLD_CUP"));
        }

        public static CareerObjective CreatePlayerOfMatchObjective()
        {
            return new CareerObjective(
                "OBJ_POTM",
                "Player of the Match",
                "Earn the Player of the Match award with an outstanding performance",
                ObjectiveType.EarnPlayerOfMatch,
                1f,
                new CareerReward("REW_POTM", "POTM Award", "+15 SP & +10 Reputation", RewardType.Reputation, 10f, "", ""));
        }
    }
}
