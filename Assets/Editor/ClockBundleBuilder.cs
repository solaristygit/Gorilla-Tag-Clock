#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;

public static class ClockBundleBuilder
{
    [MenuItem("GorillaTagClock/Build Clock Bundle")]
    public static void BuildClockBundle()
    {
        string outputDirectory =
            Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                "Build"
            );

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        GameObject clock =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Clock/GorillaTagClock.fbx"
            );

        if (clock == null)
        {
            Debug.LogError(
                "[GorillaTagClock] Could not find " +
                "Assets/Clock/GorillaTagClock.fbx"
            );

            return;
        }

        AssetBundleBuild build =
            new AssetBundleBuild();

        build.assetBundleName =
            "gorillatagclock";

        build.assetNames =
            new[]
            {
                "Assets/Clock/GorillaTagClock.fbx"
            };

        BuildPipeline.BuildAssetBundles(
            outputDirectory,
            new[]
            {
                build
            },
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );

        Debug.Log(
            "[GorillaTagClock] AssetBundle built at: " +
            outputDirectory
        );
    }
}

#endif
