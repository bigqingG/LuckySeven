using System;
using System.IO;
using LuckySeven.Config;
using LuckySeven.Game;
using LuckySeven.Presentation;
using LuckySeven.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace LuckySeven.Editor
{
    public static class LuckySevenProjectTools
    {
        public const string ScenePath = "Assets/_LuckySeven/Scenes/Main.unity";
        public const string ConfigPath = "Assets/_LuckySeven/Config/GameConfig.asset";

        [MenuItem("Lucky Seven/Create Initial Main Scene")]
        public static void Setup()
        {
            if (File.Exists(ScenePath)) {
                if (!File.Exists("Backups/V1BeforeCards/_LuckySeven/Scenes/Main.unity")) throw new InvalidOperationException("V1 scene backup required before card migration.");
                for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save modified scenes before migration.");
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            }
            if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
            {
                TMP_PackageResourceImporter.ImportResources(true, false, false);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null) throw new InvalidOperationException("TMP resources are still importing. Run scene creation again after import.");
            Directory.CreateDirectory("Assets/_LuckySeven/Config"); Directory.CreateDirectory("Assets/_LuckySeven/Scenes");
            AssetDatabase.Refresh();
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config == null) { config = ScriptableObject.CreateInstance<GameConfig>(); AssetDatabase.CreateAsset(config, ConfigPath); }
            config.ApplyCardDefaults(); EditorUtility.SetDirty(config);
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            var camera = new GameObject("UICamera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(15, 25, 34, 255); camera.orthographic = true; camera.cullingMask = 0;
            var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var root = new GameObject("DesignRoot", typeof(RectTransform), typeof(BattleHUDView));
            var rect = root.GetComponent<RectTransform>(); rect.SetParent(canvas.transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(1280, 720);
            var view = root.GetComponent<BattleHUDView>(); view.BuildLayout(font);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var boot = new GameObject("Bootstrap", typeof(GameFlowController), typeof(BattleFxPlayer), typeof(AudioSource));
            var audio = boot.GetComponent<AudioSource>(); audio.playOnAwake = false; audio.spatialBlend = 0;
            var effects = boot.GetComponent<BattleFxPlayer>(); effects.Configure(view.Feedback, view.EnemyVisual, audio);
            boot.GetComponent<GameFlowController>().Configure(config, view, effects);
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            foreach (var existing in EditorBuildSettings.scenes)
                if (existing.path != ScenePath) scenes.Add(existing);
            EditorBuildSettings.scenes = scenes.ToArray();
            ConfigureWeb();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Lucky Seven V2 card scene migrated; V1 backup preserved.");
        }

        [MenuItem("Lucky Seven/Apply Table Card Layout")]
        public static void TableLayout()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before migration.");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save modified scenes before migration.");
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var view=UnityEngine.Object.FindFirstObjectByType<BattleHUDView>();
            if(view==null)throw new InvalidOperationException("Main HUD missing.");
            view.ConfigureTableLayout();EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }

        [MenuItem("Lucky Seven/Open Main Scene")]
        public static void OpenMain()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Lucky Seven/Configure Web")]
        public static void ConfigureWeb()
        {
            PlayerSettings.productName = "Lucky Seven";
            PlayerSettings.defaultWebScreenWidth = 1280; PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.runInBackground = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "PROJECT:LuckySeven";
        }

        public static void RestartForWeb()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save the modified scene before reloading platform support.");
            EditorApplication.OpenProject(Path.GetFullPath("."), "-buildTarget", "WebGL", "-executeMethod", "LuckySeven.Editor.LuckySevenProjectTools.BuildWeb", "-logFile", Path.GetFullPath("Docs/Validation/WebEditor.log"));
        }

        [MenuItem("Lucky Seven/Build Web Prototype")]
        public static void BuildWeb()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL)) throw new InvalidOperationException("WebGL module unavailable.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                SessionState.SetBool("LuckySeven.PendingWebBuild", true);
                if (!EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                {
                    SessionState.EraseBool("LuckySeven.PendingWebBuild");
                    throw new InvalidOperationException("Could not switch to WebGL.");
                }
                return;
            }
            ConfigureWeb();
            UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent(out var contentResult);
            if(!string.IsNullOrEmpty(contentResult.Error))throw new InvalidOperationException(contentResult.Error);
            Directory.CreateDirectory("Builds/WebGLV2"); Directory.CreateDirectory("Docs/Validation");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = "Builds/WebGLV2", target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            Directory.CreateDirectory("Docs/Validation/ResourceComboV1");
            File.WriteAllText("Docs/Validation/ResourceComboV1/web-build.txt", $"Result: {report.summary.result}\nPlatform: {report.summary.platform}\nSize: {report.summary.totalSize}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nDuration: {report.summary.totalTime}\nOutput: {report.summary.outputPath}\n");
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0
                || !File.Exists("Builds/WebGLV2/index.html") || Directory.GetFiles("Builds/WebGLV2", "*.wasm*", SearchOption.AllDirectories).Length == 0)
                throw new InvalidOperationException("Web build failed or artifacts are missing. Inspect Editor.log and web-build.txt.");
        }
    }
}
