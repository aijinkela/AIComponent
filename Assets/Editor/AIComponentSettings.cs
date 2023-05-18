using UnityEngine;
using UnityEditor;

namespace AIComponent {

[FilePath("UserSettings/AIComponentSettings.asset",
          FilePathAttribute.Location.ProjectFolder)]
public sealed class AIComponentSettings : ScriptableSingleton<AIComponentSettings>
{
    public string apiKey = null;
    public string apiBase =  "https://api.openai.com";
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
        var apiBase = settings.apiBase;
        var timeout = settings.timeout;

        EditorGUI.BeginChangeCheck();

        key = EditorGUILayout.TextField("API Key", key);
        apiBase = EditorGUILayout.TextField("API Base Url", apiBase);
        timeout = EditorGUILayout.IntField("Timeout", timeout);

        if (EditorGUI.EndChangeCheck())
        {
            settings.apiKey = key;
            settings.apiBase = apiBase;
            settings.timeout = timeout;
            settings.Save();
        }
    }

    [SettingsProvider]
    public static SettingsProvider CreateCustomSettingsProvider()
      => new AIComponentSettingsProvider();
}

} // namespace AIComponent
