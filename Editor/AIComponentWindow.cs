using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIComponent
{

    public sealed class AIComponentWindow : EditorWindow
    {
        #region Script generator
        static string WrapPrompt(string input)
      => "Write a Unity Compnent script.\n" +
         " - I only need the script body. Don’t add any explanation.\n" +
         " - You should add using statement at the top of the code to ensure that the code compilation will not report errors. \n" +
         "The Component is described as follows:\n" + input;

        public void GeneratorScript()
        {
            Debug.Log("prompt: " + _prompt);
            var warp = WrapPrompt(_prompt);
            var text = AIUtil.InvokeChat(warp);
            Debug.Log("chatgpt resp: ");
            Debug.Log(text);
            var scriptName = GetClassName(text);
            var scriptContent = text;
            var filename = "Assets/AIScripts/" + scriptName + ".cs";
            CreateScriptAsset(filename, scriptContent);
            // EditorCoroutineUtility.StartCoroutine(BindScriptAsset(filename, scriptName), this);  //some times not work 
            BindScriptAsset(filename, scriptName);
        }

        public void GeneratorShape()
        {
            Debug.Log("prompt: " + _prompt);
            var downloadUrl = AIUtil.InvokeShapE(_prompt);
            var filename = FormatFileName(_prompt);
            var filepath = "Assets/AIShapes/" + filename + ".blend";
            DownloadFile(downloadUrl, filepath);
            GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(filepath);
            if (prefabObj != null)
            {
                PrefabUtility.InstantiatePrefab(prefabObj);
                Debug.Log("Prefab added to scene!");
            }
        }

        public static string FormatFileName(string input)
        {
            // 将非法字符替换为空格
            string output = Regex.Replace(input, @"[^\w\.-]", " ");

            // 删除多余的空格并将其替换为下划线
            output = Regex.Replace(output, @"\s+", "_");

            // 去除文件名中末尾的点号和空格
            output = output.TrimEnd(new char[] { '.', ' ' });

            return output;
        }

        private void EnsureDirectoryExists(string folderPath){
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private void DownloadFile(string url, string file)
        {
            using (WebClient client = new WebClient())
            {
                byte[] data = client.DownloadData(url);  // 下载远端文件数据

                string fullPath = Path.GetFullPath(file);
                EnsureDirectoryExists(Path.GetDirectoryName(fullPath));
                // 写入数据到本地文件
                File.WriteAllBytes(fullPath, data);

                AssetDatabase.Refresh();  // 刷新资产目录

                Debug.Log("Downloaded " + url + " to " + fullPath);
            }
        }

        private string GetClassName(string code)
        {
            string className = null;
            Regex regex = new Regex(@"(?:^|\s+)class\s+([a-zA-Z_][a-zA-Z0-9_]*)");
            Match match = regex.Match(code);

            if (match.Success)
            {
                className = match.Groups[1].Value;
            }

            return className;
        }

        public static Type GetAssemblyType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null) return type;
            }
            return null;
        }


        void CreateScriptAsset(string file, string code)
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            var script = method.Invoke(null, new object[] { file, code });

            string fullPath = Path.GetFullPath(file);
            File.WriteAllText(fullPath, code);
        }

        private MonoScript tmpScript;
        private void Update()
        {
            if (tmpScript != null)
            {
                Type classType = tmpScript.GetClass();
                if (classType != null && classType.BaseType == typeof(MonoBehaviour))
                {
                    if (Selection.activeGameObject != null)
                    {
                        Selection.activeGameObject.AddComponent(classType);
                    }
                    tmpScript = null;
                }
            }
        }

        private void BindScriptAsset(string file, string scriptName)
        {
            AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(file);
            if (Selection.activeGameObject != null)
            {
                tmpScript = script;
            }
        }

        #endregion

        #region Editor GUI

        string _prompt = "random rotate";

        const string ApiKeyErrorText =
          "API Key hasn't been set. Please check the project settings " +
          "(Edit > Project Settings > AI Component > API Key).";

        bool IsApiKeyOk
          => !string.IsNullOrEmpty(AIComponentSettings.instance.apiKey);

        [MenuItem("Window/AI Component")]
        static void Init() => GetWindow<AIComponentWindow>(true, "Add AI Component");

        void OnGUI()
        {
            if (IsApiKeyOk)
            {
                _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Script")) GeneratorScript();
                if (GUILayout.Button("Shape")) GeneratorShape();
            }
            else
            {
                EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
            }
        }

        #endregion
    }

} // namespace AIComponent
