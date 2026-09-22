using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;

namespace CricketGame.Gameplay.Bowling
{
    public class BowlingTestHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BowlingController bowler;
        [SerializeField] private BowlingInput bowlingInput;
        [SerializeField] private SimpleCricketBall ball;

        private void Awake()
        {
            if (bowler == null) bowler = FindFirstObjectByType<BowlingController>();
            if (bowlingInput == null) bowlingInput = FindFirstObjectByType<BowlingInput>();
            if (ball == null) ball = FindFirstObjectByType<SimpleCricketBall>();
        }

        private void OnGUI()
        {
            // Bowling Dashboard (Top Left)
            GUI.Box(new Rect(10, 10, 320, 270), "CRICKET BOWLING CONTROLS (STEP 5)");

            GUILayout.BeginArea(new Rect(20, 35, 300, 240));
            {
                string stateStr = bowler != null ? bowler.CurrentState.ToString() : "N/A";
                GUILayout.Label(string.Format("Bowler State: <b>{0}</b>", stateStr));

                string typeStr = bowlingInput != null ? bowlingInput.SelectedType.ToString() : "Fast";
                string lengthStr = bowlingInput != null ? bowlingInput.SelectedLength.ToString() : "GoodLength";
                string lineStr = bowlingInput != null ? bowlingInput.SelectedLine.ToString() : "OffStump";
                GUILayout.Label(string.Format("Selected: <b>{0}</b> | Length: <b>{1}</b>", typeStr, lengthStr));
                GUILayout.Label(string.Format("Target Line: <b>{0}</b>", lineStr));

                float swing = bowlingInput != null ? bowlingInput.SwingAmount : 0f;
                float spin = bowlingInput != null ? bowlingInput.SpinAmount : 0f;
                GUILayout.Label(string.Format("Swing: <b>{0:+0.00;-0.00;0.00}</b> | Spin: <b>{1:+0.0;-0.0;0.0}°</b>", swing, spin));

                float acc = bowlingInput != null ? bowlingInput.AccuracyMeter : 0.85f;
                GUILayout.Label(string.Format("Accuracy Meter: <b>{0:P0}</b>", acc));

                string ballStateStr = ball != null ? ball.CurrentState.ToString() : "Held";
                float speedKph = ball != null ? ball.SpeedKph : 0f;
                GUILayout.Label(string.Format("Ball State: <b>{0}</b> ({1:F1} km/h)", ballStateStr, speedKph));

                GUILayout.Space(5);
                GUILayout.Label("--- LAST DELIVERY RESULT ---");
                if (bowler != null && bowler.LastResult != null)
                {
                    BowlingResult r = bowler.LastResult;
                    GUILayout.Label(string.Format("Speed: <b>{0:F1} km/h</b> | Pitch Z: <b>{1:F2}m</b>", r.speedKph, r.bouncePosition.z));
                    GUILayout.Label(string.Format("Hit By Bat: <b>{0}</b> | Valid: <b>{1}</b>", r.wasHitByBatsman ? "<color=green>YES</color>" : "NO", r.wasValidDelivery));
                }
                else
                {
                    GUILayout.Label("Ready to bowl. Press <b>[SPACE]</b> to begin run-up!");
                }
            }
            GUILayout.EndArea();

            // Controls Guide (Bottom Left)
            GUI.Box(new Rect(10, Screen.height - 200, 320, 190), "BOWLING KEYBOARD CONTROLS");
            GUILayout.BeginArea(new Rect(20, Screen.height - 175, 300, 160));
            {
                GUILayout.Label("• <b>[SPACE]</b> : Begin Delivery Run-Up");
                GUILayout.Label("• <b>[1, 2, 3, 4]</b> : Fast, Medium, Off-Spin, Leg-Spin");
                GUILayout.Label("• <b>[Q, W, E, R, T]</b> : Yorker, Full, Good, Short, Bouncer");
                GUILayout.Label("• <b>[A / D]</b> : Inswing / Outswing Curve");
                GUILayout.Label("• <b>[← / →]</b> : Shift Target Line (Off / Leg)");
                GUILayout.Label("• <b>[1, 2, 3]</b> : Broadcast / Batting / Bowling Camera");
            }
            GUILayout.EndArea();
        }
    }
}
