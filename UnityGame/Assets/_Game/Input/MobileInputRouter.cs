using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Fielding;
using CricketGame.Gameplay.Running;

namespace CricketGame.MobileInput
{
    public class MobileInputRouter
    {
        private IBattingInput battingInput;
        private IBowlingInput bowlingInput;
        private IFieldingInput fieldingInput;
        private IRunningInput runningInput;

        private MobileInputState state;

        public MobileInputState State { get { return state; } }

        public MobileInputRouter(
            IBattingInput batting,
            IBowlingInput bowling,
            IFieldingInput fielding,
            IRunningInput running)
        {
            battingInput = batting;
            bowlingInput = bowling;
            fieldingInput = fielding;
            runningInput = running;
            state = new MobileInputState();
        }

        public void BindBattingInput(IBattingInput batting)
        {
            battingInput = batting;
        }

        public void BindBowlingInput(IBowlingInput bowling)
        {
            bowlingInput = bowling;
        }

        public void BindFieldingInput(IFieldingInput fielding)
        {
            fieldingInput = fielding;
        }

        public void BindRunningInput(IRunningInput running)
        {
            runningInput = running;
        }

        // ----------------------------------------------------
        // Batting Mobile Routing
        // ----------------------------------------------------
        public void SendBattingDirection(Vector2 direction)
        {
            state.BattingDirection = direction;
            if (battingInput != null)
            {
                battingInput.SetShotDirection(direction);
            }
        }

        public void SendBattingShotSelection(BattingShotType shotType, bool isLofted)
        {
            state.SelectedShotType = shotType;
            state.IsBattingLofted = isLofted;
            state.IsBattingDefensive = (shotType == BattingShotType.Defensive);
            if (battingInput != null)
            {
                battingInput.SetShotModifier(isLofted, state.IsBattingDefensive);
            }
        }

        public void SendBattingSwing()
        {
            state.IsSwingTriggered = true;
            if (battingInput != null)
            {
                battingInput.TriggerShot(state.SelectedShotType, state.BattingDirection, state.IsBattingLofted);
            }
        }

        public void SendDirectShot(BattingShotType shotType, Vector2 direction, bool isLofted)
        {
            state.SelectedShotType = shotType;
            state.BattingDirection = direction;
            state.IsBattingLofted = isLofted;
            state.IsBattingDefensive = (shotType == BattingShotType.Defensive);
            state.IsSwingTriggered = true;

            if (battingInput != null)
            {
                battingInput.TriggerShot(shotType, direction, isLofted);
            }
        }

        // ----------------------------------------------------
        // Bowling Mobile Routing
        // ----------------------------------------------------
        public void SendBowlingDeliveryType(BowlingBaseType baseType)
        {
            state.SelectedBowlingType = baseType;
            if (bowlingInput != null)
            {
                bowlingInput.SelectDelivery(baseType);
            }
        }

        public void SendBowlingLine(BowlingLine line)
        {
            state.SelectedBowlingLine = line;
            if (bowlingInput != null)
            {
                bowlingInput.SetTargetLine(line);
            }
        }

        public void SendBowlingLength(BowlingLength length)
        {
            state.SelectedBowlingLength = length;
            if (bowlingInput != null)
            {
                bowlingInput.SetTargetLength(length);
            }
        }

        public void SendBowlingSwing(float swing)
        {
            state.BowlingSwingAmount = swing;
            if (bowlingInput != null)
            {
                bowlingInput.SetSwing(swing);
            }
        }

        public void SendBowlingSpin(float spin)
        {
            state.BowlingSpinAmount = spin;
            if (bowlingInput != null)
            {
                bowlingInput.SetSpin(spin);
            }
        }

        public void SendBowlingPower(float power01)
        {
            state.BowlingPower = power01;
            if (bowlingInput != null)
            {
                bowlingInput.SetPower(power01);
            }
        }

        public void SendBowlingTriggerDelivery()
        {
            state.IsDeliveryTriggered = true;
            if (bowlingInput != null)
            {
                bowlingInput.TriggerDelivery();
            }
        }

        // ----------------------------------------------------
        // Running Mobile Routing
        // ----------------------------------------------------
        public void SendRunRequest()
        {
            state.IsRunRequested = true;
            if (runningInput != null)
            {
                runningInput.RequestRun();
            }
        }

        public void SendDiveRequest()
        {
            state.IsDiveRequested = true;
            if (runningInput != null)
            {
                runningInput.RequestDive();
            }
        }

        public void SendCancelRunRequest()
        {
            state.IsCancelRunRequested = true;
            if (runningInput != null)
            {
                runningInput.RequestCancel();
            }
        }

        // ----------------------------------------------------
        // Fielding Mobile Routing
        // ----------------------------------------------------
        public void SendFieldingMove(Vector2 direction, bool sprint)
        {
            state.FieldingMoveDirection = direction;
            state.WantsFieldingSprint = sprint;
            if (fieldingInput != null)
            {
                fieldingInput.MoveFieldPlayer(direction, sprint);
            }
        }

        public void SendFieldingPickup()
        {
            state.IsPickupRequested = true;
            if (fieldingInput != null)
            {
                fieldingInput.AttemptPickup();
            }
        }

        public void SendFieldingCatch()
        {
            state.IsCatchRequested = true;
            if (fieldingInput != null)
            {
                fieldingInput.AttemptCatch();
            }
        }

        public void SendFieldingThrowTarget(FieldingTarget target)
        {
            state.SelectedThrowTarget = target;
            if (fieldingInput != null)
            {
                fieldingInput.SelectThrowTarget(target);
            }
        }

        public void SendFieldingTriggerThrow()
        {
            state.IsThrowRequested = true;
            if (fieldingInput != null)
            {
                fieldingInput.TriggerThrow();
            }
        }

        public void SendFieldingActivation(bool activate)
        {
            if (fieldingInput != null)
            {
                fieldingInput.ActivateFielding(activate);
            }
        }
    }
}
