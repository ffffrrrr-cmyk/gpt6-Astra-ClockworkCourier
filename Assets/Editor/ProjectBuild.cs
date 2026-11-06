using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ClockworkCourier.Editor
{
    public static class ProjectBuild
    {
        [MenuItem("Clockwork Courier/Rebuild scene and generated assets")]
        public static void Generate()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Generated");
            Directory.CreateDirectory("Assets/Prefabs");
            Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.Refresh();
            if (AssetDatabase.LoadAssetAtPath<GameTuning>("Assets/Resources/GameTuning.asset") == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameTuning>(), "Assets/Resources/GameTuning.asset");
            ConfigurePipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            WorldBuilder.EnsurePresentation();
            var world = WorldBuilder.Build(new ProgressData());
            Physics.SyncTransforms();
            new GameObject("Game Session").AddComponent<GameSession>();
            var assets = new HashSet<UnityEngine.Object>();
            int counter = 0;
            foreach (var filter in world.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = filter.sharedMesh;
                if (mesh == null || !assets.Add(mesh) || AssetDatabase.Contains(mesh)) continue;
                AssetDatabase.CreateAsset(mesh, "Assets/Generated/Mesh-" + counter++ + ".asset");
            }
            foreach (var renderer in world.GetComponentsInChildren<Renderer>(true))
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || !assets.Add(material) || AssetDatabase.Contains(material)) continue;
                    var texture = material.GetTexture("_BaseMap");
                    if (texture != null && !AssetDatabase.Contains(texture)) AssetDatabase.CreateAsset(texture, "Assets/Generated/Finish-" + counter + ".asset");
                    AssetDatabase.CreateAsset(material, "Assets/Generated/Material-" + counter++ + ".mat");
                }
            var hero = world.GetComponentInChildren<CourierController>();
            PrefabUtility.SaveAsPrefabAsset(hero.gameObject, "Assets/Prefabs/Courier.prefab");
            var exportedKinds = new HashSet<EnemyKind>();
            foreach (var enemy in world.GetComponentsInChildren<EnemyActor>())
            {
                string path = "Assets/Prefabs/" + enemy.kind + ".prefab";
                if (exportedKinds.Add(enemy.kind)) PrefabUtility.SaveAsPrefabAsset(enemy.gameObject, path);
            }
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/ClockworkCourier.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/ClockworkCourier.unity", true) };
            PlayerSettings.companyName = "ClockworkCourier";
            PlayerSettings.productName = "U2";
            PlayerSettings.defaultScreenWidth = 1440;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            QualitySettings.vSyncCount = 1;
            QualitySettings.shadowDistance = 40;
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            AssetDatabase.SaveAssets();
            Debug.Log("U2_GENERATED: scene, meshes, materials and prefabs saved.");
        }

        private static void ConfigurePipeline()
        {
            string rendererPath = "Assets/Generated/CourierRenderer.asset";
            string pipelinePath = "Assets/Generated/CourierPipeline.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            pipeline.renderScale = 1;
            pipeline.msaaSampleCount = 2;
            pipeline.shadowDistance = 40;
            pipeline.mainLightShadowmapResolution = 4096;
            pipeline.shadowCascadeCount = 4;
            var pipelineSettings = new SerializedObject(pipeline);
            pipelineSettings.FindProperty("m_SoftShadowsSupported").boolValue = true;
            pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        [MenuItem("Clockwork Courier/Build Windows")]
        public static void BuildWindows()
        {
            Generate();
            RuleChecks.Run();
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/ClockworkCourier.unity" },
                locationPathName = "Builds/Windows/ClockworkCourier.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            Directory.CreateDirectory("Validation");
            File.WriteAllText("Validation/build-result.json", JsonUtility.ToJson(new BuildResult
            {
                result = report.summary.result.ToString(),
                errors = (int)report.summary.totalErrors,
                warnings = (int)report.summary.totalWarnings,
                bytes = report.summary.totalSize.ToString(),
                unity = Application.unityVersion,
                timestamp = DateTime.UtcNow.ToString("O")
            }, true));
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("U2 Windows build failed");
            Debug.Log("U2_BUILD_SUCCEEDED");
        }

        [Serializable]
        private sealed class BuildResult
        {
            public string result;
            public int errors;
            public int warnings;
            public string bytes;
            public string unity;
            public string timestamp;
        }
    }
}
