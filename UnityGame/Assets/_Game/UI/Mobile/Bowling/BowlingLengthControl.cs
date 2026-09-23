using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Bowling;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Bowling
{
    public class BowlingLengthControl : MonoBehaviour
    {
        [SerializeField] private Button yorkerButton;
        [SerializeField] private Button fullButton;
        [SerializeField] private Button goodLengthButton;
        [SerializeField] private Button shortButton;
        [SerializeField] private Button bouncerButton;

        private BowlingLength selectedLength = BowlingLength.GoodLength;

        public BowlingLength SelectedLength { get { return selectedLength; } }
        public event Action<BowlingLength> OnLengthChanged;

        private void Awake()
        {
            if (yorkerButton != null) yorkerButton.onClick.AddListener(SelectYorker);
            if (fullButton != null) fullButton.onClick.AddListener(SelectFull);
            if (goodLengthButton != null) goodLengthButton.onClick.AddListener(SelectGoodLength);
            if (shortButton != null) shortButton.onClick.AddListener(SelectShort);
            if (bouncerButton != null) bouncerButton.onClick.AddListener(SelectBouncer);
        }

        public void SelectYorker() { ApplyLength(BowlingLength.Yorker); }
        public void SelectFull() { ApplyLength(BowlingLength.Full); }
        public void SelectGoodLength() { ApplyLength(BowlingLength.GoodLength); }
        public void SelectShort() { ApplyLength(BowlingLength.Short); }
        public void SelectBouncer() { ApplyLength(BowlingLength.Bouncer); }

        public void ApplyLength(BowlingLength length)
        {
            selectedLength = length;
            if (OnLengthChanged != null)
            {
                OnLengthChanged(selectedLength);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBowlingLength(selectedLength);
            }
        }
    }
}
