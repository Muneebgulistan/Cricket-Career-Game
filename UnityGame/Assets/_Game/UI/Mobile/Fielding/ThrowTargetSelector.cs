using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Fielding;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Fielding
{
    public class ThrowTargetSelector : MonoBehaviour
    {
        [SerializeField] private Button keeperButton;
        [SerializeField] private Button bowlerButton;
        [SerializeField] private Button strikerEndButton;
        [SerializeField] private Button nonStrikerEndButton;

        private FieldingTarget selectedTarget = FieldingTarget.WicketKeeper;

        public FieldingTarget SelectedTarget { get { return selectedTarget; } }
        public event Action<FieldingTarget> OnTargetChanged;

        private void Awake()
        {
            if (keeperButton != null) keeperButton.onClick.AddListener(SelectKeeper);
            if (bowlerButton != null) bowlerButton.onClick.AddListener(SelectBowler);
            if (strikerEndButton != null) strikerEndButton.onClick.AddListener(SelectStrikerEnd);
            if (nonStrikerEndButton != null) nonStrikerEndButton.onClick.AddListener(SelectNonStrikerEnd);
        }

        public void SelectKeeper() { ApplyTarget(FieldingTarget.WicketKeeper); }
        public void SelectBowler() { ApplyTarget(FieldingTarget.Bowler); }
        public void SelectStrikerEnd() { ApplyTarget(FieldingTarget.StrikerEnd); }
        public void SelectNonStrikerEnd() { ApplyTarget(FieldingTarget.NonStrikerEnd); }

        public void ApplyTarget(FieldingTarget target)
        {
            selectedTarget = target;
            if (OnTargetChanged != null)
            {
                OnTargetChanged(selectedTarget);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendFieldingThrowTarget(selectedTarget);
            }
        }
    }
}
