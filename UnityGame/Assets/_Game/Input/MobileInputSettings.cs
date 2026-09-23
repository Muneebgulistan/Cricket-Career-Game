using System;
using UnityEngine;

namespace CricketGame.MobileInput
{
    [Serializable]
    public class MobileInputSettings
    {
        [Header("Swipe Recognition")]
        public float minSwipeDistance = 50f;
        public float maxSwipeDuration = 0.4f;
        public float swipeDirectionThreshold = 0.5f;

        [Header("Virtual Joystick")]
        public float joystickDeadZone = 0.1f;
        public float joystickMaxRadius = 80f;
        public bool dynamicJoystick = true;

        [Header("Sensitivity & Power")]
        public float touchSensitivity = 1.0f;
        public float powerMeterSpeed = 1.5f;
        public float timingMeterSpeed = 1.0f;

        public static MobileInputSettings Default()
        {
            MobileInputSettings s = new MobileInputSettings();
            s.minSwipeDistance = 50f;
            s.maxSwipeDuration = 0.4f;
            s.swipeDirectionThreshold = 0.5f;
            s.joystickDeadZone = 0.1f;
            s.joystickMaxRadius = 80f;
            s.dynamicJoystick = true;
            s.touchSensitivity = 1.0f;
            s.powerMeterSpeed = 1.5f;
            s.timingMeterSpeed = 1.0f;
            return s;
        }
    }
}
