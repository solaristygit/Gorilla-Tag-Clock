#if UNITY_EDITOR

using UnityEditor;
using System.IO;

public static class ClockBundleBuilder
{
    [MenuItem("GorillaTagClock/Build Clock Bundle")]
    public static void Build()
    {
        string output =
            Path.Combine(
                Directory.GetParent(UnityEngine.Application.dataPath).FullName,
                "Build"
            );

        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        BuildPipeline.BuildAssetBundles(
            output,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );

        UnityEngine.Debug.Log(
            "[GorillaTagClock] Bundle built!"
        );
    }
}

#endif
