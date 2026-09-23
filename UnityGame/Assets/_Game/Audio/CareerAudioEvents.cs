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

        public static void PlayButtonClick()
        {
            if (OnButtonClick != null)
            {
                OnButtonClick();
            }
            else
            {
                Debug.Log("[CareerAudio] SFX: Button Click.");
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
                Debug.Log(string.Format("[CareerAudio] SFX: Match Result Fanfare ({0}).", isWin ? "Victory" : "Defeat"));
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
                Debug.Log("[CareerAudio] SFX: Career Promotion Trumpet Celebration!");
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
                Debug.Log("[CareerAudio] SFX: New Tournament Unlocked Chime!");
            }
        }
    }
}
