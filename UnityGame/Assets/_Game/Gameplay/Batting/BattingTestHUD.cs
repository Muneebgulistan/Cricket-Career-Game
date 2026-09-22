using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;

namespace CricketGame.Gameplay.Batting
{
    public class BattingTestHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattingController batsman;
        [SerializeField] private TestDeliveryGenerator deliveryGenerator;
        [SerializeField] private SimpleCricketBall ball;

        [Header("Feedback Flash")]
        private float feedbackTimer = 0f;
        private string feedbackText = "";
        private Color feedbackColor = Color.white;

        private void Awake()
        {
            if (batsman == null) batsman = FindFirstObjectByType<BattingController>();
            if (deliveryGenerator == null) deliveryGenerator = FindFirstObjectByType<TestDeliveryGenerator>();
            if (ball == null) ball = FindFirstObjectByType<SimpleCricketBall>();
        }

        private void Start()
        {
            if (batsman != null)
            {
                batsman.OnShotExecuted += HandleShotExecuted;
            }
        }

        private void OnDestroy()
        {
            if (batsman != null)
            {
                batsman.OnShotExecuted -= HandleShotExecuted;
            }
        }

        private void Update()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
            }
        }

        private void HandleShotExecuted(BattingResult result)
        {
            feedbackTimer = 2.5f;

            if (result.isMiss)
            {
                feedbackText = "MISS!";
                feedbackColor = new Color(0.9f, 0.2f, 0.2f);
            }
            else
            {
                switch (result.timing)
                {
                    case TimingQuality.Perfect:
                        feedbackText = "PERFECT TIMING!";
                        feedbackColor = new Color(0.1f, 0.95f, 0.2f);
                        break;
                    case TimingQuality.Good:
                        feedbackText = "GOOD TIMING";
                        feedbackColor = new Color(0.2f, 0.8f, 1.0f);
                        break;
                    case TimingQuality.Early:
                        feedbackText = "EARLY";
                        feedbackColor = new Color(0.95f, 0.85f, 0.1f);
                        break;
                    case TimingQuality.Late:
                        feedbackText = "LATE";
                        feedbackColor = new Color(1.0f, 0.55f, 0.1f);
                        break;
                    default:
                        feedbackText = "MISS";
                        feedbackColor = Color.red;
                        break;
                }

                if (result.contactQuality == ContactQuality.SweetSpot)
                {
                    feedbackText += " ★ SWEET SPOT";
                }
                else if (result.isEdge)
                {
                    feedbackText += " (EDGE)";
                }
            }
        }

        private void OnGUI()
        {
            // Title Header & Delivery Info Panel
            GUI.Box(new Rect(10, 10, 310, 240), "CRICKET BATTING PROTOTYPE (STEP 4)");

            GUILayout.BeginArea(new Rect(20, 35, 290, 210));
            {
                string stateStr = batsman != null ? batsman.CurrentState.ToString() : "N/A";
                GUILayout.Label(string.Format("Batsman State: <b>{0}</b>", stateStr));

                string shotStr = batsman != null ? batsman.RuntimeData.currentShotType.ToString() : "N/A";
                GUILayout.Label(string.Format("Selected Shot: <b>{0}</b>", shotStr));

                string deliveryStr = deliveryGenerator != null ? deliveryGenerator.SelectedDelivery.ToString() : "Straight";
                float speed = deliveryGenerator != null ? deliveryGenerator.DeliverySpeedKph : 135f;
                GUILayout.Label(string.Format("Delivery Type: <b>{0}</b> ({1:F0} km/h)", deliveryStr, speed));

                GUILayout.Space(5);
                GUILayout.Label("--- LAST SHOT METRICS ---");

                if (batsman != null && batsman.LastResult != null)
                {
                    BattingResult r = batsman.LastResult;
                    GUILayout.Label(string.Format("Timing: <b>{0}</b> ({1:+0.000;-0.000;0.000}s)", r.timing, r.timingDelta));
                    GUILayout.Label(string.Format("Contact: <b>{0}</b> (SweetSpot: {1:F2}m)", r.contactQuality, r.distanceFromSweetSpot));
                    GUILayout.Label(string.Format("Exit Speed: <b>{0:F1} km/h</b>", r.exitSpeedKph));
                    GUILayout.Label(string.Format("Direction: <b>{0:F1}°</b> | Angle: <b>{1:F1}°</b>", r.directionAngle, r.launchAngle));
                    GUILayout.Label(string.Format("Est. Distance: <b>{0:F1} meters</b>", r.distanceEstimate));
                    GUILayout.Label(string.Format("Estimated Runs: <b><color=yellow>{0}</color></b>", r.estimatedRuns));
                }
                else
                {
                    GUILayout.Label("Awaiting delivery... (Press [R] to bowl)");
                }
            }
            GUILayout.EndArea();

            // Center Visual Feedback Badge
            if (feedbackTimer > 0f)
            {
                GUIStyle badgeStyle = new GUIStyle(GUI.skin.box);
                badgeStyle.fontSize = 22;
                badgeStyle.fontStyle = FontStyle.Bold;
                badgeStyle.alignment = TextAnchor.MiddleCenter;
                badgeStyle.normal.textColor = feedbackColor;

                float width = 360f;
                float height = 50f;
                float x = (Screen.width - width) * 0.5f;
                float y = 70f;

                GUI.Box(new Rect(x, y, width, height), feedbackText, badgeStyle);
            }

            // Controls Help Overlay (Bottom Left)
            GUI.Box(new Rect(10, Screen.height - 180, 310, 170), "CONTROLS GUIDE");
            GUILayout.BeginArea(new Rect(20, Screen.height - 155, 290, 140));
            {
                GUILayout.Label("• <b>[SPACE]</b> : Swing Bat");
                GUILayout.Label("• <b>[W / S / A / D]</b> : Loft / Defend / Leg / Off");
                GUILayout.Label("• <b>[1 - 7]</b> : Quick Shot Select (Def, Drive, Pull, etc.)");
                GUILayout.Label("• <b>[R]</b> : Bowl Test Ball | <b>[T]</b> : Change Delivery");
                GUILayout.Label("• <b>[F1 - F5]</b> : Straight, Inswing, Outswing, Short, Full");
                GUILayout.Label("• <b>[1, 2, 3]</b> : Broadcast / Batting / Bowling Camera");
            }
            GUILayout.EndArea();
        }
    }
}
