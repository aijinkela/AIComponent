using UnityEngine;
using UnityEditor;

namespace AIComponent {

[FilePath("UserSettings/AIComponentSettings.asset",
          FilePathAttribute.Location.ProjectFolder)]
public sealed class AIComponentSettings : ScriptableSingleton<AIComponentSettings>
{
    public string apiKey = null;
    public string chatAPIBaseUrl =  "https://api.openai.com";
    public string shapeAPIBaseUrl = "http://127.0.0.1:8000";
    public int timeout = 0;
    public void Save() => Save(true);
    void OnDisable() => Save();
}

sealed class AIComponentSettingsProvider : SettingsProvider
{
    public AIComponentSettingsProvider()
      : base("Project/AI Component", SettingsScope.Project) {}

    public override void OnGUI(string search)
    {
        var settings = AIComponentSettings.instance;

        var key = settings.apiKey;
        var chatAPIBaseUrl = settings.chatAPIBaseUrl;
        var shapeAPIBaseUrl = settings.shapeAPIBaseUrl;
        var timeout = settings.timeout;

        EditorGUI.BeginChangeCheck();

        key = EditorGUILayout.TextField("API Key", key);
        chatAPIBaseUrl = EditorGUILayout.TextField("Chat API Base Url", chatAPIBaseUrl);
        shapeAPIBaseUrl = EditorGUILayout.TextField("Shap-E API Base Url", shapeAPIBaseUrl);
        timeout = EditorGUILayout.IntField("Timeout", timeout);

        if (EditorGUI.EndChangeCheck())
        {
            settings.apiKey = key;
            settings.chatAPIBaseUrl = chatAPIBaseUrl;
            settings.shapeAPIBaseUrl = shapeAPIBaseUrl;
            settings.timeout = timeout;
            settings.Save();
        }
    }

    [SettingsProvider]
    public static SettingsProvider CreateCustomSettingsProvider()
      => new AIComponentSettingsProvider();
}

} // namespace AIComponent
