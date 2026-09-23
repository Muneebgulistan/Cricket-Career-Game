using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Bowling;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Bowling
{
    public class BowlingLineControl : MonoBehaviour
    {
        [SerializeField] private Button outsideOffButton;
        [SerializeField] private Button offStumpButton;
        [SerializeField] private Button middleStumpButton;
        [SerializeField] private Button legStumpButton;

        private BowlingLine selectedLine = BowlingLine.MiddleStump;

        public BowlingLine SelectedLine { get { return selectedLine; } }
        public event Action<BowlingLine> OnLineChanged;

        private void Awake()
        {
            if (outsideOffButton != null) outsideOffButton.onClick.AddListener(SelectOutsideOff);
            if (offStumpButton != null) offStumpButton.onClick.AddListener(SelectOffStump);
            if (middleStumpButton != null) middleStumpButton.onClick.AddListener(SelectMiddleStump);
            if (legStumpButton != null) legStumpButton.onClick.AddListener(SelectLegStump);
        }

        public void SelectOutsideOff() { ApplyLine(BowlingLine.OutsideOff); }
        public void SelectOffStump() { ApplyLine(BowlingLine.OffStump); }
        public void SelectMiddleStump() { ApplyLine(BowlingLine.MiddleStump); }
        public void SelectLegStump() { ApplyLine(BowlingLine.LegStump); }

        public void ApplyLine(BowlingLine line)
        {
            selectedLine = line;
            if (OnLineChanged != null)
            {
                OnLineChanged(selectedLine);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBowlingLine(selectedLine);
            }
        }
    }
}
