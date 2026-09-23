using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.UI.Match
{
    public class MatchScoreboard : MonoBehaviour
    {
        [Header("Top Scoreboard UI Text")]
        [SerializeField] private Text teamScoreText;
        [SerializeField] private Text oversText;
        [SerializeField] private Text runRateText;
        [SerializeField] private Text targetText;
        [SerializeField] private GameObject powerplayBadge;

        public void UpdateDisplay(InningsScore innings, bool isPowerplay)
        {
            if (innings == null) return;

            if (teamScoreText != null)
            {
                teamScoreText.text = string.Format("{0}: {1}/{2}", innings.battingTeam, innings.totalRuns, innings.wickets);
            }

            if (oversText != null)
            {
                oversText.text = string.Format("Overs: {0} / {1}", innings.OversFormatted, innings.maxOvers);
            }

            if (runRateText != null)
            {
                if (innings.targetScore > 0)
                {
                    runRateText.text = string.Format("CRR: {0:F2} | RRR: {1:F2}", innings.CurrentRunRate, innings.RequiredRunRate);
                }
                else
                {
                    runRateText.text = string.Format("CRR: {0:F2}", innings.CurrentRunRate);
                }
            }

            if (targetText != null)
            {
                if (innings.targetScore > 0)
                {
                    targetText.gameObject.SetActive(true);
                    targetText.text = string.Format("Target: {0} (Need {1} runs in {2} balls)", 
                        innings.targetScore, 
                        innings.RunsNeeded, 
                        innings.RemainingBalls);
                }
                else
                {
                    targetText.gameObject.SetActive(false);
                }
            }

            if (powerplayBadge != null)
            {
                powerplayBadge.SetActive(isPowerplay);
            }
        }
    }
}
