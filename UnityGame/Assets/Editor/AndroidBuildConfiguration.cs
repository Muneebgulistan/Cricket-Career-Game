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
        changed |= PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel24;
        changed |= PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel34;
        changed |= PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64;
        changed |= PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP;

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
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

        if (changed) Debug.Log("Android release settings configured: API 24–34, ARM64, IL2CPP, ASTC AAB.");
    }
}
