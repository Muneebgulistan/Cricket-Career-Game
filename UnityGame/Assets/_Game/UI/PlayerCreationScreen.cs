using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Cricket;
using CricketGame.Players;
using CricketGame.Career;
using CricketGame.SaveSystem;

namespace CricketGame.UI
{
    /// <summary>
    /// Player creation screen allowing customization of name, role, nationality,
    /// batting style, and bowling style, with automatic save profile creation.
    /// </summary>
    public class PlayerCreationScreen : MonoBehaviour
    {
        [Header("UI Form Inputs")]
        [SerializeField] private InputField nameInputField;
        [SerializeField] private Dropdown roleDropdown;
        [SerializeField] private Dropdown nationalityDropdown;
        [SerializeField] private Dropdown battingStyleDropdown;
        [SerializeField] private Dropdown bowlingStyleDropdown;
        [SerializeField] private Text errorFeedbackText;

        [Header("Action Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button randomizeButton;

        // In-memory form state (for programmatic control & unit tests)
        private string characterName = "Muneeb Gulistan";
        private PlayingRole characterRole = PlayingRole.Batsman;
        private string characterNationality = "Pakistan";
        private BattingStyle characterBattingStyle = BattingStyle.RightHand;
        private BowlingStyle characterBowlingStyle = BowlingStyle.RightArmFast;
        private int characterAge = 16;

        public string CharacterName { get { return characterName; } set { characterName = value; } }
        public PlayingRole CharacterRole { get { return characterRole; } set { characterRole = value; } }
        public string CharacterNationality { get { return characterNationality; } set { characterNationality = value; } }
        public BattingStyle CharacterBattingStyle { get { return characterBattingStyle; } set { characterBattingStyle = value; } }
        public BowlingStyle CharacterBowlingStyle { get { return characterBowlingStyle; } set { characterBowlingStyle = value; } }
        public int CharacterAge { get { return characterAge; } set { characterAge = value; } }

        public event Action<CareerProfile> OnCareerCreated;
        public event Action OnCreationCancelled;

        private void Start()
        {
            SetupListeners();
            SyncFormToUI();
        }

        private void SetupListeners()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
            if (backButton != null) backButton.onClick.AddListener(OnCancelClicked);
            if (randomizeButton != null) randomizeButton.onClick.AddListener(OnRandomizeClicked);
        }

        public void SetPlayerDetails(string name, PlayingRole role, string nationality, BattingStyle batStyle, BowlingStyle bowlStyle, int age = 16)
        {
            characterName = name;
            characterRole = role;
            characterNationality = nationality;
            characterBattingStyle = batStyle;
            characterBowlingStyle = bowlStyle;
            characterAge = age;
            SyncFormToUI();
        }

        public void SyncFormToUI()
        {
            if (nameInputField != null) nameInputField.text = characterName;
            if (roleDropdown != null) roleDropdown.value = (int)characterRole;
            if (battingStyleDropdown != null) battingStyleDropdown.value = (int)characterBattingStyle;
            if (bowlingStyleDropdown != null) bowlingStyleDropdown.value = (int)characterBowlingStyle;
        }

        public void ReadFromUI()
        {
            if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
            {
                characterName = nameInputField.text.Trim();
            }

            if (roleDropdown != null && Enum.IsDefined(typeof(PlayingRole), roleDropdown.value))
            {
                characterRole = (PlayingRole)roleDropdown.value;
            }

            if (battingStyleDropdown != null && Enum.IsDefined(typeof(BattingStyle), battingStyleDropdown.value))
            {
                characterBattingStyle = (BattingStyle)battingStyleDropdown.value;
            }

            if (bowlingStyleDropdown != null && Enum.IsDefined(typeof(BowlingStyle), bowlingStyleDropdown.value))
            {
                characterBowlingStyle = (BowlingStyle)bowlingStyleDropdown.value;
            }
        }

        public bool ValidateForm(out string errorMessage)
        {
            ReadFromUI();

            if (string.IsNullOrEmpty(characterName) || characterName.Trim().Length < 2)
            {
                errorMessage = "Player name must be at least 2 characters long.";
                if (errorFeedbackText != null) errorFeedbackText.text = errorMessage;
                return false;
            }

            if (characterAge < 14 || characterAge > 30)
            {
                errorMessage = "Player age must be between 14 and 30 for junior debut.";
                if (errorFeedbackText != null) errorFeedbackText.text = errorMessage;
                return false;
            }

            errorMessage = string.Empty;
            if (errorFeedbackText != null) errorFeedbackText.text = string.Empty;
            return true;
        }

        public PlayerProfile CreatePlayerProfile()
        {
            PlayerProfile profile = new PlayerProfile(
                characterName,
                characterAge,
                characterNationality,
                characterRole,
                characterBattingStyle,
                characterBowlingStyle
            );

            // Tailor initial attributes according to chosen cricket role
            switch (characterRole)
            {
                case PlayingRole.Batsman:
                    profile.battingRating = 65;
                    profile.bowlingRating = 35;
                    profile.fieldingRating = 55;
                    profile.overallRating = 60;
                    break;
                case PlayingRole.Bowler:
                    profile.battingRating = 35;
                    profile.bowlingRating = 65;
                    profile.fieldingRating = 55;
                    profile.overallRating = 60;
                    break;
                case PlayingRole.AllRounder:
                    profile.battingRating = 55;
                    profile.bowlingRating = 55;
                    profile.fieldingRating = 55;
                    profile.overallRating = 58;
                    break;
                case PlayingRole.WicketKeeper:
                    profile.battingRating = 60;
                    profile.bowlingRating = 20;
                    profile.fieldingRating = 70;
                    profile.isWicketKeeper = true;
                    profile.overallRating = 62;
                    break;
            }

            profile.fitness = 100;
            profile.form = 75;
            profile.currentTeam = "Lahore Eagles U-16";
            profile.currentLevel = CareerLevel.Under16Cup;

            return profile;
        }

        public CareerProfile CreateCareerAndSave(string slotName = null)
        {
            string error;
            if (!ValidateForm(out error))
            {
                CricketGame.Core.CricketLogger.LogWarning(string.Format("[PlayerCreationScreen] Validation failed: {0}", error));
                return null;
            }

            PlayerProfile player = CreatePlayerProfile();
            CareerProfile career = new CareerProfile(player);

            // Save to SaveProfileManager (secure local persistence)
            if (SaveProfileManager.Instance != null)
            {
                SaveProfileManager.Instance.SaveProfile(career, slotName);
            }

            // Also register in existing CareerManager if present
            if (CareerManager.Instance != null)
            {
                CareerManager.Instance.CreateNewCareer(player);
            }

            if (OnCareerCreated != null)
            {
                OnCareerCreated(career);
            }

            return career;
        }

        public void OnConfirmClicked()
        {
            CareerProfile career = CreateCareerAndSave();
            if (career != null)
            {
                if (AppFlowManager.Instance != null)
                {
                    AppFlowManager.Instance.OnPlayerCreationComplete(career.player);
                }
                else if (UIManager.Instance != null)
                {
                    UIManager.Instance.PopPanel();
                }
            }
        }

        public void OnCancelClicked()
        {
            if (OnCreationCancelled != null) OnCreationCancelled();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.PopPanel();
            }
        }

        public void OnRandomizeClicked()
        {
            string[] firstNames = { "Muneeb", "Babar", "Shaheen", "Naseem", "Fakhar", "Haris", "Shadab", "Rizwan" };
            string[] lastNames = { "Gulistan", "Azam", "Afridi", "Shah", "Zaman", "Rauf", "Khan", "Ahmed" };

            string randomName = firstNames[UnityEngine.Random.Range(0, firstNames.Length)] + " " +
                               lastNames[UnityEngine.Random.Range(0, lastNames.Length)];

            characterName = randomName;
            characterRole = (PlayingRole)UnityEngine.Random.Range(0, 4);
            characterBattingStyle = (BattingStyle)UnityEngine.Random.Range(0, 2);
            SyncFormToUI();
        }
    }
}
