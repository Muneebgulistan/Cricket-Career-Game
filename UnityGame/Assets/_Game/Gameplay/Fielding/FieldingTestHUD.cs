using UnityEngine;
using CricketGame.Gameplay.Ball;

namespace CricketGame.Gameplay.Fielding
{
    public class FieldingTestHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FieldingManager fieldingManager;
        [SerializeField] private SimpleCricketBall ball;
        [SerializeField] private FieldingInput fieldingInput;

        [Header("Display Settings")]
        [SerializeField] private bool showHUD = true;

        private void Awake()
        {
            if (fieldingManager == null) fieldingManager = FindFirstObjectByType<FieldingManager>();
            if (ball == null) ball = FindFirstObjectByType<SimpleCricketBall>();
            if (fieldingInput == null) fieldingInput = FindFirstObjectByType<FieldingInput>();
        }

        private void Update()
        {
            // Number key shortcuts 5-9 & 0 for scenarios
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5)) PlayScenario(1); // Point
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6)) PlayScenario(2); // Cover
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7)) PlayScenario(3); // MidWicket
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha8)) PlayScenario(4); // Catch
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha9)) PlayScenario(5); // LongOff
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0)) PlayScenario(6); // DeepMidWicket
        }

        private void PlayScenario(int idx)
        {
            if (fieldingManager != null)
            {
                fieldingManager.PlayScenario(idx);
            }
        }

        private void OnGUI()
        {
            if (!showHUD) return;

            // 1. Telemetry Box (Top Left)
            GUILayout.BeginArea(new Rect(10, 10, 360, 290), GUI.skin.box);
            GUILayout.Label("<b>CRICKET CAREER GAME — FIELDING SYSTEM (STEP 6)</b>");
            GUILayout.Space(4);

            FieldingController activeFielder = fieldingManager != null ? fieldingManager.ActivePrimaryFielder : null;
            FieldingResult lastRes = fieldingManager != null ? fieldingManager.LastResult : null;

            string fielderName = activeFielder != null ? activeFielder.GetFielderName() : "None";
            string fielderPos = activeFielder != null ? activeFielder.AssignedPosition.ToString() : "N/A";
            string stateStr = activeFielder != null ? activeFielder.CurrentState.ToString() : "Idle";
            float dist = activeFielder != null ? activeFielder.RuntimeData.distanceToBall : 0f;
            float speed = ball != null ? ball.SpeedKph : 0f;
            string throwTarget = activeFielder != null ? activeFielder.RuntimeData.selectedThrowTarget.ToString() : "WicketKeeper";

            float catchDiff = (activeFielder != null && activeFielder.RuntimeData.lastDecision != null) 
                ? activeFielder.RuntimeData.lastDecision.catchDifficulty 
                : 0f;

            GUILayout.Label(string.Format("Active Fielder: <b>{0} ({1})</b>", fielderName, fielderPos));
            GUILayout.Label(string.Format("Fielding State: <color=yellow><b>{0}</b></color>", stateStr));
            GUILayout.Label(string.Format("Distance to Ball: {0:F1} m", dist));
            GUILayout.Label(string.Format("Ball Speed: {0:F1} km/h", speed));
            GUILayout.Label(string.Format("Throw Target: <b>{0}</b>", throwTarget));

            if (catchDiff > 0.01f)
            {
                GUILayout.Label(string.Format("Catch Chance: <color=cyan><b>Difficulty {0:P0}</b></color>", catchDiff));
            }

            if (lastRes != null)
            {
                string outcomeColor = lastRes.outcome == FieldingOutcome.Catch || lastRes.outcome == FieldingOutcome.RunOut
                    ? "green" : (lastRes.outcome == FieldingOutcome.Four || lastRes.outcome == FieldingOutcome.Six ? "red" : "white");
                GUILayout.Label(string.Format("Outcome: <color={0}><b>{1}</b></color>", outcomeColor, lastRes.outcome));
                if (!string.IsNullOrEmpty(lastRes.summaryText))
                {
                    GUILayout.Label(string.Format("<i>{0}</i>", lastRes.summaryText));
                }
            }

            GUILayout.EndArea();

            // 2. Scenario Trigger Buttons (Top Right)
            GUILayout.BeginArea(new Rect(Screen.width - 240, 10, 230, 310), GUI.skin.box);
            GUILayout.Label("<b>TEST SCENARIOS</b>");
            GUILayout.Space(2);

            if (GUILayout.Button("1. Ground to Point (5)")) PlayScenario(1);
            if (GUILayout.Button("2. Ground to Cover (6)")) PlayScenario(2);
            if (GUILayout.Button("3. Ground to MidWicket (7)")) PlayScenario(3);
            if (GUILayout.Button("4. High Catch Chance (8)")) PlayScenario(4);
            if (GUILayout.Button("5. Ground to LongOff (9)")) PlayScenario(5);
            if (GUILayout.Button("6. Loft to DeepMidWkt (0)")) PlayScenario(6);
            if (GUILayout.Button("7. Glance to FineLeg")) PlayScenario(7);
            if (GUILayout.Button("8. Cut to ThirdMan")) PlayScenario(8);

            GUILayout.EndArea();

            // 3. Controls Help Box (Bottom Left)
            GUILayout.BeginArea(new Rect(10, Screen.height - 110, 480, 100), GUI.skin.box);
            GUILayout.Label("<b>FIELDING CONTROLS:</b>");
            GUILayout.Label("F: Activate | C: Attempt Catch | E: Attempt Pickup | T: Throw");
            GUILayout.Label("1: Throw to Keeper | 2: Bowler | 3: Striker End | 4: Non-Striker End");
            GUILayout.Label("WASD / Arrows: Manual Move | Shift: Sprint");
            GUILayout.EndArea();
        }
    }
}
