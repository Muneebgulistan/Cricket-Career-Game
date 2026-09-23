using System;
using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Career.Rewards
{
    public enum RewardType
    {
        AttributeIncrease,
        SkillPoints,
        Reputation,
        TournamentUnlock,
        NewTeamOpportunity,
        NewMatchType,
        CosmeticUnlock
    }

    [Serializable]
    public class CareerReward
    {
        public string rewardId;
        public string title;
        public string description;
        public RewardType type;
        public float amount;
        public string targetAttribute;
        public string unlockTargetId;
        public bool isClaimed;

        public CareerReward()
        {
            rewardId = Guid.NewGuid().ToString();
            title = "Skill Points Bonus";
            description = "+5 Skill Points";
            type = RewardType.SkillPoints;
            amount = 5f;
            targetAttribute = string.Empty;
            unlockTargetId = string.Empty;
            isClaimed = false;
        }

        public CareerReward(string id, string title, string desc, RewardType type, float amount, string targetAttr, string unlockId)
        {
            this.rewardId = id;
            this.title = title;
            this.description = desc;
            this.type = type;
            this.amount = amount;
            this.targetAttribute = targetAttr ?? string.Empty;
            this.unlockTargetId = unlockId ?? string.Empty;
            this.isClaimed = false;
        }

        public bool Apply(CareerProfile profile)
        {
            if (profile == null || isClaimed) return false;

            switch (type)
            {
                case RewardType.AttributeIncrease:
                    if (profile.player != null)
                    {
                        if (string.Equals(targetAttribute, "Batting", StringComparison.OrdinalIgnoreCase))
                        {
                            profile.player.battingRating = Mathf.Clamp(profile.player.battingRating + (int)amount, 10, 99);
                        }
                        else if (string.Equals(targetAttribute, "Bowling", StringComparison.OrdinalIgnoreCase))
                        {
                            profile.player.bowlingRating = Mathf.Clamp(profile.player.bowlingRating + (int)amount, 10, 99);
                        }
                        else if (string.Equals(targetAttribute, "Fitness", StringComparison.OrdinalIgnoreCase))
                        {
                            profile.player.fitness = Mathf.Clamp(profile.player.fitness + (int)amount, 10, 100);
                        }
                        else if (string.Equals(targetAttribute, "Fielding", StringComparison.OrdinalIgnoreCase))
                        {
                            profile.player.fieldingRating = Mathf.Clamp(profile.player.fieldingRating + (int)amount, 10, 99);
                        }
                        else
                        {
                            profile.player.overallRating = Mathf.Clamp(profile.player.overallRating + (int)amount, 10, 99);
                        }
                    }
                    break;

                case RewardType.SkillPoints:
                    profile.skillPoints += (int)amount;
                    break;

                case RewardType.Reputation:
                    profile.reputation = Mathf.Clamp(profile.reputation + amount, 0f, 100f);
                    break;

                case RewardType.TournamentUnlock:
                    if (!string.IsNullOrEmpty(unlockTargetId))
                    {
                        if (profile.unlockedTournaments == null)
                        {
                            profile.unlockedTournaments = new System.Collections.Generic.List<string>();
                        }
                        if (!profile.unlockedTournaments.Contains(unlockTargetId))
                        {
                            profile.unlockedTournaments.Add(unlockTargetId);
                        }
                    }
                    break;

                case RewardType.NewTeamOpportunity:
                case RewardType.NewMatchType:
                case RewardType.CosmeticUnlock:
                    // Data-driven placeholders recorded in claimed rewards
                    break;
            }

            if (profile.claimedRewards == null)
            {
                profile.claimedRewards = new System.Collections.Generic.List<string>();
            }
            if (!profile.claimedRewards.Contains(rewardId))
            {
                profile.claimedRewards.Add(rewardId);
            }

            isClaimed = true;
            return true;
        }
    }
}
