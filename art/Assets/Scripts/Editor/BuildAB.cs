using UnityEngine;
using UnityEditor;
using System.IO;

namespace G9ZModelEditor
{
    public class BuildWindow : EditorWindow
    {

        #region

        [MenuItem("Engine/Assets/BuildAB - 打包 windows")]
        static void BuildAssetBundlePc()
        {
            BuildSetting("win", BuildTarget.StandaloneWindows);
        }


        [MenuItem("Engine/Assets/BuildAB - 打包 Android")]
        static void BuildAssetBundleAndroid()
        {
            BuildSetting("android", BuildTarget.Android);
        }

        static private void BuildSetting(string ext, BuildTarget buildTarget)
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            foreach (var one in files)
            {
                string fileExt = Path.GetExtension(one);
                switch (fileExt)
                {
                    case ".meta":
                    case ".dll":
                        continue;
                }

                if (one.Contains("Assets\\res\\"))
                    continue;

                if (one.Contains(Application.streamingAssetsPath))
                    continue;

                string filePath = "Assets" + one.Replace(Application.dataPath, "");
                Debug.Log(filePath);

                AssetImporter importer = AssetImporter.GetAtPath(filePath);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    importer.assetBundleVariant = ext;
                    Debug.Log("[" + importer.assetBundleName + importer.assetBundleVariant + "] " + one.ToString());
                }
            }

            string outPath = Path.Combine(Application.streamingAssetsPath, "assetbundle" + "." + ext);
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }

            BuildPipeline.BuildAssetBundles(outPath, BuildAssetBundleOptions.None, buildTarget);
            EditorUtility.DisplayDialog("输出信息", "输出完成", "确定");
        }

        #endregion

        private static BuildWindow _wnd = null;

        [MenuItem("Engine/Assets/BuildAB - 查看选中文件的依赖")]
        private static void Create()
        {
            if (_wnd == null)
            {
                _wnd = EditorWindow.GetWindow(typeof(BuildWindow), false, "Build") as BuildWindow;
            }

            if (_wnd != null)
                _wnd.Show();
        }

        private bool _showUIList = false, _showUIDependencesList = false;
        private Vector2 _scrollPosition, _scrollDepPosition;
        private string _outputPath = "";
        private BuildTarget _buildTarget = BuildTarget.StandaloneWindows;

        public BuildWindow()
        {
            _outputPath = Path.Combine(Application.streamingAssetsPath, "AssetBundle");
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
        }

        private void OnGUI()
        {

            _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("输出平台", _buildTarget);

            EditorGUILayout.LabelField("总选中对象", Selection.objects != null ? Selection.objects.Length.ToString() : "null");
            _showUIList = EditorGUILayout.Foldout(_showUIList, "选中的文件");
            if (_showUIList)
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                for (int i = 0; i < Selection.objects.Length; i++)
                {
                    EditorGUILayout.LabelField(string.Format("{0}", AssetDatabase.GetAssetOrScenePath(Selection.objects[i])));
                }
                EditorGUILayout.EndScrollView();
            }


            EditorGUILayout.LabelField("当前选中", Selection.activeObject ? AssetDatabase.GetAssetOrScenePath(Selection.activeObject) : "");
            _showUIDependencesList = EditorGUILayout.Foldout(_showUIDependencesList, "当前选中对象 的 依赖对象");

            if (_showUIDependencesList)
            {
                _scrollDepPosition = EditorGUILayout.BeginScrollView(_scrollDepPosition);
                string[] dependences = AssetDatabase.GetDependencies(new string[] { AssetDatabase.GetAssetOrScenePath(Selection.activeObject) });
                for (int i = 0; i < dependences.Length; i++)
                {
                    EditorGUILayout.LabelField(dependences[i]);
                }
                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("保存 依赖对象 到 输出目录"))
            {
                SaveDependencesToFile(AssetDatabase.GetAssetOrScenePath(Selection.activeObject));
            }

            //if ( GUILayout.Button( "Build" ) ) {
            //    if ( Selection.activeObject ) {
            //        Build( Selection.objects );
            //    } else {
            //        EditorUtility.DisplayDialog( "Error", string.Format( "没有选中物体" ), "确定" );
            //    }
            //}
        }

        void Build(Object[] objs)
        {
            if (objs == null || objs.Length < 1)
            {
                EditorUtility.DisplayDialog("Error", string.Format("没有选中物体"), "确定");
                return;
            }

            AssetBundleBuild[] buildMap = new AssetBundleBuild[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                BuildTarge(objs[i], _buildTarget, ref buildMap[i]);
            }

            BuildPipeline.BuildAssetBundles(_outputPath, buildMap, BuildAssetBundleOptions.DeterministicAssetBundle, _buildTarget);
        }

        void BuildTarge(Object obj, BuildTarget bTarget, ref AssetBundleBuild abb)
        {
            string suffix = "";
            switch (bTarget)
            {
                case BuildTarget.Android:
                    suffix = "android";
                    break;
            }

            // 打包的资源包名称
            abb.assetBundleVariant = suffix;
            abb.assetBundleName = obj.name;

            // 定义字符串，用来记录此资源包文件名称
            string[] resourcesAssets = new string[1];
            resourcesAssets[0] = AssetDatabase.GetAssetPath(obj);

            // 将资源名称数组赋给AssetBuild   
            abb.assetNames = resourcesAssets;
        }

        private void SaveDependencesToFile(string objPath)
        {
            string[] dependences = AssetDatabase.GetDependencies(new string[] { objPath });
            using (StreamWriter sw = new StreamWriter(_outputPath + "/dependences.txt"))
            {
                for (int i = 0; i < dependences.Length; i++)
                {
                    sw.WriteLine(dependences[i]);
                }
                sw.Flush();
                sw.Close();
            }
        }
    }
}