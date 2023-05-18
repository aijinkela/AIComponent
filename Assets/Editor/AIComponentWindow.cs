using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace AIComponent
{

    public sealed class AIComponentWindow : EditorWindow
    {
        #region Script generator
        static string WrapPrompt(string input)
      => "Write a Unity Compnent script.\n" +
         " - I only need the script body. Don’t add any explanation.\n" +
         " - You should add using statement at the top of the code to ensure that the code compilation will not report errors. \n" +
         " - reply as the follow format .\n" +
         "XXX(The Script Name)\n" +
         "------------------- \n" +
         "XXX(The Script Content)\n" +
         "The Component is described as follows:\n" + input;

        public void Generator()
        {
            var warp = WrapPrompt(_prompt);
            var text = OpenAIUtil.InvokeChat(warp);
            var splits = text.Split("-------------------");
            var scriptName = splits[0].Replace("\n", "");
            var scriptContent = splits[1].Replace("```", "").Replace("csharp", "");
            var filename = "Assets/AIScripts/" + scriptName + ".cs";
            CreateScriptAsset(filename, scriptContent);
            BindScriptAssetSync(filename);
        }


        void CreateScriptAsset(string file, string code)
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            var script = method.Invoke(null, new object[] { file, code });
            Debug.Log(script);
        }

        private void BindScriptAssetSync(string file)
        {
            AssetDatabase.ImportAsset(file);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(file);
            Selection.activeGameObject.AddComponent(script.GetClass());
            EditorUtility.SetDirty(Selection.activeGameObject);
        }

        #endregion

        #region Editor GUI

        string _prompt = "random rotate gameObject";

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
