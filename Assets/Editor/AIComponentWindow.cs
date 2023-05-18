using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.EditorCoroutines.Editor;

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

        public void Generator()
        {
            Debug.Log("prompt: " + _prompt);
            var warp = WrapPrompt(_prompt);
            var text = OpenAIUtil.InvokeChat(warp);
            Debug.Log("chatgpt resp: ");
            Debug.Log(text);
            var scriptName = GetClassName(text);
            var scriptContent = text;
            var filename = "Assets/AIScripts/" + scriptName + ".cs";
            CreateScriptAsset(filename, scriptContent);
            // EditorCoroutineUtility.StartCoroutine(BindScriptAsset(filename, scriptName), this);  //some times not work 
            BindScriptAsset(filename, scriptName);
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

        public static Type GetAssemblyType(string typeName) {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
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
        private void Update() {
            if(tmpScript != null){
                Type classType = tmpScript.GetClass();
                if (classType != null && classType.BaseType == typeof(MonoBehaviour))
                {
                    if(Selection.activeGameObject != null){
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
            if(Selection.activeGameObject != null){
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
                if (GUILayout.Button("Run")) Generator();
            }
            else
            {
                EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
            }
        }

        #endregion
    }

} // namespace AIComponent
