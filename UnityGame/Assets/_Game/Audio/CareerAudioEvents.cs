using System;
using UnityEngine;

namespace CricketGame.Audio
{
    public static class CareerAudioEvents
    {
        public static event Action OnButtonClick;
        public static event Action<bool> OnMatchResultSound;
        public static event Action OnPromotionSound;
        public static event Action OnTournamentUnlockSound;
        public static event Action OnMilestoneAchieved;

        public static void PlayButtonClick()
        {
            if (OnButtonClick != null)
            {
                OnButtonClick();
            }
            else
            {
                CricketGame.Core.CricketLogger.Log("[CareerAudio] SFX: Button Click.");
            }
        }

        public static void PlayMatchResultSound(bool isWin)
        {
            if (OnMatchResultSound != null)
            {
                OnMatchResultSound(isWin);
            }
            else
            {
                CricketGame.Core.CricketLogger.Log(string.Format("[CareerAudio] SFX: Match Result Fanfare ({0}).", isWin ? "Victory" : "Defeat"));
            }
        }

        public static void PlayPromotionSound()
        {
            if (OnPromotionSound != null)
            {
                OnPromotionSound();
            }
            else
            {
                CricketGame.Core.CricketLogger.Log("[CareerAudio] SFX: Career Promotion Trumpet Celebration!");
            }
        }

        public static void PlayTournamentUnlockSound()
        {
            if (OnTournamentUnlockSound != null)
            {
                OnTournamentUnlockSound();
            }
            else
            {
                CricketGame.Core.CricketLogger.Log("[CareerAudio] SFX: New Tournament Unlocked Chime!");
            }
        }

        public static void PlayMilestoneAchieved()
        {
            if (OnMilestoneAchieved != null)
            {
                OnMilestoneAchieved();
            }
            else
            {
                CricketGame.Core.CricketLogger.Log("[CareerAudio] SFX: Milestone Achieved!");
            }
        }
    }
}
