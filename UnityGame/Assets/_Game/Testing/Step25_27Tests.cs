#if UNITY_EDITOR
using UnityEngine;
using CricketGame.Core;
using CricketGame.Testing;

namespace CricketGame.Testing
{
    /// <summary>
    /// Step 25-27 Release Candidate verification suite.
    /// Tests GameConstants values, CricketLogger functionality, GameLifecycleManager,
    /// and runs MatchSimulationStressTest.
    /// </summary>
    public static class Step25_27Tests
    {
        public static bool VerifyGameConstants(out string message)
        {
            string ver = GameConstants.AppVersion;
            int build = GameConstants.BuildNumber;
            string tag = GameConstants.ReleaseTag;
            int fps = GameConstants.TargetFrameRate;

            if (!string.Equals(ver, "1.0.0"))
            {
                message = "GameConstants AppVersion is not 1.0.0";
                return false;
            }
            if (build != 1)
            {
                message = "GameConstants BuildNumber is not 1";
                return false;
            }
            if (!string.Equals(tag, "v1.0.0-RC1"))
            {
                message = "GameConstants ReleaseTag is not v1.0.0-RC1";
                return false;
            }
            if (fps != 60)
            {
                message = "GameConstants TargetFrameRate is not 60";
                return false;
            }

            message = "GameConstants verified: Version 1.0.0 (Build 1), Release v1.0.0-RC1, 60 FPS.";
            return true;
        }

        public static bool VerifyCricketLogger(out string message)
        {
            try
            {
                CricketLogger.Log("Testing CricketLogger.Log");
                message = "CricketLogger executed the development-only Log wrapper without exception.";
                return true;
            }
            catch (System.Exception ex)
            {
                message = "CricketLogger failed with exception: " + ex.Message;
                return false;
            }
        }

        public static bool VerifyGameLifecycleManager(out string message)
        {
            GameLifecycleManager previousLifecycle = GameLifecycleManager.Instance;
            GameStateManager previousStateManager = GameStateManager.Instance;
            float previousTimeScale = Time.timeScale;
            int previousFrameRate = Application.targetFrameRate;
            int previousVSyncCount = QualitySettings.vSyncCount;
            try
            {
                GameObject stateObject = new GameObject("Test_GameStateManager");
                GameStateManager stateManager = stateObject.AddComponent<GameStateManager>();
                GameStateManager.SetInstanceForTesting(stateManager);
                stateManager.ChangeState(GameState.PlayingMatch);

                GameObject managerObj = new GameObject("Test_LifecycleManager");
                GameLifecycleManager manager = managerObj.AddComponent<GameLifecycleManager>();
                GameLifecycleManager.SetInstanceForTesting(manager);
                manager.ApplyPerformanceSettings();

                manager.OnApplicationPause(true);
                if (!manager.IsPaused || stateManager.CurrentState != GameState.MatchPaused || Time.timeScale != 0f)
                {
                    Object.DestroyImmediate(managerObj);
                    Object.DestroyImmediate(stateObject);
                    message = "Backgrounding did not pause the live match state and clock.";
                    return false;
                }

                manager.OnApplicationFocus(true);
                if (manager.IsPaused || stateManager.CurrentState != GameState.PlayingMatch || Time.timeScale != 1f)
                {
                    Object.DestroyImmediate(managerObj);
                    Object.DestroyImmediate(stateObject);
                    message = "Foregrounding did not resume the lifecycle-paused match.";
                    return false;
                }

                Object.DestroyImmediate(managerObj);
                Object.DestroyImmediate(stateObject);
                message = "Pause and focus lifecycle events paused/resumed the live match.";
                return true;
            }
            catch (System.Exception ex)
            {
                message = "GameLifecycleManager test failed: " + ex.Message;
                return false;
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Application.targetFrameRate = previousFrameRate;
                QualitySettings.vSyncCount = previousVSyncCount;
                GameLifecycleManager.SetInstanceForTesting(previousLifecycle);
                GameStateManager.SetInstanceForTesting(previousStateManager);
            }
        }

        public static bool VerifyMatchSimulationStressTest(out string message)
        {
            StressTestReport report = MatchSimulationStressTest.RunFullT20StressTest(100f);
            message = report.Summary;
            return report.Passed;
        }

        public static bool RunAll(out string message)
        {
            string[] results = new string[4];
            bool constants = VerifyGameConstants(out results[0]);
            bool logger = VerifyCricketLogger(out results[1]);
            bool lifecycle = VerifyGameLifecycleManager(out results[2]);
            bool stress = VerifyMatchSimulationStressTest(out results[3]);
            bool passed = constants && logger && lifecycle && stress;
            message = string.Join("\n", results) + "\nRelease-candidate regression: " + (passed ? "PASS" : "FAIL");
            return passed;
        }
    }
}
#endif
