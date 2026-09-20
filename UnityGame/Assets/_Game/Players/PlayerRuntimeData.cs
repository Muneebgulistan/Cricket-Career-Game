using System;
using UnityEngine;

namespace CricketGame.Players
{
    public enum PlayerMatchRole
    {
        Batsman,
        Bowler,
        AllRounder,
        WicketKeeper,
        Fielder,
        Umpire
    }

    [Serializable]
    public class PlayerRuntimeData
    {
        [Header("Position & Transform State")]
        public Vector3 currentPosition;
        public Quaternion currentRotation;

        [Header("Physical Match Condition")]
        public float currentStamina = 100f;
        public float currentHealth = 100f;

        [Header("Role & Control")]
        public PlayerMatchRole currentRole = PlayerMatchRole.Batsman;
        public bool isControlledByUser = false;
        public bool isAIControlled = true;

        [Header("Field State")]
        public bool isOnField = true;
        public bool isBatting = false;
        public bool isBowling = false;
        public bool isFielding = false;
        public bool isUmpire = false;

        public void InitializeState(PlayerMatchRole role, bool userControlled, bool aiControlled)
        {
            currentRole = role;
            isControlledByUser = userControlled;
            isAIControlled = aiControlled;
            currentStamina = 100f;
            currentHealth = 100f;
            isOnField = true;

            UpdateRoleFlags(role);
        }

        public void UpdateRoleFlags(PlayerMatchRole role)
        {
            currentRole = role;
            isBatting = (role == PlayerMatchRole.Batsman || role == PlayerMatchRole.AllRounder);
            isBowling = (role == PlayerMatchRole.Bowler);
            isFielding = (role == PlayerMatchRole.Fielder || role == PlayerMatchRole.WicketKeeper);
            isUmpire = (role == PlayerMatchRole.Umpire);
        }

        public void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Clamp(currentStamina - amount, 0f, 100f);
        }

        public void RecoverStamina(float amount)
        {
            currentStamina = Mathf.Clamp(currentStamina + amount, 0f, 100f);
        }
    }
}
