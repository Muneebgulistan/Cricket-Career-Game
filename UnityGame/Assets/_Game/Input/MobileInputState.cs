using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Fielding;
using CricketGame.Gameplay.Running;

namespace CricketGame.MobileInput
{
    public class MobileInputState
    {
        // Batting State
        public Vector2 BattingDirection;
        public BattingShotType SelectedShotType;
        public bool IsBattingLofted;
        public bool IsBattingDefensive;
        public bool IsSwingTriggered;
        public float BattingPower;

        // Bowling State
        public BowlingBaseType SelectedBowlingType;
        public BowlingLine SelectedBowlingLine;
        public BowlingLength SelectedBowlingLength;
        public float BowlingSwingAmount;
        public float BowlingSpinAmount;
        public float BowlingPower;
        public bool IsDeliveryTriggered;

        // Running State
        public bool IsRunRequested;
        public bool IsDiveRequested;
        public bool IsCancelRunRequested;

        // Fielding State
        public Vector2 FieldingMoveDirection;
        public bool WantsFieldingSprint;
        public bool IsPickupRequested;
        public bool IsCatchRequested;
        public bool IsThrowRequested;
        public FieldingTarget SelectedThrowTarget;
        public bool IsFieldingDiveRequested;

        public MobileInputState()
        {
            Reset();
        }

        public void Reset()
        {
            BattingDirection = Vector2.zero;
            SelectedShotType = BattingShotType.StraightDrive;
            IsBattingLofted = false;
            IsBattingDefensive = false;
            IsSwingTriggered = false;
            BattingPower = 1.0f;

            SelectedBowlingType = BowlingBaseType.Fast;
            SelectedBowlingLine = BowlingLine.MiddleStump;
            SelectedBowlingLength = BowlingLength.GoodLength;
            BowlingSwingAmount = 0f;
            BowlingSpinAmount = 0f;
            BowlingPower = 1.0f;
            IsDeliveryTriggered = false;

            IsRunRequested = false;
            IsDiveRequested = false;
            IsCancelRunRequested = false;

            FieldingMoveDirection = Vector2.zero;
            WantsFieldingSprint = false;
            IsPickupRequested = false;
            IsCatchRequested = false;
            IsThrowRequested = false;
            SelectedThrowTarget = FieldingTarget.WicketKeeper;
            IsFieldingDiveRequested = false;
        }
    }
}
