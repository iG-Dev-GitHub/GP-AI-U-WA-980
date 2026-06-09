#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using static LoadingCanvasAI.LoadingCanvasTextGeneratorSettings;

namespace LoadingCanvasAI
{
    [CustomEditor(typeof(LoadingCanvasTextGeneratorSettings), true)]
    public class LoadingCanvasTextGeneratorEditor : Editor
    {
        LoadingCanvasTextGeneratorSettings Settings => (LoadingCanvasTextGeneratorSettings)target;

        const string RootStartsWith = "LoadingCanvas";
        const string NotificationsView = "NotificationsView";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("Generate neutral, project-unique texts for LoadingCanvas (Title/Description/Later) via OpenAI, then apply to prefabs and scenes.", MessageType.Info);
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Run ALL (Scenes + Build Scenes + Prefabs)"))
            {
                RunAll();
            }
            //if (GUILayout.Button("Run for Open Scenes"))
            //{
            //    RunForOpenScenes();
            //}
            //if (GUILayout.Button("Run for Build Scenes"))
            //{
            //    RunForAllBuildScenes();
            //}
            //if (GUILayout.Button("Run for Prefabs"))
            //{
            //    RunForPrefabs();
            //}
        }

        // ========== MAIN RUNNERS ==========

        async void RunAll()
        {
            var result = await Settings.GenerateAsync("", "", "", "");
            if (!result.ok)
            {
                Debug.LogError($"AI generation failed: {result.error}");
                return;
            }

            RunForOpenScenes(result.data);
            RunForAllBuildScenes(result.data);
            RunForPrefabs(result.data);
            Debug.Log("Run ALL finished");
        }

        async void RunForOpenScenes()
        {
            var result = await Settings.GenerateAsync("", "", "", "");
            if (!result.ok) { Debug.LogError(result.error); return; }
            RunForOpenScenes(result.data);
        }

        void RunForOpenScenes(GeneratedTriple data)
        {
            var targets = FindTargetsInOpenScenes();
            ApplyGeneratedTexts(targets, data, saveScenes: true);
        }

        async void RunForAllBuildScenes()
        {
            var result = await Settings.GenerateAsync("", "", "", "");
            if (!result.ok) { Debug.LogError(result.error); return; }
            RunForAllBuildScenes(result.data);
        }

        void RunForAllBuildScenes(GeneratedTriple data)
        {
            var paths = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();
            if (paths.Count == 0) return;

            var currentScene = SceneManager.GetActiveScene().path;

            foreach (var path in paths)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var targets = FindTargetsInScene(scene);
                ApplyGeneratedTexts(targets, data, saveScenes: true);
            }

            if (!string.IsNullOrEmpty(currentScene))
                EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }

        async void RunForPrefabs()
        {
            var result = await Settings.GenerateAsync("", "", "", "");
            if (!result.ok) { Debug.LogError(result.error); return; }
            RunForPrefabs(result.data);
        }

        void RunForPrefabs(GeneratedTriple data)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab " + RootStartsWith);
            int changed = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                if (!go.name.StartsWith(RootStartsWith, StringComparison.OrdinalIgnoreCase)) continue;

                var target = BuildTargetFromRoot(go, path, isPrefab: true);
                if (target != null)
                {
                    if (ApplyGeneratedTexts(new List<LC_Target> { target }, data, saveScenes: false, isPrefab: true))
                    {
                        changed++;
                        EditorUtility.SetDirty(go);
                    }
                }
            }
            if (changed > 0) AssetDatabase.SaveAssets();
            Debug.Log($"Prefabs processed: {changed}");
        }

        // ========== CORE APPLY ==========

        bool ApplyGeneratedTexts(List<LC_Target> targets, GeneratedTriple data, bool saveScenes, bool isPrefab = false)
        {
            if (targets == null || targets.Count == 0) return false;

            int success = 0;
            foreach (var t in targets)
            {
                if (t.root == null) continue;

                Undo.RegisterFullObjectHierarchyUndo(t.root, "Apply LoadingCanvas AI Texts");

                if (t.title) t.title.text = SafeTrim(data.title, 60);
                if (t.description) t.description.text = SafeTrim(data.description, 160);
                if (t.later) t.later.text = SafeTrim(data.later, 40);
                if (t.allow) t.allow.text = SafeTrim(data.allow, 40);

                EditorUtility.SetDirty(t.root);
                success++;
            }

            if (saveScenes)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded && scene.IsValid())
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
            }

            return success > 0;
        }

        // ========== TARGET FINDERS ==========

        List<LC_Target> FindTargetsInOpenScenes()
        {
            var list = new List<LC_Target>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                list.AddRange(FindTargetsInScene(scene));
            }
            return list;
        }

        List<LC_Target> FindTargetsInScene(Scene scene)
        {
            var list = new List<LC_Target>();
            if (!scene.isLoaded) return list;
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root.name.StartsWith(RootStartsWith, StringComparison.OrdinalIgnoreCase) || root.GetComponentInChildren<StartupLoadingController>(true) != null)
                {
                    var t = BuildTargetFromRoot(root, scene.path, isPrefab: false);
                    if (t != null) list.Add(t);
                }
            }
            return list;
        }

        // ========== HELPERS ==========

        class LC_Target
        {
            public GameObject root;
            public TMP_Text title;
            public TMP_Text description;
            public TMP_Text later;
            public TMP_Text allow;
            public string contextPath;
            public bool isPrefab;
        }

        LC_Target BuildTargetFromRoot(GameObject root, string contextPath, bool isPrefab)
        {
            Transform notif = root.transform.Find(NotificationsView);
            if (notif == null)
            {
                notif = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.Equals(NotificationsView, StringComparison.OrdinalIgnoreCase));
            }
            if (notif == null) return null;

            TMP_Text title = FindTMP(notif, "Title_Text");
            TMP_Text description = FindTMP(notif, "Description_Text");
            TMP_Text later = FindTMP(notif, "LaterNotificationst/Text (TMP)")
                             ?? FindTMP(notif, "LaterNotifications/Text (TMP)")
                             ?? FindTMPLoose(notif, "Later");
            TMP_Text allow = FindTMP(notif, "AllowNotifications/Text (TMP)")
                             ?? FindTMPLoose(notif, "Allow");

            if (title == null || description == null || later == null || allow == null)
                return null;

            return new LC_Target
            {
                root = root,
                title = title,
                description = description,
                later = later,
                allow = allow,
                contextPath = contextPath,
                isPrefab = isPrefab
            };
        }

        TMP_Text FindTMP(Transform root, string relativePath)
        {
            var t = root.Find(relativePath);
            return t ? t.GetComponent<TMP_Text>() : null;
        }

        TMP_Text FindTMPLoose(Transform root, string nameContains)
        {
            return root.GetComponentsInChildren<TMP_Text>(true)
                       .FirstOrDefault(x => x.name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        string SafeTrim(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
#endif
