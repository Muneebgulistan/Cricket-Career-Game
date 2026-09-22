using System;
using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    public enum ContactQuality
    {
        SweetSpot,
        GoodContact,
        Edge,
        Miss
    }

    [Serializable]
    public class BattingContactPoint
    {
        [Header("Distance Thresholds from Sweet Spot (meters)")]
        public float sweetSpotRadius = 0.08f;
        public float goodContactRadius = 0.18f;
        public float edgeContactRadius = 0.28f;

        public ContactQuality EvaluateContact(Vector3 contactWorldPos, Vector3 sweetSpotWorldPos, out float distanceFromSweetSpot)
        {
            distanceFromSweetSpot = Vector3.Distance(contactWorldPos, sweetSpotWorldPos);

            if (distanceFromSweetSpot <= sweetSpotRadius)
            {
                return ContactQuality.SweetSpot;
            }
            if (distanceFromSweetSpot <= goodContactRadius)
            {
                return ContactQuality.GoodContact;
            }
            if (distanceFromSweetSpot <= edgeContactRadius)
            {
                return ContactQuality.Edge;
            }

            return ContactQuality.Miss;
        }

        public float GetContactMultiplier(ContactQuality quality)
        {
            switch (quality)
            {
                case ContactQuality.SweetSpot:
                    return 1.0f;
                case ContactQuality.GoodContact:
                    return 0.85f;
                case ContactQuality.Edge:
                    return 0.45f;
                default:
                    return 0.0f;
            }
        }
    }
}
