using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ForjaDasMares.Editor
{
    public static class ArenaBuilds
    {
        static string OutputArgument()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-arenaBuildOutput") return Path.GetFullPath(args[i + 1]);
            throw new ArgumentException("Provide -arenaBuildOutput <absolute output path>.");
        }

        public static void Windows()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            Build(BuildTarget.StandaloneWindows64, OutputArgument());
        }

        public static void Android()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
            EditorUserBuildSettings.buildAppBundle = false;
            Build(BuildTarget.Android, OutputArgument());
        }

        static void Build(BuildTarget target, string output)
        {
            if (!File.Exists(ArenaBuilder.ScenePath)) throw new FileNotFoundException(ArenaBuilder.ScenePath);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ArenaBuilder.ScenePath },
                locationPathName = output,
                target = target,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Arena build failed: {report.summary.result}, {report.summary.totalErrors} errors.");
            Debug.Log($"ARENA_PLAYER_BUILD_OK {target} {output}");
        }
    }
}
