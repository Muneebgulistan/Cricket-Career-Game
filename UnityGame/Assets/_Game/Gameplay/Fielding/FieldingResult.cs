using System;
using UnityEngine;

namespace CricketGame.Gameplay.Fielding
{
    public enum FieldingOutcome
    {
        NoAction,
        BallStopped,
        CleanPickup,
        Fumble,
        Catch,
        DroppedCatch,
        MissedCatch,
        Throw,
        AccurateThrow,
        MissedThrow,
        RunOut,
        Safe,
        Boundary,
        Four,
        Six
    }

    [Serializable]
    public class FieldingResult
    {
        public FieldingOutcome outcome;
        public string fielderName;
        public FieldingPosition fielderPosition;
        public float distanceCovered;
        public float timeToPickup;
        public FieldingThrowData throwData;
        public bool wasRunOut;
        public int runsConceded;
        public bool isClean;
        public string summaryText;

        public FieldingResult()
        {
            outcome = FieldingOutcome.NoAction;
            fielderName = "Fielder";
            fielderPosition = FieldingPosition.Cover;
            distanceCovered = 0f;
            timeToPickup = 0f;
            throwData = null;
            wasRunOut = false;
            runsConceded = 0;
            isClean = true;
            summaryText = string.Empty;
        }

        public static FieldingResult CreateCatch(string fielder, FieldingPosition pos, bool clean)
        {
            FieldingResult r = new FieldingResult();
            r.outcome = clean ? FieldingOutcome.Catch : FieldingOutcome.DroppedCatch;
            r.fielderName = fielder;
            r.fielderPosition = pos;
            r.isClean = clean;
            r.runsConceded = clean ? 0 : 1;
            r.summaryText = clean ? string.Format("OUT! Magnificent catch by {0} at {1}!", fielder, pos)
                                  : string.Format("DROPPED! Catch put down by {0} at {1}!", fielder, pos);
            return r;
        }

        public static FieldingResult CreateGroundField(string fielder, FieldingPosition pos, bool clean, int runs)
        {
            FieldingResult r = new FieldingResult();
            r.outcome = clean ? FieldingOutcome.CleanPickup : FieldingOutcome.Fumble;
            r.fielderName = fielder;
            r.fielderPosition = pos;
            r.isClean = clean;
            r.runsConceded = runs;
            r.summaryText = clean ? string.Format("Clean pickup by {0} at {1}.", fielder, pos)
                                  : string.Format("Fumble by {0} at {1}!", fielder, pos);
            return r;
        }

        public static FieldingResult CreateBoundary(bool isSix, int runs)
        {
            FieldingResult r = new FieldingResult();
            r.outcome = isSix ? FieldingOutcome.Six : FieldingOutcome.Four;
            r.runsConceded = runs;
            r.isClean = false;
            r.summaryText = isSix ? "SIX! Over the boundary rope!" : "FOUR! Ball races across the boundary rope!";
            return r;
        }

        public static FieldingResult CreateRunOut(string fielder, FieldingPosition pos, bool isRunOut, int runs)
        {
            FieldingResult r = new FieldingResult();
            r.outcome = isRunOut ? FieldingOutcome.RunOut : FieldingOutcome.Safe;
            r.fielderName = fielder;
            r.fielderPosition = pos;
            r.wasRunOut = isRunOut;
            r.runsConceded = runs;
            r.summaryText = isRunOut ? string.Format("RUN OUT! Direct hit / throw from {0} at {1} breaks the stumps!", fielder, pos)
                                     : string.Format("Batsman makes ground safely ({0} runs).", runs);
            return r;
        }
    }
}
