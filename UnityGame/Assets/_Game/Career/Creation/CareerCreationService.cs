using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Players;
using CricketGame.Career.Stages;
using CricketGame.Tournaments;

namespace CricketGame.Career.Creation
{
    public static class CareerCreationService
    {
        public const string DefaultUnder16TournamentId = "U16_NATIONAL_CUP";

        public static CareerProfile InitializeNewCareer(CareerCreationData data)
        {
            if (data == null)
            {
                data = CareerCreationData.CreateDefaultUnder16Prodigy();
            }

            // 1. Build PlayerProfile
            PlayerProfile player = new PlayerProfile(
                data.playerName,
                data.age,
                data.nationality,
                data.role,
                data.battingStyle,
                data.bowlingStyle
            );

            // Assign attributes
            player.battingRating = data.battingSkill;
            player.bowlingRating = data.bowlingSkill;
            player.fieldingRating = data.fieldingSkill;
            player.RecalculateOverallRating();
            player.currentTeam = "Lahore Eagles U-16";
            player.currentLevel = CareerLevel.Under16Cup;
            player.form = 75;
            player.fitness = 100;

            // 2. Build CareerProfile
            CareerProfile profile = new CareerProfile(player);
            profile.currentTournamentId = DefaultUnder16TournamentId;
            profile.unlockedTournaments.Clear();
            profile.unlockedTournaments.Add(DefaultUnder16TournamentId);

            // 3. Register to CareerManager if available
            if (CareerManager.Instance != null)
            {
                CareerManager.Instance.CreateNewCareer(player);
            }

            return profile;
        }

        public static bool CanEnterTournament(CareerProfile profile, string tournamentId)
        {
            if (profile == null || string.IsNullOrEmpty(tournamentId)) return false;

            if (profile.unlockedTournaments != null && profile.unlockedTournaments.Contains(tournamentId))
            {
                return true;
            }

            return false;
        }

        public static bool TrySelectTournament(CareerProfile profile, string tournamentId, out string failureReason)
        {
            failureReason = string.Empty;
            if (profile == null)
            {
                failureReason = "No active career profile.";
                return false;
            }

            if (!CanEnterTournament(profile, tournamentId))
            {
                failureReason = string.Format("Tournament '{0}' is currently locked. Advance your career stage to unlock it.", tournamentId);
                return false;
            }

            profile.currentTournamentId = tournamentId;
            return true;
        }
    }
}
