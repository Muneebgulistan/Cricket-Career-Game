using System;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Mobile.Match
{
    public class MobileTargetDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject bannerContainer;
        [SerializeField] private Text equationText;

        public void UpdateTargetEquation(string teamName, int runsNeeded, int ballsRemaining)
        {
            if (bannerContainer == null || equationText == null) return;

            if (runsNeeded > 0 && ballsRemaining >= 0)
            {
                bannerContainer.SetActive(true);
                equationText.text = string.Format("{0} need {1} runs in {2} balls", teamName, runsNeeded, ballsRemaining);
            }
            else
            {
                bannerContainer.SetActive(false);
            }
        }

        public void Hide()
        {
            if (bannerContainer != null)
            {
                bannerContainer.SetActive(false);
            }
        }
    }
}
