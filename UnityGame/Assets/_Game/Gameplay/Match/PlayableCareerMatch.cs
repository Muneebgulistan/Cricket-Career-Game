using System.Collections;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Career.MatchIntegration;

namespace CricketGame.Gameplay.Match
{
    /// <summary>Self-contained, touch friendly playable match used by the main Match scene.</summary>
    public sealed class PlayableCareerMatch : MonoBehaviour
    {
        private const int BallsPerInnings = 12;
        private int runs, wickets, balls, opponentRuns, opponentWickets;
        private string playerTeam = "Lahore Eagles U-16", opponent = "Karachi Kings U-16";
        private string commentary = "Choose a shot when the bowler starts the delivery.";
        private bool finished, ballInFlight;
        private GameObject ball;
        private GUIStyle titleStyle, bodyStyle, buttonStyle, panelStyle;
        private Camera matchCamera;
        private MatchResult pendingResult;

        private void Start()
        {
            var context = CareerMatchLauncher.Instance != null ? CareerMatchLauncher.Instance.ActiveContext : null;
            if (context != null)
            {
                if (!string.IsNullOrEmpty(context.playerTeamName)) playerTeam = context.playerTeamName;
                if (!string.IsNullOrEmpty(context.opponentTeamName)) opponent = context.opponentTeamName;
            }
            BuildPitch();
        }

        private void BuildPitch()
        {
            matchCamera = Camera.main;
            if (matchCamera == null) matchCamera = new GameObject("Match Camera").AddComponent<Camera>();
            matchCamera.tag = "MainCamera";
            matchCamera.transform.position = new Vector3(0, 10, -17);
            matchCamera.transform.rotation = Quaternion.Euler(24, 0, 0);
            matchCamera.fieldOfView = 55;
            matchCamera.clearFlags = CameraClearFlags.SolidColor;
            matchCamera.backgroundColor = new Color(.32f, .57f, .72f);
            RenderSettings.ambientLight = new Color(.72f, .74f, .68f);

            MakePrimitive(PrimitiveType.Plane, "Oval", new Vector3(0, -.15f, 4), new Vector3(5.5f, 1, 5.5f), new Color(.16f, .42f, .20f));
            MakePrimitive(PrimitiveType.Cube, "Pitch", new Vector3(0, -.02f, 0), new Vector3(2.5f, .12f, 12), new Color(.67f, .49f, .30f));
            MakePrimitive(PrimitiveType.Cube, "Crease", new Vector3(0, .05f, -4.2f), new Vector3(2.7f, .02f, .08f), Color.white);
            MakePrimitive(PrimitiveType.Cube, "Bowling crease", new Vector3(0, .05f, 4.2f), new Vector3(2.7f, .02f, .08f), Color.white);
            for (int i = -1; i <= 1; i++)
            {
                MakePrimitive(PrimitiveType.Cylinder, "Stump", new Vector3(i * .16f, .42f, -4.05f), new Vector3(.045f, .42f, .045f), new Color(.92f, .86f, .68f));
                MakePrimitive(PrimitiveType.Cylinder, "Stump", new Vector3(i * .16f, .42f, 4.05f), new Vector3(.045f, .42f, .045f), new Color(.92f, .86f, .68f));
            }
            MakePrimitive(PrimitiveType.Capsule, "Batter", new Vector3(0, .85f, -3.3f), new Vector3(.65f, .9f, .65f), new Color(.14f, .29f, .78f));
            MakePrimitive(PrimitiveType.Capsule, "Bowler", new Vector3(.25f, .85f, 3.1f), new Vector3(.62f, .9f, .62f), new Color(.77f, .18f, .16f));
            var bat = MakePrimitive(PrimitiveType.Cube, "Bat", new Vector3(.45f, .9f, -3.35f), new Vector3(.14f, .85f, .22f), new Color(.55f, .31f, .12f));
            bat.transform.rotation = Quaternion.Euler(0, 0, -18);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                MakePrimitive(PrimitiveType.Sphere, "Fielding teammate", new Vector3(Mathf.Sin(angle) * 8, .7f, Mathf.Cos(angle) * 8), new Vector3(.7f, 1.25f, .7f), new Color(.77f, .18f, .16f));
            }
            if (FindObjectOfType<Light>() == null)
            {
                var light = new GameObject("Stadium Sun").AddComponent<Light>();
                light.type = LightType.Directional; light.intensity = 1.15f;
                light.transform.rotation = Quaternion.Euler(48, -32, 0);
            }
        }

