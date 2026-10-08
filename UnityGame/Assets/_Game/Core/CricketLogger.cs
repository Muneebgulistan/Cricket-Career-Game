using System;
using UnityEngine;

namespace CricketGame.Core
{
    /// <summary>
    /// Static logger wrapper for Cricket Career Game.
    /// Standard Log and LogWarning calls are compiled only in Editor or Debug builds
    /// to eliminate garbage allocations and console log overhead in production mobile builds.
    /// LogError always passes through for critical runtime exception tracking.
    /// </summary>
    public static class CricketLogger
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        public static void LogException(Exception exception)
        {
            Debug.LogError(exception);
        }
    }
}
