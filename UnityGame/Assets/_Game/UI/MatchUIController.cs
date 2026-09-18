using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay;

namespace CricketGame.UI
{
    public class MatchUIController : MonoBehaviour
    {
        [Header("Scoreboard Elements")]
        [SerializeField] private Text teamScoreText;
        [SerializeField] private Text oversText;
        [SerializeField] private Text strikerText;
        [SerializeField] private Text bowlerText;
        [SerializeField] private Text runRateText;

        [Header("Control Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button endMatchButton;

        [Header("Pause Overlay")]
        [SerializeField] private GameObject pausePanel;

        private void Start()
        {
            SetupListeners();
            UpdateScoreboardDisplay();
        }

        private void SetupListeners()
        {
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (endMatchButton != null) endMatchButton.onClick.AddListener(OnEndMatchClicked);
        }

        public void UpdateScoreboardDisplay()
        {
            MatchManager mm = MatchManager.Instance;
            if (mm != null)
            {
                if (teamScoreText != null) 
                    teamScoreText.text = $"{mm.battingTeam}: {mm.totalRuns}/{mm.wicketsLost}";
                
                if (oversText != null) 
                    oversText.text = $"Overs: {mm.currentOvers:F1}/{mm.maxOvers}";

                if (strikerText != null) 
                    strikerText.text = $"{mm.strikerName} {mm.strikerRuns} ({mm.strikerBalls})";

                if (bowlerText != null) 
                    bowlerText.text = $"{mm.bowlerName} ({mm.bowlerFigures})";

                if (runRateText != null)
                {
                    float crr = mm.currentOvers > 0 ? (mm.totalRuns / mm.currentOvers) : 0f;
                    runRateText.text = $"CRR: {crr:F2}";
                }
            }
        }

        public void OnPauseClicked()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
            MatchManager.Instance?.PauseMatch();
        }

        public void OnResumeClicked()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            MatchManager.Instance?.ResumeMatch();
        }

        public void OnEndMatchClicked()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            MatchManager.Instance?.EndMatchAndReturnToHub(true);
        }
    }
}
