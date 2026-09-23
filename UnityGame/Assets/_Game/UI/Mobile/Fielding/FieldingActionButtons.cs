using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Fielding
{
    public class FieldingActionButtons : MonoBehaviour
    {
        [SerializeField] private Button catchButton;
        [SerializeField] private Button pickupButton;
        [SerializeField] private Button throwButton;
        [SerializeField] private Button diveButton;

        public event Action OnCatchClicked;
        public event Action OnPickupClicked;
        public event Action OnThrowClicked;
        public event Action OnDiveClicked;

        private void Awake()
        {
            if (catchButton != null) catchButton.onClick.AddListener(HandleCatch);
            if (pickupButton != null) pickupButton.onClick.AddListener(HandlePickup);
            if (throwButton != null) throwButton.onClick.AddListener(HandleThrow);
            if (diveButton != null) diveButton.onClick.AddListener(HandleDive);
        }

        public void HandleCatch()
        {
            if (OnCatchClicked != null) OnCatchClicked();
            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendFieldingCatch();
            }
        }

        public void HandlePickup()
        {
            if (OnPickupClicked != null) OnPickupClicked();
            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendFieldingPickup();
            }
        }

        public void HandleThrow()
        {
            if (OnThrowClicked != null) OnThrowClicked();
            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendFieldingTriggerThrow();
            }
        }

        public void HandleDive()
        {
            if (OnDiveClicked != null) OnDiveClicked();
            // Dive can be treated as rapid pickup / slide
            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendFieldingPickup();
            }
        }

        public void SetCatchAvailable(bool available)
        {
            if (catchButton != null) catchButton.interactable = available;
        }

        public void SetPickupAvailable(bool available)
        {
            if (pickupButton != null) pickupButton.interactable = available;
        }

        public void SetThrowAvailable(bool available)
        {
            if (throwButton != null) throwButton.interactable = available;
        }
    }
}
