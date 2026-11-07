using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace LuckySeven.Editor
{
    // Local, project-scoped automation. Only explicit commands in this project's Temp folder run.
    // No network listener or arbitrary code execution; safe to retain as reproducible tooling.
    [InitializeOnLoad]
    public static class LuckySevenValidationRunner
    {
        private const string Request = "Temp/LuckySeven.command";
        private static TestRunnerApi api;
        static LuckySevenValidationRunner()
        {
            EditorApplication.update += Tick;
            // PlayMode reloads the domain. Re-register callbacks and retain mode through SessionState.
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Results());
        }
        private static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (SessionState.GetBool("LuckySeven.PendingWebBuild", false) && EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                SessionState.EraseBool("LuckySeven.PendingWebBuild");
                File.WriteAllText(Request, "build");
            }
            if (!File.Exists(Request)) return;
            string command;
            try { command = File.ReadAllText(Request).Trim(); File.Delete(Request); }
            catch (IOException) { return; }
            try
            {
                switch (command)
                {
                    case "stop-play": EditorApplication.isPlaying = false; break;
                    case "sync-text": LuckySevenPixelTools.SyncText(); break;
                    case "pixel-skin": LuckySevenPixelTools.Apply(); break;
                    case "localize-cards": LuckySevenLocalizationTools.Setup(); break;
                    case "install-localization": UnityEditor.PackageManager.Client.Add("com.unity.localization"); break;
                    case "table-layout": LuckySevenProjectTools.TableLayout(); break;
                    case "setup": LuckySevenProjectTools.Setup(); break;
                    case "edit-tests": RunTests(TestMode.EditMode, "LuckySeven.EditModeTests"); return;
                    case "play-tests": RunTests(TestMode.PlayMode, "LuckySeven.PlayModeTests"); return;
                    case "build": LuckySevenProjectTools.BuildWeb(); break;
                    case "restart-web": LuckySevenProjectTools.RestartForWeb(); return;
                    default: throw new InvalidOperationException("Unknown Lucky Seven command: " + command);
                }
                File.WriteAllText("Docs/Validation/command-result.txt", command + (SessionState.GetBool("LuckySeven.PendingWebBuild", false) ? ": waiting for Web platform switch" : ": completed"));
            }
            catch (Exception exception)
            { File.WriteAllText("Docs/Validation/command-result.txt", command + ": " + exception); Debug.LogException(exception); }
        }
        [MenuItem("Lucky Seven/Run EditMode Tests")]
        public static void EditTests() { RunTests(TestMode.EditMode, "LuckySeven.EditModeTests"); }
        [MenuItem("Lucky Seven/Run PlayMode Tests")]
        public static void PlayTests() { RunTests(TestMode.PlayMode, "LuckySeven.PlayModeTests"); }
        private static void RunTests(TestMode mode, string assembly)
        {
            SessionState.SetString("LuckySeven.TestMode", mode.ToString());
            api.Execute(new ExecutionSettings(new Filter { testMode = mode, assemblyNames = new[] { assembly } }));
        }
        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor tests) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                string mode = SessionState.GetString("LuckySeven.TestMode", "");
                if (string.IsNullOrEmpty(mode)) return;
                SessionState.EraseString("LuckySeven.TestMode");
                Directory.CreateDirectory("Docs/Validation");
                TestRunnerApi.SaveResultToFile(result, "Docs/Validation/ResourceComboV1/" + mode + ".xml");
                File.WriteAllText("Docs/Validation/command-result.txt", $"{mode}: {result.PassCount} passed, {result.FailCount} failed, {result.SkipCount} skipped");
            }
        }
    }
}