        private GameObject MakePrimitive(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type); go.name = objectName; go.transform.position = position; go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
            return go;
        }

        private void EnsureStyles()
        {
            if (buttonStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = Color.white } };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
            panelStyle = new GUIStyle(GUI.skin.box);
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Min(Screen.width / 900f, Screen.height / 650f);
            Matrix4x4 old = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float w = Screen.width / scale, h = Screen.height / scale;
            GUI.Box(new Rect(18, 16, w - 36, 100), GUIContent.none, panelStyle);
            GUI.Label(new Rect(30, 20, w - 60, 42), playerTeam + "  " + runs + "/" + wickets, titleStyle);
            GUI.Label(new Rect(30, 62, w - 60, 40), "2 OVER MATCH     " + (balls / 6) + "." + (balls % 6) + " / 2 overs", bodyStyle);
            GUI.Box(new Rect(w * .16f, h - 174, w * .68f, 154), GUIContent.none, panelStyle);
            GUI.Label(new Rect(w * .18f, h - 168, w * .64f, 48), commentary, bodyStyle);
            if (finished)
            {
                if (GUI.Button(new Rect(w * .35f, h - 112, w * .3f, 62), "CONTINUE", buttonStyle))
                    CareerMatchCompletionPipeline.ProcessMatchResult(pendingResult);
            }
            else
            {
                GUI.enabled = !ballInFlight;
                float gap = 10, bw = (w * .64f - 2 * gap) / 3;
                if (GUI.Button(new Rect(w * .18f, h - 112, bw, 62), "DEFEND", buttonStyle)) PlayShot(0);
                if (GUI.Button(new Rect(w * .18f + bw + gap, h - 112, bw, 62), "DRIVE", buttonStyle)) PlayShot(1);
                if (GUI.Button(new Rect(w * .18f + (bw + gap) * 2, h - 112, bw, 62), "LOFT", buttonStyle)) PlayShot(2);
                GUI.enabled = true;
            }
            GUI.matrix = old;
        }

        private void PlayShot(int shot)
        {
            if (ballInFlight || finished) return;
            ballInFlight = true;
            ball = MakePrimitive(PrimitiveType.Sphere, "Match ball", new Vector3(0, 1.1f, 3.6f), Vector3.one * .26f, new Color(.78f, .08f, .06f));
            StartCoroutine(ResolveShot(shot));
        }

        private IEnumerator ResolveShot(int shot)
        {
            Vector3 start = ball.transform.position, end = new Vector3(0, .55f, -3.1f);
            for (float t = 0; t < 1; t += Time.deltaTime * 1.5f)
            {
                if (ball == null) yield break;
                ball.transform.position = Vector3.Lerp(start, end, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 1.2f;
                yield return null;
            }
            int roll = Random.Range(0, 100);
            int scored = 0;
            bool wicketThisBall;
            if (shot == 0) { scored = roll < 76 ? 0 : roll < 96 ? 1 : 2; wicketThisBall = roll >= 98; }
            else if (shot == 1) { scored = roll < 18 ? 0 : roll < 52 ? 1 : roll < 72 ? 2 : roll < 89 ? 4 : roll < 96 ? 6 : 0; wicketThisBall = roll >= 96; }
            else { scored = roll < 28 ? 0 : roll < 43 ? 1 : roll < 55 ? 2 : roll < 76 ? 4 : roll < 94 ? 6 : 0; wicketThisBall = roll >= 94; }
            if (wicketThisBall) scored = 0;
            if (wicketThisBall) wickets++;
            runs += scored; balls++;
            if (ball != null) Destroy(ball);
            ball = null;
            commentary = wicketThisBall
                ? "WICKET! The bowler has the batter." : scored == 6 ? "SIX! Launched over the boundary!" : scored == 4 ? "FOUR! Crunched into the gap!" : scored == 0 ? "Dot ball. Pick your next shot." : scored + " run" + (scored == 1 ? "." : "s.");
            // Reset this innings' wickets event marker; wicket count is cumulative as intended.
            ballInFlight = false;
            if (balls >= BallsPerInnings || wickets >= 3) FinishMatch();
        }

        private void FinishMatch()
        {
            if (finished) return;
            finished = true;
            opponentRuns = Mathf.Max(20, Random.Range(35, 76)); opponentWickets = Random.Range(1, 4);
            bool won = runs > opponentRuns;
            commentary = string.Format("{0}: {1}/{2}. {3} scored {4}/{5}. {6}", playerTeam, runs, wickets, opponent, opponentRuns, opponentWickets, won ? "You win!" : runs == opponentRuns ? "Match tied!" : "The opposition wins.");
            var result = new MatchResult();
            result.matchId = System.Guid.NewGuid().ToString(); result.homeTeamName = playerTeam; result.awayTeamName = opponent;
            result.winnerTeamName = won ? playerTeam : runs == opponentRuns ? "Tie" : opponent;
            result.isPlayerVictory = won; result.resultDescription = commentary; result.manOfTheMatch = "You";
            result.firstInnings.battingTeamName = playerTeam; result.firstInnings.bowlingTeamName = opponent;
            result.firstInnings.totalRuns = runs; result.firstInnings.wicketsLost = wickets; result.firstInnings.oversCompleted = balls / 6f;
            result.secondInnings.battingTeamName = opponent; result.secondInnings.bowlingTeamName = playerTeam;
            result.secondInnings.totalRuns = opponentRuns; result.secondInnings.wicketsLost = opponentWickets; result.secondInnings.oversCompleted = 2;
            result.userPerformance.playerName = CareerMatchLauncher.Instance != null && CareerMatchLauncher.Instance.ActiveContext != null && CareerMatchLauncher.Instance.ActiveContext.playerProfile != null
                ? CareerMatchLauncher.Instance.ActiveContext.playerProfile.name : "Player";
            result.userPerformance.runs = runs; result.userPerformance.balls = balls; result.userPerformance.isOut = wickets > 0;
            pendingResult = result;
            commentary += " Tap CONTINUE to see your career result.";
        }
    }
}
