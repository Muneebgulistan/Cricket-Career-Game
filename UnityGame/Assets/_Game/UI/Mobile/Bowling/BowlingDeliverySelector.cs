using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Cricket;
using CricketGame.Gameplay.Bowling;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Bowling
{
    public class BowlingDeliverySelector : MonoBehaviour
    {
        [SerializeField] private Button fastButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button offSpinButton;
        [SerializeField] private Button legSpinButton;

        private BowlingBaseType selectedType = BowlingBaseType.Fast;

        public BowlingBaseType SelectedType { get { return selectedType; } }
        public event Action<BowlingBaseType> OnDeliveryTypeSelected;

        private void Awake()
        {
            if (fastButton != null) fastButton.onClick.AddListener(SelectFast);
            if (mediumButton != null) mediumButton.onClick.AddListener(SelectMedium);
            if (offSpinButton != null) offSpinButton.onClick.AddListener(SelectOffSpin);
            if (legSpinButton != null) legSpinButton.onClick.AddListener(SelectLegSpin);
        }

        public void SelectFast() { ApplyType(BowlingBaseType.Fast); }
        public void SelectMedium() { ApplyType(BowlingBaseType.Medium); }
        public void SelectOffSpin() { ApplyType(BowlingBaseType.OffSpin); }
        public void SelectLegSpin() { ApplyType(BowlingBaseType.LegSpin); }

        public void ApplyType(BowlingBaseType type)
        {
            selectedType = type;
            if (OnDeliveryTypeSelected != null)
            {
                OnDeliveryTypeSelected(selectedType);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBowlingDeliveryType(selectedType);
            }
        }
    }
}
