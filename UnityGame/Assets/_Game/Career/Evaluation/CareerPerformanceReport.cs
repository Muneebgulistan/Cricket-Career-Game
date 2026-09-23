using System;
using System.Collections.Generic;

namespace CricketGame.Career.Evaluation
{
    [Serializable]
    public class CareerPerformanceReport
    {
        public float battingScore;             // 0 to 100
        public float bowlingScore;             // 0 to 100
        public float fieldingScore;            // 0 to 100
        public float matchContributionScore;   // 0 to 100
        public float tournamentContributionScore; // 0 to 100
        public float overallMatchRating;       // 1.0 to 10.0
        public bool isPlayerOfTheMatch;
        public string performanceGrade;        // "A+", "A", "B", "C", "D"
        public string summary;
        public List<string> highlights;

        public CareerPerformanceReport()
        {
            battingScore = 0f;
            bowlingScore = 0f;
            fieldingScore = 0f;
            matchContributionScore = 0f;
            tournamentContributionScore = 0f;
            overallMatchRating = 5.0f;
            isPlayerOfTheMatch = false;
            performanceGrade = "C";
            summary = "Solid appearance with standard contribution.";
            highlights = new List<string>();
        }
    }
}
