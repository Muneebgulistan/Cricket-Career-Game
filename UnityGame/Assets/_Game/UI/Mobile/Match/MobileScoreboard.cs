using System;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Mobile.Match
{
    public class MobileScoreboard : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text teamScoreText;
        [SerializeField] private Text oversText;
        [SerializeField] private Text crrText;
        [SerializeField] private Text rrrText;
        [SerializeField] private Text targetText;
        [SerializeField] private GameObject powerplayBadge;

        public void UpdateScore(string teamName, int runs, int wickets, float overs, float maxOvers, float crr, int target, float rrr, bool inPowerplay)
        {
            if (teamScoreText != null)
            {
                teamScoreText.text = string.Format("{0} {1}/{2}", teamName, runs, wickets);
            }

            if (oversText != null)
            {
                oversText.text = string.Format("Ov: {0:F1}/{1}", overs, (int)maxOvers);
            }

            if (crrText != null)
            {
                crrText.text = string.Format("CRR: {0:F2}", crr);
            }

            if (targetText != null)
            {
                if (target > 0)
                {
                    targetText.gameObject.SetActive(true);
                    targetText.text = string.Format("Target: {0}", target);
                }
                else
                {
                    targetText.gameObject.SetActive(false);
                }
            }

            if (rrrText != null)
            {
                if (target > 0 && rrr > 0f)
                {
                    rrrText.gameObject.SetActive(true);
                    rrrText.text = string.Format("RRR: {0:F2}", rrr);
                }
                else
                {
                    rrrText.gameObject.SetActive(false);
                }
            }

            if (powerplayBadge != null)
            {
                powerplayBadge.SetActive(inPowerplay);
            }
        }
    }
}
