using System;
using System.Collections.Generic;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Tournaments;

namespace CricketGame.Career.Evaluation
{
    public class CareerPerformanceEvaluator
    {
        public static CareerPerformanceReport EvaluateMatch(
            PlayerMatchPerformance performance,
            MatchResult matchResult,
            TournamentProgress tournament)
        {
            CareerPerformanceReport report = new CareerPerformanceReport();
            if (performance == null) return report;

            bool isWin = (matchResult != null && matchResult.isPlayerVictory);

            // 1. Batting Score (0 to 100)
            float batScore = 0f;
            if (performance.runs > 0 || performance.balls > 0)
            {
                float runPoints = Mathf.Min(performance.runs * 0.7f, 50f);
                float sr = performance.balls > 0 ? ((float)performance.runs / performance.balls) * 100f : 0f;
                float srBonus = Mathf.Clamp((sr - 100f) * 0.2f, -10f, 20f);
                float boundaryBonus = (performance.fours * 2f) + (performance.sixes * 4f);
                float notOutBonus = (!performance.isOut && performance.runs >= 20) ? 10f : 0f;

                batScore = Mathf.Clamp(runPoints + srBonus + boundaryBonus + notOutBonus, 0f, 100f);

                if (performance.runs >= 100)
                {
                    report.highlights.Add(string.Format("Magnificent Century: {0} off {1} balls", performance.runs, performance.balls));
                }
                else if (performance.runs >= 50)
                {
                    report.highlights.Add(string.Format("Crucial Half-Century: {0} off {1} balls", performance.runs, performance.balls));
                }
                else if (performance.runs >= 30)
                {
                    report.highlights.Add(string.Format("Valuable innings of {0} runs", performance.runs));
                }
            }
            report.battingScore = batScore;

            // 2. Bowling Score (0 to 100)
            float bowlScore = 0f;
            if (performance.oversBowled > 0f)
            {
                float wktPoints = Mathf.Min(performance.wickets * 18f, 60f);
                float econ = performance.runsConceded / performance.oversBowled;
                float econBonus = Mathf.Clamp((8.0f - econ) * 3f, -15f, 20f);
                float maidenBonus = performance.maidens * 10f;

                bowlScore = Mathf.Clamp(wktPoints + econBonus + maidenBonus + 10f, 0f, 100f);

                if (performance.wickets >= 5)
                {
                    report.highlights.Add(string.Format("Fifer Heroics: {0}/{1} in {2:F1} overs", performance.wickets, performance.runsConceded, performance.oversBowled));
                }
                else if (performance.wickets >= 3)
                {
                    report.highlights.Add(string.Format("Three-wicket haul: {0}/{1}", performance.wickets, performance.runsConceded));
                }
                else if (performance.maidens > 0)
                {
                    report.highlights.Add(string.Format("Disciplined bowling with {0} maiden over(s)", performance.maidens));
                }
            }
            report.bowlingScore = bowlScore;

            // 3. Fielding Score (0 to 100)
            float fieldScore = 0f;
            int totalDismissals = performance.catches + performance.runOuts + performance.stumpings;
            if (totalDismissals > 0)
            {
                fieldScore = Mathf.Clamp((performance.catches * 25f) + (performance.runOuts * 35f) + (performance.stumpings * 30f), 0f, 100f);
                if (performance.catches > 0)
                {
                    report.highlights.Add(string.Format("Held {0} catch(es)", performance.catches));
                }
                if (performance.runOuts > 0)
                {
                    report.highlights.Add(string.Format("Executed {0} run-out dismissal(s)", performance.runOuts));
                }
            }
            report.fieldingScore = fieldScore;

            // 4. Match Contribution Score (0 to 100)
            float contrib = 0f;
            int disciplinesPlayed = 0;
            if (performance.runs > 0 || performance.balls > 0) disciplinesPlayed++;
            if (performance.oversBowled > 0f) disciplinesPlayed++;

            if (disciplinesPlayed == 1)
            {
                // Single discipline specialist (e.g. Pure batsman or Pure bowler)
                float mainScore = (performance.oversBowled > 0f) ? bowlScore : batScore;
                contrib = (mainScore * 0.85f) + (fieldScore * 0.15f);
            }
            else if (disciplinesPlayed >= 2)
            {
                // All-rounder
                float maxDiscipline = Mathf.Max(batScore, bowlScore);
                float minDiscipline = Mathf.Min(batScore, bowlScore);
                contrib = (maxDiscipline * 0.55f) + (minDiscipline * 0.35f) + (fieldScore * 0.10f);
            }
            else
            {
                contrib = fieldScore > 0f ? fieldScore : 25f;
            }

            if (isWin) contrib = Mathf.Min(contrib + 10f, 100f);
            report.matchContributionScore = Mathf.Clamp(contrib, 0f, 100f);

            // 5. Tournament Contribution Score (0 to 100)
            float tourContrib = report.matchContributionScore;
            if (tournament != null && tournament.matchesPlayed > 0)
            {
                float avgRuns = tournament.matchesPlayed > 0 ? (float)tournament.playerRunsScored / tournament.matchesPlayed : 0f;
                float tourPoints = Mathf.Clamp((avgRuns * 1.2f) + (tournament.playerWicketsTaken * 8f), 0f, 100f);
                tourContrib = (report.matchContributionScore * 0.5f) + (tourPoints * 0.5f);
            }
            report.tournamentContributionScore = Mathf.Clamp(tourContrib, 0f, 100f);

            // 6. Overall Match Rating (1.0 to 10.0)
            float calculatedRating = 2.0f + (report.matchContributionScore * 0.08f);
            if (performance.matchRating > 0f)
            {
                // Harmonize with performance tracker rating if present
                calculatedRating = (calculatedRating * 0.5f) + (performance.matchRating * 0.5f);
            }
            report.overallMatchRating = Mathf.Clamp((float)Math.Round(calculatedRating, 1), 1.0f, 10.0f);

            // 7. Player of the Match
            bool potm = (report.overallMatchRating >= 8.5f) ||
                        (performance.runs >= 50 && isWin && report.overallMatchRating >= 7.5f) ||
                        (performance.wickets >= 3 && isWin && report.overallMatchRating >= 7.5f);
            report.isPlayerOfTheMatch = potm;
            if (potm)
            {
                report.highlights.Insert(0, "PLAYER OF THE MATCH AWARD");
            }

            // 8. Performance Grade
            if (report.overallMatchRating >= 9.0f) report.performanceGrade = "A+";
            else if (report.overallMatchRating >= 7.5f) report.performanceGrade = "A";
            else if (report.overallMatchRating >= 6.0f) report.performanceGrade = "B";
            else if (report.overallMatchRating >= 4.5f) report.performanceGrade = "C";
            else report.performanceGrade = "D";

            // 9. Narrative Summary
            if (potm)
            {
                report.summary = "Match-winning masterclass! Outstanding match rating and pivotal impact.";
            }
            else if (report.overallMatchRating >= 7.5f)
            {
                report.summary = "Superb performance! Contributed decisively to the squad's effort.";
            }
            else if (report.overallMatchRating >= 6.0f)
            {
                report.summary = "Dependable outing with solid execution under pressure.";
            }
            else
            {
                report.summary = "Modest return. Looking to bounce back stronger in the next fixture.";
            }

            return report;
        }
    }
}
