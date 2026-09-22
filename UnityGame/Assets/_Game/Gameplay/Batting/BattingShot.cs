using System;
using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    [Serializable]
    public class BattingShot
    {
        public BattingShotType shotType;
        public string shotName;
        public float directionAngle; // Degrees relative to straight forward (0 = Straight, negative = Off-side, positive = Leg-side for RHB)
        public float basePower;      // 0 - 100
        public float elevationAngle; // Degrees above horizontal ground plane
        public float timingWindow;   // Ideal timing window tolerance in seconds
        public float risk;           // 0.0 - 1.0 (probability factor for edges/mis-hits)
        public ContactQuality preferredContactRegion;
        public bool isLofted;

        public BattingShot()
        {
            shotType = BattingShotType.StraightDrive;
            shotName = "Straight Drive";
            directionAngle = 0f;
            basePower = 65f;
            elevationAngle = 6f;
            timingWindow = 0.15f;
            risk = 0.15f;
            preferredContactRegion = ContactQuality.SweetSpot;
            isLofted = false;
        }

        public BattingShot(BattingShotType type, string name, float dirAngle, float power, float elevation, float window, float riskFactor, ContactQuality contact, bool lofted)
        {
            shotType = type;
            shotName = name;
            directionAngle = dirAngle;
            basePower = power;
            elevationAngle = elevation;
            timingWindow = window;
            risk = riskFactor;
            preferredContactRegion = contact;
            isLofted = lofted;
        }

        public static BattingShot CreateDefault(BattingShotType type)
        {
            switch (type)
            {
                case BattingShotType.Defensive:
                    return new BattingShot(BattingShotType.Defensive, "Defensive", 0f, 15f, 0f, 0.22f, 0.05f, ContactQuality.GoodContact, false);
                case BattingShotType.StraightDrive:
                    return new BattingShot(BattingShotType.StraightDrive, "Straight Drive", 0f, 70f, 6f, 0.16f, 0.15f, ContactQuality.SweetSpot, false);
                case BattingShotType.CoverDrive:
                    return new BattingShot(BattingShotType.CoverDrive, "Cover Drive", -35f, 75f, 7f, 0.15f, 0.18f, ContactQuality.SweetSpot, false);
                case BattingShotType.OnDrive:
                    return new BattingShot(BattingShotType.OnDrive, "On Drive", 30f, 70f, 6f, 0.16f, 0.18f, ContactQuality.SweetSpot, false);
                case BattingShotType.SquareDrive:
                    return new BattingShot(BattingShotType.SquareDrive, "Square Drive", -65f, 72f, 8f, 0.14f, 0.22f, ContactQuality.SweetSpot, false);
                case BattingShotType.Cut:
                    return new BattingShot(BattingShotType.Cut, "Square Cut", -80f, 78f, 10f, 0.13f, 0.25f, ContactQuality.SweetSpot, false);
                case BattingShotType.Pull:
                    return new BattingShot(BattingShotType.Pull, "Pull Shot", 75f, 82f, 18f, 0.14f, 0.28f, ContactQuality.SweetSpot, false);
                case BattingShotType.Hook:
                    return new BattingShot(BattingShotType.Hook, "Hook Shot", 95f, 88f, 28f, 0.12f, 0.38f, ContactQuality.SweetSpot, true);
                case BattingShotType.Sweep:
                    return new BattingShot(BattingShotType.Sweep, "Sweep Shot", 70f, 60f, 4f, 0.16f, 0.20f, ContactQuality.GoodContact, false);
                case BattingShotType.LoftedDrive:
                    return new BattingShot(BattingShotType.LoftedDrive, "Lofted Drive", -20f, 92f, 32f, 0.12f, 0.35f, ContactQuality.SweetSpot, true);
                case BattingShotType.StraightLoft:
                    return new BattingShot(BattingShotType.StraightLoft, "Straight Loft", 0f, 95f, 35f, 0.12f, 0.35f, ContactQuality.SweetSpot, true);
                case BattingShotType.Miss:
                default:
                    return new BattingShot(BattingShotType.Miss, "Missed Shot", 0f, 0f, 0f, 0f, 1f, ContactQuality.Miss, false);
            }
        }
    }
}
