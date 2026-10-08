using System;
using UnityEngine;

namespace CricketGame.Audio
{
    /// <summary>
    /// Static event bus for in-match gameplay audio events.
    /// Gameplay systems raise these events. AudioManager (or any subscriber) reacts.
    /// No audio clips are required — the game runs fine with no subscribers.
    /// </summary>
    public static class GameplayAudioEvents
    {
        // --------------------------------------------------
        // Events
        // --------------------------------------------------
        public static event Action OnBatContact;
        public static event Action OnBoundaryFour;
        public static event Action OnBoundarySix;
        public static event Action OnWicketFall;
        public static event Action OnCatch;
        public static event Action OnRunOut;
        public static event Action OnDotBall;
        public static event Action OnMatchStart;
        public static event Action<bool> OnMatchEnd;  // true = player team won

        // --------------------------------------------------
        // Trigger Methods
        // --------------------------------------------------

        public static void TriggerBatContact()
        {
            if (OnBatContact != null) OnBatContact();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: Bat Contact.");
        }

        public static void TriggerBoundaryFour()
        {
            if (OnBoundaryFour != null) OnBoundaryFour();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: FOUR! Boundary.");
        }

        public static void TriggerBoundarySix()
        {
            if (OnBoundarySix != null) OnBoundarySix();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: SIX! Maximum!");
        }

        public static void TriggerWicketFall()
        {
            if (OnWicketFall != null) OnWicketFall();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: WICKET! Out!");
        }

        public static void TriggerCatch()
        {
            if (OnCatch != null) OnCatch();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: Caught!");
        }

        public static void TriggerRunOut()
        {
            if (OnRunOut != null) OnRunOut();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: Run Out!");
        }

        public static void TriggerDotBall()
        {
            if (OnDotBall != null) OnDotBall();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: Dot Ball.");
        }

        public static void TriggerMatchStart()
        {
            if (OnMatchStart != null) OnMatchStart();
            else CricketGame.Core.CricketLogger.Log("[GameplayAudio] SFX: Match Starting!");
        }

        public static void TriggerMatchEnd(bool playerTeamWon)
        {
            if (OnMatchEnd != null) OnMatchEnd(playerTeamWon);
            else CricketGame.Core.CricketLogger.Log(string.Format("[GameplayAudio] SFX: Match End ({0}).", playerTeamWon ? "Victory" : "Defeat"));
        }
    }
}
