using System;
using System.Collections.Generic;
using UnityEngine;

namespace CricketGame.Fielding
{
    public class CricketPositionManager : MonoBehaviour
    {
        public static CricketPositionManager Instance { get; private set; }

        [Header("Key Gameplay Anchors")]
        public Transform pitchCenter;
        public Transform bowlerStartPosition;
        public Transform bowlerReleasePosition;
        public Transform strikerPosition;
        public Transform nonStrikerPosition;
        public Transform wicketKeeperPosition;
        public Transform umpireBowlerEndPosition;
        public Transform umpireBatsmanEndPosition;

        [Header("Ground & Boundary References")]
        public float boundaryRadius = 70.0f;
        public Transform boundaryCenter;

        [Header("Fielding Positions (20 Named Anchors)")]
        public Transform bowlerAnchor;
        public Transform wicketKeeperAnchor;
        public Transform slip1Anchor;
        public Transform slip2Anchor;
        public Transform slip3Anchor;
        public Transform gullyAnchor;
        public Transform pointAnchor;
        public Transform coverAnchor;
        public Transform extraCoverAnchor;
        public Transform midOffAnchor;
        public Transform midOnAnchor;
        public Transform midWicketAnchor;
        public Transform squareLegAnchor;
        public Transform fineLegAnchor;
        public Transform thirdManAnchor;
        public Transform longOffAnchor;
        public Transform longOnAnchor;
        public Transform deepCoverAnchor;
        public Transform deepMidWicketAnchor;
        public Transform deepSquareLegAnchor;

        private Dictionary<string, Transform> fielderAnchorMap;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                BuildAnchorDictionary();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void BuildAnchorDictionary()
        {
            fielderAnchorMap = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase)
            {
                { "Bowler", bowlerAnchor },
                { "WicketKeeper", wicketKeeperAnchor },
                { "Slip1", slip1Anchor },
                { "Slip2", slip2Anchor },
                { "Slip3", slip3Anchor },
                { "Gully", gullyAnchor },
                { "Point", pointAnchor },
                { "Cover", coverAnchor },
                { "ExtraCover", extraCoverAnchor },
                { "MidOff", midOffAnchor },
                { "MidOn", midOnAnchor },
                { "MidWicket", midWicketAnchor },
                { "SquareLeg", squareLegAnchor },
                { "FineLeg", fineLegAnchor },
                { "ThirdMan", thirdManAnchor },
                { "LongOff", longOffAnchor },
                { "LongOn", longOnAnchor },
                { "DeepCover", deepCoverAnchor },
                { "DeepMidWicket", deepMidWicketAnchor },
                { "DeepSquareLeg", deepSquareLegAnchor }
            };
        }

        public Transform GetFielderAnchor(string positionName)
        {
            if (fielderAnchorMap == null)
            {
                BuildAnchorDictionary();
            }

            Transform anchor = null;
            if (fielderAnchorMap != null && fielderAnchorMap.TryGetValue(positionName, out anchor))
            {
                return anchor;
            }
            return null;
        }

        public bool IsInsideBoundary(Vector3 position)
        {
            Vector3 center = boundaryCenter != null ? boundaryCenter.position : Vector3.zero;
            Vector2 pos2D = new Vector2(position.x - center.x, position.z - center.z);
            return pos2D.magnitude <= boundaryRadius;
        }

        public string GetClosestFielderPositionName(Vector3 ballPosition)
        {
            if (fielderAnchorMap == null) BuildAnchorDictionary();

            string closestName = "Bowler";
            float closestDistSqr = float.MaxValue;

            foreach (var kvp in fielderAnchorMap)
            {
                if (kvp.Value != null)
                {
                    float distSqr = (kvp.Value.position - ballPosition).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                        closestName = kvp.Key;
                    }
                }
            }

            return closestName;
        }
    }
}
