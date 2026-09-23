using System;
using UnityEngine;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Fielding
{
    public class MobileFieldingHUD : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject hudContainer;

        [Header("Controls")]
        [SerializeField] private FieldingJoystick joystick;
        [SerializeField] private FieldingActionButtons actionButtons;
        [SerializeField] private ThrowTargetSelector targetSelector;

        public FieldingJoystick Joystick { get { return joystick; } }
        public FieldingActionButtons ActionButtons { get { return actionButtons; } }
        public ThrowTargetSelector TargetSelector { get { return targetSelector; } }

        public void SetVisible(bool visible)
        {
            if (hudContainer != null)
            {
                hudContainer.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
