using System;
using UnityEditor;
using UnityEngine;
using CricketGame.Core;
using CricketGame.Testing;

/// <summary>Batch-mode entry point for the release-candidate regression suite.</summary>
internal static class ReleaseCandidateTestRunner
{
    public static void RunAll()
    {
        try
        {
            string report;
            bool passed = Step25_27Tests.RunAll(out report);
            CricketLogger.Log(report);
            EditorApplication.Exit(passed ? 0 : 1);
        }
        catch (Exception exception)
        {
            CricketLogger.LogException(exception);
            EditorApplication.Exit(1);
        }
    }
}
