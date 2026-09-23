using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Batting;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Batting
{
    public class BattingShotButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private BattingShotType shotType = BattingShotType.StraightDrive;
        [SerializeField] private bool isLofted = false;
        [SerializeField] private Text label;

        public event Action<BattingShotType, bool> OnShotButtonClicked;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        public void Configure(BattingShotType type, bool lofted, string displayText)
        {
            shotType = type;
            isLofted = lofted;
            if (label != null)
            {
                label.text = displayText;
            }
        }

        public void HandleClicked()
        {
            if (OnShotButtonClicked != null)
            {
                OnShotButtonClicked(shotType, isLofted);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBattingShotSelection(shotType, isLofted);
            }
        }
    }
}
