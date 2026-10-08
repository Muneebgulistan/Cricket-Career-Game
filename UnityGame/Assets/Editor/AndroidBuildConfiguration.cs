using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Keeps the Android release target reproducible when the project is opened.
/// Texture compression targeting requires an AAB/Gradle bundle build.
/// </summary>
[InitializeOnLoad]
internal static class AndroidBuildConfiguration
{
    static AndroidBuildConfiguration()
    {
        EditorApplication.delayCall += ApplySettings;
    }

    private static void ApplySettings()
    {
        bool changed = false;
        changed |= PlayerSettings.productName != CricketGame.Core.GameConstants.AppName;
        changed |= PlayerSettings.bundleVersion != CricketGame.Core.GameConstants.AppVersion;
        changed |= PlayerSettings.Android.bundleVersionCode != CricketGame.Core.GameConstants.BuildNumber;
        changed |= PlayerSettings.Android.minSdkVersion != (AndroidSdkVersions)CricketGame.Core.GameConstants.MinimumApiLevel;
        changed |= PlayerSettings.Android.targetSdkVersion != (AndroidSdkVersions)CricketGame.Core.GameConstants.TargetApiLevel;
        changed |= PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64;
        changed |= PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP;

        PlayerSettings.productName = CricketGame.Core.GameConstants.AppName;
        PlayerSettings.bundleVersion = CricketGame.Core.GameConstants.AppVersion;
        PlayerSettings.Android.bundleVersionCode = CricketGame.Core.GameConstants.BuildNumber;
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)CricketGame.Core.GameConstants.MinimumApiLevel;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)CricketGame.Core.GameConstants.TargetApiLevel;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        TextureCompressionFormat[] currentFormats = PlayerSettings.Android.textureCompressionFormats;
        if (currentFormats == null || currentFormats.Length != 1 || currentFormats[0] != TextureCompressionFormat.ASTC)
        {
            PlayerSettings.Android.textureCompressionFormats = new[] { TextureCompressionFormat.ASTC };
            changed = true;
        }
        if (!EditorUserBuildSettings.buildAppBundle)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            changed = true;
        }

        if (changed) CricketGame.Core.CricketLogger.Log("Android release settings configured: API 24–34, ARM64, IL2CPP, ASTC AAB.");
    }
}
